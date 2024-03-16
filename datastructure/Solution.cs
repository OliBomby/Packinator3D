using System;
using System.Collections.Generic;
using Godot;

namespace Packinator3D.datastructure;

public record Solution {
    public List<Transform3D> States { get; set; }
    public DateTime Time { get; set; }
}