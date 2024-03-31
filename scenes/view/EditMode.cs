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

    public EditMode(bool editMode) {
        this.editMode = editMode;
        pieces = new List<List<BuildingBlock>>();
        pieceStates = new List<Transform3D>();
    }

    public override void _Ready() {
        puzzleNode = GetNode<PuzzleNode>("../PuzzleNode");
        camera = GetNode<Camera3D>("../SpectatorCamera");
        editModeSelected = GetNode<Label>("../EditModeSelected");
        EnterEditMode();
        UpdateStatusText();
    }

    public override void _Input(InputEvent @event) {
        if (@event.IsActionPressed("edit_mode"))
            SwitchMode();
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

        if (index < puzzleNode.PuzzlePieceNodes.Count) puzzleNode.RemoveChild(puzzleNode.PuzzlePieceNodes[index]);
        var piece = pieces[index];

        if (piece.Count > 0) {
            PuzzlePiece newPiece = new() {
                Shape = piece.ConvertAll(block => block.PositionInShape),
                Color = piece[0].Color,
                State = pieceStates[blockIndex]
            };

            puzzleNode.AddPiece(newPiece, index);
            puzzleNode.PuzzlePieceNodes[index].DisableCollisions();
        }
    }

    private void BuildTargetShape() {
        List<Vector3> targetShape = new();

        foreach (var p in pieces)
        foreach (var block in p)
            targetShape.Add(block.Position);

        puzzleNode.AddTargetShape(targetShape);
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
        UpdateStatusText();
        if (blockIndex == pieces.Count + 1)
            for (var i = 0; i < pieces.Count; i++)
                EditEnablePiece(i, true);
        else
            for (var i = 0; i < pieces.Count; i++)
                if (i == blockIndex)
                    EditEnablePiece(i);
                else
                    EditDisablePiece(i);
    }


    public override void _PhysicsProcess(double delta) {
        if (Input.IsActionJustPressed("block_build_select")) {
            blockIndex += 1;
            // blockIndex = pieces.Count + 1 -> show all
            if (blockIndex > pieces.Count + 1) blockIndex = 0;

            BlockIndexUpdate();
        }

        if (Input.IsActionJustPressed("move_piece") && blockIndex != pieces.Count + 1)
            if (TryPlaceTargetBlock()) {
                BuildPuzzlePiece(blockIndex);
                BlockIndexUpdate();
            }

        if (Input.IsActionJustPressed("reset_piece") && blockIndex != pieces.Count + 1)
            if (TryRemoveTargetBlock()) {
                BuildPuzzlePiece(blockIndex);
                if (pieces[blockIndex].Count == 0) {
                    pieces.RemoveAt(blockIndex);
                    pieceStates.RemoveAt(blockIndex);
                    puzzleNode.PuzzlePieceNodes.RemoveAt(blockIndex);

                    if (blockIndex == pieces.Count && blockIndex != 0) blockIndex -= 1;
                    BlockIndexUpdate();
                }
            }
    }


    public void SwitchMode() {
        // if (currentlyEditing) {
        // 	// Switch to view mode
        // 	enterViewMode();
        // }
        // else {
        // 	// Switch to edit mode
        // 	enterEditMode();
        // }
        // currentlyEditing ^= true;
    }

    // private void enterViewMode() {
    // 	// Change target shape
    // 	buildTargetShape();

    // 	// Explicitly remove the pieces -> fixes something with collision
    // 	foreach(PuzzlePieceNode ppn in puzzleNode.PuzzlePieceNodes) {
    // 		puzzleNode.RemoveChild(ppn);
    // 	}
    // 	puzzleNode.PuzzlePieceNodes.Clear();

    // 	for (int i = 0; i < pieces.Count; i++) {
    // 		List<BuildingBlock> piece = pieces[i];

    // 		PuzzlePiece newPiece = new();
    // 		newPiece.Shape = piece.ConvertAll(block => block.PositionInShape);
    // 		newPiece.Color = piece[0].Color;
    // 		newPiece.State = pieceStates[i];

    // 		puzzleNode.AddPiece(newPiece);
    // 	}
    // }

    private void EnterEditMode() {
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

        blockIndex = pieces.Count + 1;
        for (var i = 0; i < pieces.Count; i++) EditEnablePiece(i, true);
    }

    private bool TryPlaceTargetBlock() {
        if (blockIndex == pieces.Count + 1) return false;

        var spaceState = GetWorld3D().DirectSpaceState;
        var mousePos = GetViewport().GetMousePosition();

        var origin = camera.ProjectRayOrigin(mousePos);
        var normal = camera.ProjectRayNormal(mousePos);
        var end = origin + normal * RayLength;
        var query = PhysicsRayQueryParameters3D.Create(origin, end);
        var result = spaceState.IntersectRay(query);

        if (!result.TryGetValue("position", out var position)) {
            GD.Print("No position");
            return false;
        }

        if (!result.TryGetValue("normal", out var collisionNormal)) {
            GD.Print("No normal");
            return false;
        }

        if (!result.TryGetValue("collider", out _)) {
            GD.Print("No collider");
            return false;
        }

        var pos = (Vector3)position;
        var norm = (Vector3)collisionNormal;
        pos += norm * 0.5f;

        GD.Print("pos: ", pos);
        GD.Print("norm: ", norm);
        GD.Print("check for block: ", CheckForBlock(puzzleNode.ToLocal(pos).Round()));

        int maxIters = Mathf.FloorToInt(origin.DistanceTo(pos)) * 2;
        var i = 0;
        while (CheckForBlock(puzzleNode.ToLocal(pos).Round())) {
            pos -= normal * 0.5f;
            if (i++ >= maxIters) return false;
        }

        pos = puzzleNode.ToLocal(pos).Round();

        if (blockIndex == pieces.Count) {
            BuildingBlock block = new(PuzzleUtils.DefaultColors[blockIndex % PuzzleUtils.DefaultColors.Length]);
            block.Hide();

            block.Position = pos;
            block.PositionInShape = pos;

            List<BuildingBlock> piece = new() { block };
            pieceStates.Add(Transform3D.Identity);
            pieces.Add(piece);
            puzzleNode.AddChild(block);

            blockIndex = pieces.Count - 1;

            return true;
        }
        else {
            BuildingBlock block = new(pieces[blockIndex][0].Color);
            block.Hide();

            block.Position = pos;
            block.PositionInShape = pieceStates[blockIndex].Inverse() * pos;

            pieces[blockIndex].Add(block);
            puzzleNode.AddChild(block);
            return true;
        }
    }

    private bool TryRemoveTargetBlock() {
        if (blockIndex >= pieces.Count) return false;

        var spaceState = GetWorld3D().DirectSpaceState;
        var mousePos = GetViewport().GetMousePosition();

        var origin = camera.ProjectRayOrigin(mousePos);
        var normal = camera.ProjectRayNormal(mousePos);
        var end = origin + normal * RayLength;
        var query = PhysicsRayQueryParameters3D.Create(origin, end, 2);
        var result = spaceState.IntersectRay(query);

        if (!result.TryGetValue("collider", out var collider) || collider.Obj is not BuildingBlock block) return false;
        if (!pieces[blockIndex].Contains(block)) return false;


        bool removed = pieces[blockIndex].Remove(block);
        GD.Print("Removed: ", removed);
        block.QueueFree();
        return true;
    }

    private bool CheckForBlock(Vector3 position) {
        foreach (var piece in pieces)
        foreach (var block in piece)
            if (block.Position.DistanceSquaredTo(position) < 0.1)
                return true;
        return false;
    }
}