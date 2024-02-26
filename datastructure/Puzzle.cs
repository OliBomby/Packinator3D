using System.Collections.Generic;

namespace BlockPuzzleViewerSolverEditor.datastructure;

public record Puzzle {
    public string Name { get; set; }
    public bool[,,] TargetShape { get; set; }
    public List<PuzzlePiece> Pieces { get; set; }
    public List<Solution> Solutions { get; set; }
}