using Godot;

namespace BlockPuzzleViewerSolverEditor.datastructure;

public record PuzzlePiece {
    public bool[,,] Shape { get; set; }
    public Color Color { get; set; }
    public PuzzlePieceState State { get; set; }
}