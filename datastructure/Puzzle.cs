using System.Collections.Generic;
using Godot;

namespace Packinator3D.datastructure;

public record Puzzle {
    public string Name { get; set; }
    public List<Vector3> TargetShape { get; set; }
    public List<PuzzlePiece> Pieces { get; set; }
    public List<Solution> Solutions { get; set; }

    public Puzzle Copy() {
        return new Puzzle {
            Name = Name,
            TargetShape = new List<Vector3>(TargetShape),
            Pieces = Pieces.ConvertAll(p => p.Copy()),
            Solutions = Solutions.ConvertAll(s => s.Copy()),
        };
    }
}