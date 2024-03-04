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

		Mesh = PuzzleUtils.ShapeToMesh(puzzlePiece.Shape, Width);
		MaterialOverride = new StandardMaterial3D {
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
		};
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
