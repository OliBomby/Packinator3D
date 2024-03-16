using System.Collections.Generic;

namespace Packinator3D.datastructure;

public record SaveData {
    public List<Puzzle> Puzzles { get; set; } = new();
    public List<Puzzle> CustomPuzzles { get; set; } = new();
}