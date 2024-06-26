using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
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
    private bool allowEditTargetShape;

    // List of pieces
    private readonly List<List<BuildingBlock>> pieces;
    private readonly List<Transform3D> pieceStates;
    private PuzzleNode puzzleNode;
    private LineEdit nameEdit;
    private readonly List<BuildingBlock> targetShapeBlocks;

    [ExportGroup("Sounds")]
    [Export]
    public AudioStream PlaceSound { get; set; }

    [Export]
    public AudioStream DeleteSound { get; set; }

    [Export]
    public AudioStream PickSound { get; set; }

    [Export]
    public AudioStream SelectSound { get; set; }

    private AudioStreamPlayer placeSoundPlayer;
    private AudioStreamPlayer deleteSoundPlayer;
    private AudioStreamPlayer pickSoundPlayer;
    private AudioStreamPlayer selectSoundPlayer;

    public EditMode(bool editMode) {
        this.editMode = editMode;
        pieces = new List<List<BuildingBlock>>();
        pieceStates = new List<Transform3D>();
        targetShapeBlocks = new List<BuildingBlock>();
    }

    public override void _Ready() {
        Name = "EditMode";
        puzzleNode = GetNode<PuzzleNode>("../PuzzleNode");
        camera = GetNode<Camera3D>("../SpectatorCamera");
        editModeSelected = GetNode<Label>("../EditModeSelected");
        nameEdit = GetNode<LineEdit>("../PauseMenu/NameEdit");
        var editTargetShape = GetNode<Button>("../PauseMenu/EditTargetShape");
        allowEditTargetShape = !puzzleNode.IsSolved();
        editTargetShape.ButtonPressed = allowEditTargetShape;
        editTargetShape.Show();
        editTargetShape.Toggled += EditTargetShapeOnToggled;

        PlaceSound = ResourceLoader.Load<AudioStreamOggVorbis>("res://sounds/place.ogg");
        DeleteSound = ResourceLoader.Load<AudioStreamOggVorbis>("res://sounds/delete.ogg");
        PickSound = ResourceLoader.Load<AudioStreamOggVorbis>("res://sounds/pick.ogg");
        SelectSound = ResourceLoader.Load<AudioStreamOggVorbis>("res://sounds/select.ogg");
        placeSoundPlayer = CreateSoundPlayer(PlaceSound);
        deleteSoundPlayer = CreateSoundPlayer(DeleteSound);
        pickSoundPlayer = CreateSoundPlayer(PickSound);
        selectSoundPlayer = CreateSoundPlayer(SelectSound);

        EnterEditMode();
        UpdateStatusText();
    }

    private void EditTargetShapeOnToggled(bool toggledon) {
        allowEditTargetShape = toggledon;
        BlockIndexUpdate();
    }

    private AudioStreamPlayer CreateSoundPlayer(AudioStream stream) {
        var soundPlayer = new AudioStreamPlayer {
            Bus = "Effects",
            Stream = stream,
        };
        AddChild(soundPlayer);
        return soundPlayer;
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

        if (puzzleNode.IsSolved()) {
            puzzleNode.PuzzleData.Solutions = new List<Solution> {
                new() {
                    States = pieceStates,
                    Time = DateTime.Now,
                }
            };
        }
        else {
            puzzleNode.PuzzleData.Solutions = new List<Solution>();
        }
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
        if (!allowEditTargetShape)
            BuildTargetShapeFromPieces();

        if (index < puzzleNode.PuzzlePieceNodes.Count)
            puzzleNode.RemoveChild(puzzleNode.PuzzlePieceNodes[index]);

        var piece = pieces[index];
        if (piece.Count <= 0) return;

        puzzleNode.AddPiece(GetPuzzlePiece(index), index);
        puzzleNode.PuzzlePieceNodes[index].DisableCollisions();
    }

    private PuzzlePiece GetPuzzlePiece(int index) {
        var piece = pieces[index];
        return new PuzzlePiece {
            Shape = piece.ConvertAll(block => block.PositionInShape),
            Color = piece[0].Color,
            State = pieceStates[index]
        };
    }

    private void BuildTargetShape() {
        puzzleNode.AddTargetShape(GetTargetShape());
    }

    private void BuildTargetShapeFromPieces() {
        foreach (var block in targetShapeBlocks) block.QueueFree();
        targetShapeBlocks.Clear();

        var targetShape = GetTargetShapeFromPieces();
        foreach (var pos in targetShape) {
            AddTargetBlock(pos);
        }

        puzzleNode.AddTargetShape(targetShape);
    }

    private List<Vector3> GetTargetShapeFromPieces() {
        List<Vector3> targetShape = new();

        foreach (var p in pieces)
        foreach (var block in p)
            targetShape.Add(block.Position);

        return targetShape;
    }

    private List<Vector3> GetTargetShape() {
        return targetShapeBlocks.Select(block => block.Position).ToList();
    }

    private int NumBlockIndex => allowEditTargetShape ? pieces.Count + 3 : pieces.Count + 2;

    private void UpdateStatusText() {
        if (blockIndex == pieces.Count + 2)
            editModeSelected.Text = "Edit target shape";
        else if (blockIndex == pieces.Count + 1)
            editModeSelected.Text = "View";
        else if (blockIndex == pieces.Count)
            editModeSelected.Text = "New piece";
        else
            editModeSelected.Text = $"Editing piece {blockIndex}";
    }

    private void BlockIndexUpdate() {
        blockIndex = Mathf.PosMod(blockIndex, NumBlockIndex);

        UpdateStatusText();

        if (blockIndex == pieces.Count + 2) {
            foreach (var targetShapeBlock in targetShapeBlocks) targetShapeBlock.Show();
            for (var i = 0; i < pieces.Count; i++)
                EditDisablePiece(i);
        }
        else {
            foreach (var targetShapeBlock in targetShapeBlocks) targetShapeBlock.Hide();
        }

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
            selectSoundPlayer.Play();
        }

        if (@event.IsActionPressed("block_build_select_down")) {
            blockIndex--;
            BlockIndexUpdate();
            selectSoundPlayer.Play();
        }

        if (@event.IsActionPressed("block_build_select")) {
            PickBlock();
            pickSoundPlayer.Play();
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
        targetShapeBlocks.Clear();

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

        foreach (var p in puzzleNode.PuzzleData.TargetShape) {
            AddTargetBlock(p);
        }

        blockIndex = pieces.Count;
        BlockIndexUpdate();
    }

    private void AddTargetBlock(Vector3 pos) {
        var block = new BuildingBlock(Color.Color8(255, 100, 0), 0b1000u);
        if (blockIndex != pieces.Count + 2)
            block.Hide();
        block.Position = pos;
        targetShapeBlocks.Add(block);
        puzzleNode.AddChild(block);
    }

    private bool TryPlaceTargetBlock() {
        if (blockIndex == pieces.Count + 1) return false;

        uint collisionMask = blockIndex == pieces.Count + 2 ? 0b1000u : blockIndex == pieces.Count ? 0b011u : 0b100u;

        var spaceState = GetWorld3D().DirectSpaceState;
        var mousePos = GetViewport().GetMousePosition();

        var origin = camera.ProjectRayOrigin(mousePos);
        var normal = camera.ProjectRayNormal(mousePos);
        var end = origin + normal * RayLength;
        var query = PhysicsRayQueryParameters3D.Create(origin, end, collisionMask);
        var result = spaceState.IntersectRay(query);

        if (!IsCollisionValid(result, out var pos)) {
            if (blockIndex == pieces.Count)
                return false;

            uint mask = blockIndex == pieces.Count + 2 ? 0b010u : 0b011u;
            query = PhysicsRayQueryParameters3D.Create(origin, end, mask);
            result = spaceState.IntersectRay(query);
            if (!IsCollisionValid(result, out pos))
                return false;
        }

        if (blockIndex == pieces.Count + 2) {
            if (CheckForBlockTarget(pos))
                return false;
        }
        else if (CheckForBlock(pos, out int blockIndex2, out var otherBlock)) {
            // GD.Print("removing block.");
            if (blockIndex2 == blockIndex) return false;
            HandleRemoved(ref blockIndex2, otherBlock);
        }

        BuildingBlock block;
        if (blockIndex == pieces.Count + 2) {
            AddTargetBlock(pos);
            BuildTargetShape();
            placeSoundPlayer.Play();
            return true;
        }

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
        placeSoundPlayer.Play();
        return true;
    }

    private bool IsCollisionValid(Dictionary result, out Vector3 pos) {
        pos = Vector3.Zero;

        if (!result.TryGetValue("position", out var position)) {
            // GD.Print("No position");
            return false;
        }

        if (!result.TryGetValue("normal", out var collisionNormal)) {
            // GD.Print("No normal");
            return false;
        }

        pos = (Vector3)position;
        var norm = (Vector3)collisionNormal;
        pos += norm * 0.5f;
        pos = puzzleNode.ToLocal(pos).Round();

        // Make sure the position is above ground
        if (pos.Y < 0)
            return false;

        if (blockIndex >= pieces.Count) return true;

        // Make sure the position is adjacent to the current block
        var positionInShape = pieceStates[blockIndex].Inverse() * pos;
        bool found = pieces[blockIndex].Any(v => v.PositionInShape.DistanceSquaredTo(positionInShape) < 1.2);

        return found;
    }

    private bool TryRemoveTargetBlock() {
        if (blockIndex >= pieces.Count && blockIndex != pieces.Count + 2) return false;

        uint collisionMask = blockIndex == pieces.Count + 2 ? 0b1000u : 0b100u;

        var spaceState = GetWorld3D().DirectSpaceState;
        var mousePos = GetViewport().GetMousePosition();

        var origin = camera.ProjectRayOrigin(mousePos);
        var normal = camera.ProjectRayNormal(mousePos);
        var end = origin + normal * RayLength;
        var query = PhysicsRayQueryParameters3D.Create(origin, end, collisionMask);
        var result = spaceState.IntersectRay(query);

        if (!result.TryGetValue("collider", out var collider) || collider.Obj is not BuildingBlock block) return false;

        if (blockIndex == pieces.Count + 2) {
            if (!targetShapeBlocks.Contains(block)) return false;
            targetShapeBlocks.Remove(block);
            block.QueueFree();
            BuildTargetShape();
            deleteSoundPlayer.Play();
            return true;
        }

        if (!pieces[blockIndex].Contains(block)) return false;

        HandleRemoved(ref blockIndex, block);
        deleteSoundPlayer.Play();
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

    private bool CheckForBlockTarget(Vector3 position) {
        foreach (var b in targetShapeBlocks) {
            if (!(b.Position.DistanceSquaredTo(position) < 0.1)) continue;
            return true;
        }

        return false;
    }
}