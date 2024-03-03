using System.Collections.Generic;
using Godot;

namespace BlockPuzzleViewerSolverEditor.datastructure;

public record PuzzlePiece {
    public List<Vector3> Shape { get; set; }
    public Color Color { get; set; }
    public PuzzlePieceState State { get; set; }
}