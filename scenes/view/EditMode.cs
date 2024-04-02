using System;
using System.Collections.Generic;
using Godot;
using Packinator3D.datastructure;
using Packinator3D.scenes.puzzle;

namespace Packinator3D.scenes.view;

internal partial class EditMode : Node3D {
    private const float RayLength = 10;
    private int blockIndex;
    private Camera3D camera;

    // Whether we are currently editing or viewing
    private bool currentlyEditing;
    private bool editMode;
    private Label editModeSelected;

    // List of pieces
    private readonly List<List<BuildingBlock>> pieces;
    private readonly List<Transform3D> pieceStates;
    private PuzzleNode puzzleNode;
    private LineEdit nameEdit;

    public EditMode(bool editMode) {
        this.editMode = editMode;
        pieces = new List<List<BuildingBlock>>();
        pieceStates = new List<Transform3D>();
    }

    public override void _Ready() {
        Name = "EditMode";
        puzzleNode = GetNode<PuzzleNode>("../PuzzleNode");
        camera = GetNode<Camera3D>("../SpectatorCamera");
        editModeSelected = GetNode<Label>("../EditModeSelected");
        nameEdit = GetNode<LineEdit>("../PauseMenu/NameEdit");
        EnterEditMode();
        UpdateStatusText();
    }

    public override void _ExitTree() {
        // Save the puzzle
        puzzleNode.PuzzleData.Pieces = new List<PuzzlePiece>();
        for (var i = 0; i < pieces.Count; i++) {
            var piece = GetPuzzlePiece(i);
            PuzzleUtils.CenterizePiece(piece);
            pieceStates[i] = piece.State;
            puzzleNode.PuzzleData.Pieces.Add(piece);
        }

        puzzleNode.PuzzleData.TargetShape = GetTargetShape();

        puzzleNode.PuzzleData.Solutions = new List<Solution> {
            new() {
                States = pieceStates,
                Time = DateTime.Now,
            }
        };
    }

    private void EditDisablePiece(int index) {
        foreach (var block in pieces[index]) block.DisableCollisions();
        puzzleNode.PuzzlePieceNodes[index].SetTransparency(0.4f);
    }

    private void EditEnablePiece(int index, bool puzzlePieceCollisions = false) {
        foreach (var block in pieces[index]) block.EnableCollisions();
        puzzleNode.PuzzlePieceNodes[index].SetTransparency(1.0f);

        if (puzzlePieceCollisions)
            puzzleNode.PuzzlePieceNodes[index].EnableCollisions();
        else
            puzzleNode.PuzzlePieceNodes[index].DisableCollisions();
    }

    private void BuildPuzzlePiece(int index) {
        BuildTargetShape();

        if (index < puzzleNode.PuzzlePieceNodes.Count)
            puzzleNode.RemoveChild(puzzleNode.PuzzlePieceNodes[index]);

        var piece = pieces[index];
        if (piece.Count <= 0) return;

        puzzleNode.AddPiece(GetPuzzlePiece(index), index);
        puzzleNode.PuzzlePieceNodes[index].DisableCollisions();
    }

    private PuzzlePiece GetPuzzlePiece(int index) {
        var piece = pieces[index];
        return new() {
            Shape = piece.ConvertAll(block => block.PositionInShape),
            Color = piece[0].Color,
            State = pieceStates[index]
        };
    }

    private void BuildTargetShape() {
        puzzleNode.AddTargetShape(GetTargetShape());
    }

    private List<Vector3> GetTargetShape() {
        List<Vector3> targetShape = new();

        foreach (var p in pieces)
        foreach (var block in p)
            targetShape.Add(block.Position);

        return targetShape;
    }

    private void UpdateStatusText() {
        if (blockIndex == pieces.Count + 1)
            editModeSelected.Text = "View";
        else if (blockIndex == pieces.Count)
            editModeSelected.Text = "New piece";
        else
            editModeSelected.Text = $"Editing piece {blockIndex}";
    }

    private void BlockIndexUpdate() {
        blockIndex = Mathf.PosMod(blockIndex, pieces.Count + 2);

        UpdateStatusText();
        if (blockIndex >= pieces.Count)
            for (var i = 0; i < pieces.Count; i++)
                EditEnablePiece(i, true);
        else
            for (var i = 0; i < pieces.Count; i++)
                if (i == blockIndex)
                    EditEnablePiece(i);
                else
                    EditDisablePiece(i);
    }

    public override void _Input(InputEvent @event) {
        if (@event.IsActionPressed("block_build_select_up")) {
            blockIndex++;
            BlockIndexUpdate();
        }

        if (@event.IsActionPressed("block_build_select_down")) {
            blockIndex--;
            BlockIndexUpdate();
        }

        if (@event.IsActionPressed("block_build_select")) {
            PickBlock();
        }

        if (@event.IsActionPressed("move_piece") && blockIndex != pieces.Count + 1) {
            TryPlaceTargetBlock();
        }

        if (@event.IsActionPressed("reset_piece") && blockIndex != pieces.Count + 1) {
            TryRemoveTargetBlock();
        }
    }

    private void EnterEditMode() {
        nameEdit.Editable = true;

        pieces.Clear();
        pieceStates.Clear();

        foreach (var puzzlePiece in puzzleNode.PuzzlePieceNodes) {
            pieceStates.Add(puzzlePiece.Transform);
            puzzlePiece.PieceData.State = puzzlePiece.Transform;
            List<BuildingBlock> piece = new();

            var pieceData = puzzlePiece.PieceData;
            foreach (var voxelPos in pieceData.Shape) {
                var block = new BuildingBlock(puzzlePiece.Color);
                block.Hide();

                block.PositionInShape = voxelPos;
                block.Position = puzzlePiece.Transform * voxelPos;

                piece.Add(block);
                puzzleNode.AddChild(block);
            }

            pieces.Add(piece);
        }

        blockIndex = pieces.Count;
        BlockIndexUpdate();
    }

    private bool TryPlaceTargetBlock() {
        if (blockIndex == pieces.Count + 1) return false;

        uint collsionMask = blockIndex == pieces.Count ? 0b011u : 0b100u;

        var spaceState = GetWorld3D().DirectSpaceState;
        var mousePos = GetViewport().GetMousePosition();

        var origin = camera.ProjectRayOrigin(mousePos);
        var normal = camera.ProjectRayNormal(mousePos);
        var end = origin + normal * RayLength;
        var query = PhysicsRayQueryParameters3D.Create(origin, end, collsionMask);
        var result = spaceState.IntersectRay(query);

        if (!result.TryGetValue("position", out var position)) {
            // GD.Print("No position");
            return false;
        }

        if (!result.TryGetValue("normal", out var collisionNormal)) {
            // GD.Print("No normal");
            return false;
        }

        var pos = (Vector3)position;
        var norm = (Vector3)collisionNormal;
        pos += norm * 0.5f;

        // GD.Print("pos: ", pos);
        // GD.Print("norm: ", norm);

        pos = puzzleNode.ToLocal(pos).Round();

        if (CheckForBlock(pos, out int blockIndex2, out var otherBlock)) {
            // GD.Print("removing block.");
            if (blockIndex2 == blockIndex) return false;
            HandleRemoved(ref blockIndex2, otherBlock);
        }

        BuildingBlock block;
        if (blockIndex == pieces.Count) {
            List<BuildingBlock> piece = new();
            pieceStates.Add(Transform3D.Identity);
            pieces.Add(piece);
            blockIndex = pieces.Count - 1;

            block = new BuildingBlock(PuzzleUtils.DefaultColors[blockIndex % PuzzleUtils.DefaultColors.Length]);
        }
        else {
            block = new BuildingBlock(pieces[blockIndex][0].Color);
        }

        block.Hide();
        block.Position = pos;
        block.PositionInShape = pieceStates[blockIndex].Inverse() * pos;

        HandlePlaced(blockIndex, block);
        return true;
    }

    private bool TryRemoveTargetBlock() {
        if (blockIndex >= pieces.Count) return false;

        var spaceState = GetWorld3D().DirectSpaceState;
        var mousePos = GetViewport().GetMousePosition();

        var origin = camera.ProjectRayOrigin(mousePos);
        var normal = camera.ProjectRayNormal(mousePos);
        var end = origin + normal * RayLength;
        var query = PhysicsRayQueryParameters3D.Create(origin, end, 0b100);
        var result = spaceState.IntersectRay(query);

        if (!result.TryGetValue("collider", out var collider) || collider.Obj is not BuildingBlock block) return false;
        if (!pieces[blockIndex].Contains(block)) return false;

        HandleRemoved(ref blockIndex, block);
        return true;
    }

    private void PickBlock() {
        var spaceState = GetWorld3D().DirectSpaceState;
        var mousePos = GetViewport().GetMousePosition();

        var origin = camera.ProjectRayOrigin(mousePos);
        var normal = camera.ProjectRayNormal(mousePos);
        var end = origin + normal * RayLength;
        var query = PhysicsRayQueryParameters3D.Create(origin, end, 0b001);
        var result = spaceState.IntersectRay(query);

        if (!result.TryGetValue("collider", out var collider) || collider.Obj is not BuildingBlock block) return;

        blockIndex = pieces.FindIndex(piece => piece.Contains(block));
        BlockIndexUpdate();
    }

    private void HandlePlaced(int blockIndex2, BuildingBlock block) {
        pieces[blockIndex2].Add(block);
        puzzleNode.AddChild(block);

        BuildPuzzlePiece(blockIndex2);
        BlockIndexUpdate();
    }

    private void HandleRemoved(ref int blockIndex2, BuildingBlock block) {
        pieces[blockIndex2].Remove(block);
        // GD.Print("Removed: ", removed);
        block.QueueFree();

        BuildPuzzlePiece(blockIndex2);

        if (pieces[blockIndex2].Count == 0) {
            pieces.RemoveAt(blockIndex2);
            pieceStates.RemoveAt(blockIndex2);
            puzzleNode.PuzzlePieceNodes.RemoveAt(blockIndex2);

            BlockIndexUpdate();
        }
    }

    private bool CheckForBlock(Vector3 position, out int blockIndex2, out BuildingBlock block) {
        blockIndex2 = 0;
        foreach (var piece in pieces) {
            foreach (var b in piece) {
                if (!(b.Position.DistanceSquaredTo(position) < 0.1)) continue;
                block = b;
                return true;
            }

            blockIndex2++;
        }

        block = null;
        return false;
    }
}