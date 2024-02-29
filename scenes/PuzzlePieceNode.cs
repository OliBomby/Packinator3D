using BlockPuzzleViewerSolverEditor.datastructure;
using Godot;

namespace BlockPuzzleViewerSolverEditor.scenes;

public partial class PuzzlePieceNode : MeshInstance3D
{
	[Export]
	public Color Color { get; set; }

	[Export]
	private float Width { get; set; } = 0.95f;

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
					var p = new Vector3(x, y, z) + offset;
					// Check if the voxel is exposed to air
					if (x == 0 || !voxels[x - 1, y, z]) {
						AddQuad(st, p, Vector3.Left);
					}
					if (x == voxels.GetLength(0) - 1 || !voxels[x + 1, y, z]) {
						AddQuad(st, p, Vector3.Right);
					}
					if (y == 0 || !voxels[x, y - 1, z]) {
						AddQuad(st, p, Vector3.Down);
					}
					if (y == voxels.GetLength(1) - 1 || !voxels[x, y + 1, z]) {
						AddQuad(st, p, Vector3.Up);
					}
					if (z == 0 || !voxels[x, y, z - 1]) {
						AddQuad(st, p, Vector3.Forward);
					}
					if (z == voxels.GetLength(2) - 1 || !voxels[x, y, z + 1]) {
						AddQuad(st, p, Vector3.Back);
					}
				}
			}
		}

		st.Index();
		st.GenerateTangents();
		st.SetMaterial(new StandardMaterial3D {
				AlbedoColor = Color,
				DistanceFadeMode = BaseMaterial3D.DistanceFadeModeEnum.PixelDither,
				DistanceFadeMaxDistance = 1,
				DistanceFadeMinDistance = 0.3f,
		});
		Mesh = st.Commit();
	}

	private void AddQuad(SurfaceTool st, Vector3 p, Vector3 dir) {
		var right = dir.Cross(Vector3.Up).Normalized();
		if (right == Vector3.Zero) right = dir.Cross(Vector3.Forward).Normalized();
		var up = dir.Cross(right).Normalized();
		var p0 = p + dir * 0.5f - right * 0.5f - up * 0.5f;
		var p1 = p + dir * 0.5f + right * 0.5f - up * 0.5f;
		var p2 = p + dir * 0.5f + right * 0.5f + up * 0.5f;
		var p3 = p + dir * 0.5f - right * 0.5f + up * 0.5f;
		st.SetNormal(dir);
		st.SetUV(new Vector2(0, 0));
		st.AddVertex(p0);
		st.SetNormal(dir);
		st.SetUV(new Vector2(0, 1));
		st.AddVertex(p3);
		st.SetNormal(dir);
		st.SetUV(new Vector2(1, 0));
		st.AddVertex(p1);
		st.SetNormal(dir);
		st.SetUV(new Vector2(1, 0));
		st.AddVertex(p1);
		st.SetNormal(dir);
		st.SetUV(new Vector2(0, 1));
		st.AddVertex(p3);
		st.SetNormal(dir);
		st.SetUV(new Vector2(1, 1));
		st.AddVertex(p2);
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
