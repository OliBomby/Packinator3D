using Godot;

namespace BlockPuzzleViewerSolverEditor.datastructure;

public record PuzzlePieceState {
    public Vector3 Offset { get; set; }
    public Vector3 Rotation { get; set; }
}