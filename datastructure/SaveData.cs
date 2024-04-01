using System.Collections.Generic;

namespace Packinator3D.datastructure;

public record SaveData {
    public List<Puzzle> CustomPuzzles { get; set; } = new();
    public List<Puzzle> Puzzles { get; set; } = new();

    public List<float> BusVolumes { get; set; }

    public float Sensitivity { get; set; } = 1;
}