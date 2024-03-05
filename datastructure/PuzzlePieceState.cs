using Godot;

namespace BlockPuzzleViewerSolverEditor.datastructure;

public record PuzzlePieceState(Vector3 Offset, Vector3 Rotation) {
    public PuzzlePieceState Copy() => new(Offset, Rotation);
}