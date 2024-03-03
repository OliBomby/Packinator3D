using System;
using System.Collections.Generic;

namespace BlockPuzzleViewerSolverEditor.datastructure;

public record Solution {
    public List<PuzzlePieceState> States { get; set; }
    public DateTime Time { get; set; }
}