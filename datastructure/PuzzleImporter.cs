using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace BlockPuzzleViewerSolverEditor.datastructure;

public static class PuzzleImporter {
    public static readonly Color[] DefaultColors = {
        Colors.Yellow,
        Colors.Orange,
        Colors.Green,
        Colors.Pink,
        Colors.Red,
        Colors.SkyBlue,
        Colors.Blue,
        Colors.LightGreen,
    };

    public static Puzzle FromSolution(string path) {
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        var pieces = new List<PuzzlePiece>();
        var solution = new Solution { States = new List<PuzzlePieceState>(), Time = DateTime.Now };
        var index = 0;

        while (!file.EofReached()) {
            string line = file.GetLine();
            if (string.IsNullOrWhiteSpace(line) || line[0] == '#') continue;
            var piece = PieceFromString(line, DefaultColors[index++ % DefaultColors.Length]);
            pieces.Add(piece);
            solution.States.Add(new PuzzlePieceState {
                Offset = piece.State.Offset,
                Rotation = piece.State.Rotation,
            });
        }

        // Move the pieces to the start position
        var states = GetStartStates(pieces);
        for (var i = 0; i < pieces.Count; i++) {
            pieces[i].State = states[i];
        }

        return new Puzzle {
            Name = path.GetFile(),
            Pieces = pieces,
            TargetShape = pieces.SelectMany(o => o.Shape).ToList(),
            Solutions = new List<Solution> { solution }
        };
    }

    private static List<PuzzlePieceState> GetStartStates(List<PuzzlePiece> pieces) {
        var states = new List<PuzzlePieceState>();
        float x = -4;

        foreach (var piece in pieces) {
            var (min, max) = GetDimensions(piece.Shape);
            var rotation = GetRotationToMinimizeAxis(max - min, Vector3.Axis.X);
            (min, max) = RotateDimensions(min, max, rotation);
            var pos = new Vector3(x - min.X, -min.Y, -4 - min.Z);
            x += max.X - min.X + 2;
            states.Add(new PuzzlePieceState {
                Offset = pos,
                Rotation = rotation,
            });
        }

        return states;
    }

    private static (Vector3, Vector3) RotateDimensions(Vector3 min, Vector3 max, Vector3 rotation) {
        (min, max) = (Rotate(min, rotation).Round(), Rotate(max, rotation).Round());
        if (min.X > max.X) (min.X, max.X) = (max.X, min.X);
        if (min.Y > max.Y) (min.Y, max.Y) = (max.Y, min.Y);
        if (min.Z > max.Z) (min.Z, max.Z) = (max.Z, min.Z);
        return (min, max);
    }

    private static Vector3 Rotate(Vector3 vec, Vector3 rotation) {
        if (rotation == Vector3.Zero) return vec;
        return vec.Rotated(rotation.Normalized(), rotation.Length());
    }

    private static Vector3 GetRotationToMinimizeAxis(Vector3 dims, Vector3.Axis axis) {
        var smallestAxis = dims.X <= dims.Z && dims.X <= dims.Y ? Vector3.Axis.X : dims.Y <= dims.Z ? Vector3.Axis.Y : Vector3.Axis.Z;
        if (smallestAxis == axis) return Vector3.Zero;
        if (smallestAxis != Vector3.Axis.X && axis != Vector3.Axis.X) return new Vector3(Mathf.Pi / 2, 0, 0);
        if (smallestAxis != Vector3.Axis.Y && axis != Vector3.Axis.Y) return new Vector3(0, Mathf.Pi / 2, 0);
        return new Vector3(0, 0, Mathf.Pi / 2);
    }

    private static (Vector3, Vector3) GetDimensions(List<Vector3> shape) {
        var min = new Vector3(
            shape.Min(o => o.X),
            shape.Min(o => o.Y),
            shape.Min(o => o.Z));
        var max = new Vector3(
            shape.Max(o => o.X),
            shape.Max(o => o.Y),
            shape.Max(o => o.Z));
        return (min, max);
    }

    public static PuzzlePiece PieceFromString(string line, Color color) {
        var shape = ShapeFromString(line);
        var center = GetCenter(shape);
        for (var i = 0; i < shape.Count; i++) {
            shape[i] -= center;
        }

        return new PuzzlePiece {
            Shape = shape,
            Color = color,
            State = new PuzzlePieceState {
                Offset = center,
                Rotation = Vector3.Zero,
            },
        };
    }

    public static Vector3 GetCenter(List<Vector3> shape) {
        float x = Mathf.Round(shape.Select(o => o.X).Average());
        float y = Mathf.Round(shape.Select(o => o.Y).Average());
        float z = Mathf.Round(shape.Select(o => o.Z).Average());
        return new Vector3(x, y, z);
    }

    public static List<Vector3> ShapeFromString(string str) {
        string[] coordSplit = str.Split("   ");
        var shape = new List<Vector3>();
        foreach (string coord in coordSplit) {
            string[] xyz = coord.Split(" ");
            shape.Add(new Vector3(float.Parse(xyz[0]), float.Parse(xyz[1]), float.Parse(xyz[2])));
        }

        return shape;
    }
}