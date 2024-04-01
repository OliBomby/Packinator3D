using System.Collections.Generic;
using Godot;

namespace Packinator3D.datastructure;

public record PuzzlePiece {
    public List<Vector3> Shape { get; set; }
    public Color Color { get; set; }
    public Transform3D State { get; set; }

    public PuzzlePiece Copy() {
        return this with { Shape = new List<Vector3>(Shape) };
    }
}