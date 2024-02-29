using System.Collections.Generic;
using Godot;

namespace BlockPuzzleViewerSolverEditor.datastructure;

public record Puzzle {
    public string Name { get; set; }
    public List<Vector3> TargetShape { get; set; }
    public List<PuzzlePiece> Pieces { get; set; }
    public List<Solution> Solutions { get; set; }
}