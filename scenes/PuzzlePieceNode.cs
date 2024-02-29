using BlockPuzzleViewerSolverEditor.datastructure;
using Godot;

namespace BlockPuzzleViewerSolverEditor.scenes;

public partial class PuzzlePieceNode : MeshInstance3D
{
	[Export]
	public Color Color { get; set; }

	[Export]
	private float Width { get; set; } = 0.90f;

	public PuzzlePieceNode() {
		GIMode = GIModeEnum.Dynamic;
	}

	public PuzzlePieceNode(PuzzlePiece puzzlePiece) : this() {
		LoadData(puzzlePiece);
	}

	private void LoadData(PuzzlePiece puzzlePiece) {
		Color = puzzlePiece.Color;
		LoadState(puzzlePiece.State);

		// Create the mesh out of the shape voxels
		(bool[,,] voxels, var offset) = PuzzleUtils.ShapeToVoxels(puzzlePiece.Shape);
		var st = new SurfaceTool();
		st.Begin(Mesh.PrimitiveType.Triangles);
		// Create a quad mesh for each side of a voxel that is exposed to air
		for (var x = 0; x < voxels.GetLength(0); x++) {
			for (var y = 0; y < voxels.GetLength(1); y++) {
				for (var z = 0; z < voxels.GetLength(2); z++) {
					if (!voxels[x, y, z]) continue;
					var p = new Vector3(x, y, z);
					// Check if the voxel is exposed to air
					AddSide(st, voxels, p, offset, Vector3.Left);
					AddSide(st, voxels, p, offset, Vector3.Right);
					AddSide(st, voxels, p, offset, Vector3.Down);
					AddSide(st, voxels, p, offset, Vector3.Up);
					AddSide(st, voxels, p, offset, Vector3.Forward);
					AddSide(st, voxels, p, offset, Vector3.Back);
				}
			}
		}

		st.Index();
		st.GenerateTangents();
		st.SetMaterial(new StandardMaterial3D {
			AlbedoColor = Color,
			AlbedoTexture = ResourceLoader.Load<Texture2D>("res://scenes/wood/wood_0002_color_1k.jpg"),
			NormalEnabled = true,
			NormalScale = 0.65f,
			NormalTexture = ResourceLoader.Load<Texture2D>("res://scenes/wood/wood_0002_normal_opengl_1k.png"),
			RoughnessTexture = ResourceLoader.Load<Texture2D>("res://scenes/wood/wood_0002_roughness_1k.jpg"),
			AOTexture = ResourceLoader.Load<Texture2D>("res://scenes/wood/wood_0002_ao_1k.jpg"),
			HeightmapEnabled = true,
			HeightmapTexture = ResourceLoader.Load<Texture2D>("res://scenes/wood/wood_0002_height_1k.png"),
			ClearcoatEnabled = true,
			Clearcoat = 0.2f,
			DistanceFadeMode = BaseMaterial3D.DistanceFadeModeEnum.PixelDither,
			DistanceFadeMaxDistance = 1,
			DistanceFadeMinDistance = 0.3f,
		});
		Mesh = st.Commit();
	}

	private void AddSide(SurfaceTool st, bool[,,] voxels, Vector3 p, Vector3 offset, Vector3 dir) {
		var right = dir.Cross(Vector3.Up).Normalized();
		if (right == Vector3.Zero) right = dir.Cross(Vector3.Forward).Normalized();
		var up = dir.Cross(right).Normalized();
		var left = -right;
		var down = -up;
		bool l = PuzzleUtils.IsFull(voxels, p + left);
		bool r = PuzzleUtils.IsFull(voxels, p + right);
		bool u = PuzzleUtils.IsFull(voxels, p + up);
		bool d = PuzzleUtils.IsFull(voxels, p + down);
		bool lu = PuzzleUtils.IsFull(voxels, p + left + up);
		bool ru = PuzzleUtils.IsFull(voxels, p + right + up);
		bool ld = PuzzleUtils.IsFull(voxels, p + left + down);
		bool rd = PuzzleUtils.IsFull(voxels, p + right + down);
		bool f = PuzzleUtils.IsFull(voxels, p + dir);
		bool fl = PuzzleUtils.IsFull(voxels, p + dir + left);
		bool fr = PuzzleUtils.IsFull(voxels, p + dir + right);
		bool fu = PuzzleUtils.IsFull(voxels, p + dir + up);
		bool fd = PuzzleUtils.IsFull(voxels, p + dir + down);

		void addQuad(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3) {
			var uv0 = new Vector2(p0.Dot(right) / 2 + 0.5f, p0.Dot(up) / 2 + 0.5f);
			var uv1 = new Vector2(p1.Dot(right) / 2 + 0.5f, p1.Dot(up) / 2 + 0.5f);
			var uv2 = new Vector2(p2.Dot(right) / 2 + 0.5f, p2.Dot(up) / 2 + 0.5f);
			var uv3 = new Vector2(p3.Dot(right) / 2 + 0.5f, p3.Dot(up) / 2 + 0.5f);
			p0 = p + offset + dir * 0.5f * Width + p0 * 0.5f;
			p1 = p + offset + dir * 0.5f * Width + p1 * 0.5f;
			p2 = p + offset + dir * 0.5f * Width + p2 * 0.5f;
			p3 = p + offset + dir * 0.5f * Width + p3 * 0.5f;
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

		if (!f) addQuad(left * Width + down * Width, right * Width + down * Width, right * Width + up * Width, left * Width + up * Width);
		if (l && !(fl && f)) addQuad(left + down * Width, left * Width + down * Width, left * Width + up * Width, left + up * Width);
		if (r && !(fr && f)) addQuad(right * Width + down * Width, right + down * Width, right + up * Width, right * Width + up * Width);
		if (u && !(fu && f)) addQuad(left * Width + up * Width, right * Width + up * Width, right * Width + up, left * Width + up);
		if (d && !(fd && f)) addQuad(left * Width + down, right * Width + down, right * Width + down * Width, left * Width + down * Width);
		if (l && u && lu) addQuad(left + up * Width, left * Width + up * Width, left * Width + up, left + up);
		if (r && u && ru) addQuad(right * Width + up * Width, right + up * Width, right + up, right * Width + up);
		if (l && d && ld) addQuad(left + down, left * Width + down, left * Width + down * Width, left + down * Width);
		if (r && d && rd) addQuad(right * Width + down, right + down, right + down * Width, right * Width + down * Width);
	}

	private void LoadState(PuzzlePieceState puzzlePieceState) {
		Position = puzzlePieceState.Offset;
		Rotation = puzzlePieceState.Rotation;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

	}

	// // Called every frame. 'delta' is the elapsed time since the previous frame.
	// private float rotation;
	// public override void _Process(double delta) {
	// 	rotation += (float)delta;
	// 	if (rotation > 1) {
	// 		RotateY(Mathf.Pi / 2);
	// 		rotation -= 1;
	// 	}
	// }
}
