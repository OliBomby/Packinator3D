using System.Collections.Generic;
using System.Linq;
using Godot;

namespace BlockPuzzleViewerSolverEditor.datastructure;

public static class PuzzleUtils {
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

    public static bool IsFull(bool[,,] voxels, Vector3 pos) => IsFull(voxels, Mathf.RoundToInt(pos.X), Mathf.RoundToInt(pos.Y), Mathf.RoundToInt(pos.Z));

    public static bool IsAir(bool[,,] voxels, Vector3 pos) => IsAir(voxels, Mathf.RoundToInt(pos.X), Mathf.RoundToInt(pos.Y), Mathf.RoundToInt(pos.Z));

    public static bool IsFull(bool[,,] voxels, int x, int y, int z) {
        if (x < 0 || x >= voxels.GetLength(0)) return false;
        if (y < 0 || y >= voxels.GetLength(1)) return false;
        if (z < 0 || z >= voxels.GetLength(2)) return false;
        return voxels[x, y, z];
    }

    public static bool IsAir(bool[,,] voxels, int x, int y, int z) {
        if (x < 0 || x >= voxels.GetLength(0)) return true;
        if (y < 0 || y >= voxels.GetLength(1)) return true;
        if (z < 0 || z >= voxels.GetLength(2)) return true;
        return !voxels[x, y, z];
    }

    public static (bool[,,], Vector3) ShapeToVoxels(List<Vector3> shape) {
        var (min, max) = GetDimensions(shape);
        var size = max - min + Vector3.One;
        var voxels = new bool[(int)size.X, (int)size.Y, (int)size.Z];
        foreach (var pos in shape) {
            var p = pos - min;
            voxels[(int)p.X, (int)p.Y, (int)p.Z] = true;
        }
        return (voxels, min);
    }

    public static List<PuzzlePieceState> GetStartStates(List<PuzzlePiece> pieces) {
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

    public static (Vector3, Vector3) RotateDimensions(Vector3 min, Vector3 max, Vector3 rotation) {
        (min, max) = (Rotate(min, rotation).Round(), Rotate(max, rotation).Round());
        if (min.X > max.X) (min.X, max.X) = (max.X, min.X);
        if (min.Y > max.Y) (min.Y, max.Y) = (max.Y, min.Y);
        if (min.Z > max.Z) (min.Z, max.Z) = (max.Z, min.Z);
        return (min, max);
    }

    public static Vector3 Rotate(Vector3 vec, Vector3 rotation) {
        if (rotation == Vector3.Zero) return vec;
        return vec.Rotated(rotation.Normalized(), rotation.Length());
    }

    public static Vector3 GetRotationToMinimizeAxis(Vector3 dims, Vector3.Axis axis) {
        var smallestAxis = dims.X <= dims.Z && dims.X <= dims.Y ? Vector3.Axis.X : dims.Y <= dims.Z ? Vector3.Axis.Y : Vector3.Axis.Z;
        if (smallestAxis == axis) return Vector3.Zero;
        if (smallestAxis != Vector3.Axis.X && axis != Vector3.Axis.X) return new Vector3(Mathf.Pi / 2, 0, 0);
        if (smallestAxis != Vector3.Axis.Y && axis != Vector3.Axis.Y) return new Vector3(0, Mathf.Pi / 2, 0);
        return new Vector3(0, 0, Mathf.Pi / 2);
    }

    public static (Vector3, Vector3) GetDimensions(List<Vector3> shape) {
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

    public static Vector3 GetCenter(List<Vector3> shape) {
        float x = Mathf.Round(shape.Select(o => o.X).Average());
        float y = Mathf.Round(shape.Select(o => o.Y).Average());
        float z = Mathf.Round(shape.Select(o => o.Z).Average());
        return new Vector3(x, y, z);
    }
}