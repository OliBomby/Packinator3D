using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Packinator3D.scenes.puzzle;

namespace Packinator3D.datastructure;

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

    public static List<Vector3> PieceNodesToShape(IEnumerable<PuzzlePieceNode> pieces) {
		var shape = new List<Vector3>();
		foreach (var piece in pieces) {
			shape.AddRange(piece.PieceData.Shape.Select(v => Transform(v, piece.Transform)));
		}
		return shape;
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

    public static List<Transform3D> GetStartStates(List<PuzzlePiece> pieces) {
        var states = new List<Transform3D>();
        float x = -9;

        foreach (var piece in pieces) {
            var (min, max) = GetDimensions(piece.Shape);
            var rotation = GetRotationToMinimizeAxis(max - min, Vector3.Axis.X);
            (min, max) = RotateDimensions(min, max, rotation);
            var pos = new Vector3(x - min.X, -min.Y, -9 - min.Z);
            x += max.X - min.X + 2;
            states.Add(new Transform3D(rotation, pos));
        }

        return states;
    }

    public static Vector3 Transform(Vector3 v, Transform3D t) => (t * v).Round();

    public static Vector3 Transform(Vector3 v, Basis t) => (t * v).Round();

    public static (Vector3, Vector3) RotateDimensions(Vector3 min, Vector3 max, Basis rotation) {
        (min, max) = (Transform(min, rotation), Transform(max, rotation));
        if (min.X > max.X) (min.X, max.X) = (max.X, min.X);
        if (min.Y > max.Y) (min.Y, max.Y) = (max.Y, min.Y);
        if (min.Z > max.Z) (min.Z, max.Z) = (max.Z, min.Z);
        return (min, max);
    }

    public static Basis GetRotationToMinimizeAxis(Vector3 dims, Vector3.Axis axis) {
        var smallestAxis = dims.X <= dims.Z && dims.X <= dims.Y ? Vector3.Axis.X : dims.Y <= dims.Z ? Vector3.Axis.Y : Vector3.Axis.Z;
        if (smallestAxis == axis) return Basis.Identity;
        if (smallestAxis != Vector3.Axis.X && axis != Vector3.Axis.X) return Basis.FromEuler(new Vector3(Mathf.Pi / 2, 0, 0));
        if (smallestAxis != Vector3.Axis.Y && axis != Vector3.Axis.Y) return Basis.FromEuler(new Vector3(0, Mathf.Pi / 2, 0));
        return Basis.FromEuler(new Vector3(0, 0, Mathf.Pi / 2));
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

    public static Mesh ShapeToMesh(List<Vector3> shape, float width=1) {
        // Create the mesh out of the shape voxels
        (bool[,,] voxels, var offset) = ShapeToVoxels(shape);
        var st = new SurfaceTool();
        st.Begin(Mesh.PrimitiveType.Triangles);
        // Create a quad mesh for each side of a voxel that is exposed to air
        for (var x = 0; x < voxels.GetLength(0); x++) {
            for (var y = 0; y < voxels.GetLength(1); y++) {
                for (var z = 0; z < voxels.GetLength(2); z++) {
                    if (!voxels[x, y, z]) continue;
                    var p = new Vector3(x, y, z);
                    // Check if the voxel is exposed to air
                    AddSide(st, voxels, p, offset, Vector3.Left, width);
                    AddSide(st, voxels, p, offset, Vector3.Right, width);
                    AddSide(st, voxels, p, offset, Vector3.Down, width);
                    AddSide(st, voxels, p, offset, Vector3.Up, width);
                    AddSide(st, voxels, p, offset, Vector3.Forward, width);
                    AddSide(st, voxels, p, offset, Vector3.Back, width);
                }
            }
        }

        st.Index();
        st.GenerateTangents();
        return st.Commit();
    }

	private static void AddSide(SurfaceTool st, bool[,,] voxels, Vector3 p, Vector3 offset, Vector3 dir, float width) {
		var right = dir.Cross(Vector3.Up).Normalized();
		if (right == Vector3.Zero) right = dir.Cross(Vector3.Forward).Normalized();
		var up = dir.Cross(right).Normalized();
		var left = -right;
		var down = -up;
		bool l = IsFull(voxels, p + left);
		bool r = IsFull(voxels, p + right);
		bool u = IsFull(voxels, p + up);
		bool d = IsFull(voxels, p + down);
		bool lu = IsFull(voxels, p + left + up);
		bool ru = IsFull(voxels, p + right + up);
		bool ld = IsFull(voxels, p + left + down);
		bool rd = IsFull(voxels, p + right + down);
		bool f = IsFull(voxels, p + dir);
		bool fl = IsFull(voxels, p + dir + left);
		bool fr = IsFull(voxels, p + dir + right);
		bool fu = IsFull(voxels, p + dir + up);
		bool fd = IsFull(voxels, p + dir + down);
		bool flu = IsFull(voxels, p + dir + left + up);
		bool fru = IsFull(voxels, p + dir + right + up);
		bool fld = IsFull(voxels, p + dir + left + down);
		bool frd = IsFull(voxels, p + dir + right + down);

		void addQuad(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3) {
			var uv0 = new Vector2(p0.Dot(right) / 2 + 0.5f, p0.Dot(up) / 2 + 0.5f);
			var uv1 = new Vector2(p1.Dot(right) / 2 + 0.5f, p1.Dot(up) / 2 + 0.5f);
			var uv2 = new Vector2(p2.Dot(right) / 2 + 0.5f, p2.Dot(up) / 2 + 0.5f);
			var uv3 = new Vector2(p3.Dot(right) / 2 + 0.5f, p3.Dot(up) / 2 + 0.5f);
			p0 = p + offset + dir * 0.5f * width + p0 * 0.5f;
			p1 = p + offset + dir * 0.5f * width + p1 * 0.5f;
			p2 = p + offset + dir * 0.5f * width + p2 * 0.5f;
			p3 = p + offset + dir * 0.5f * width + p3 * 0.5f;
			st.SetUV(uv0);
			st.SetNormal(dir);
			st.AddVertex(p0);
			st.SetUV(uv3);
			st.SetNormal(dir);
			st.AddVertex(p3);
			st.SetUV(uv1);
			st.SetNormal(dir);
			st.AddVertex(p1);
			st.SetUV(uv1);
			st.SetNormal(dir);
			st.AddVertex(p1);
			st.SetUV(uv3);
			st.SetNormal(dir);
			st.AddVertex(p3);
			st.SetUV(uv2);
			st.SetNormal(dir);
			st.AddVertex(p2);
		}

		if (!f) addQuad(left * width + down * width, right * width + down * width, right * width + up * width, left * width + up * width);
		if (Math.Abs(width - 1) < 1e-5) return;
		if (l && !(fl && f)) addQuad(left + down * width, left * width + down * width, left * width + up * width, left + up * width);
		if (r && !(fr && f)) addQuad(right * width + down * width, right + down * width, right + up * width, right * width + up * width);
		if (u && !(fu && f)) addQuad(left * width + up * width, right * width + up * width, right * width + up, left * width + up);
		if (d && !(fd && f)) addQuad(left * width + down, right * width + down, right * width + down * width, left * width + down * width);
		if (l && u && lu && !(fl && fu && flu)) addQuad(left + up * width, left * width + up * width, left * width + up, left + up);
		if (r && u && ru && !(fr && fu && fru)) addQuad(right * width + up * width, right + up * width, right + up, right * width + up);
		if (l && d && ld && !(fl && fd && fld)) addQuad(left + down, left * width + down, left * width + down * width, left + down * width);
		if (r && d && rd && !(fr && fd && frd)) addQuad(right * width + down, right + down, right + down * width, right * width + down * width);
	}
}