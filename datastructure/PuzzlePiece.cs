using System.Collections.Generic;
using Godot;

namespace BlockPuzzleViewerSolverEditor.datastructure;

public record PuzzlePiece {
    public List<Vector3> Shape { get; set; }
    public Color Color { get; set; }
    public Transform3D State { get; set; }
}