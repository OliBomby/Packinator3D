using BlockPuzzleViewerSolverEditor.datastructure;
using Godot;

namespace BlockPuzzleViewerSolverEditor.scenes.puzzle;

public partial class PuzzlePieceNode : StaticBody3D
{
	[Export]
	public Color Color { get; set; }

	[Export]
	private float Width { get; set; }

	public PuzzlePiece PieceData { get; private set; }

	private readonly MeshInstance3D renderMesh;

	public PuzzlePieceNode(PuzzlePiece puzzlePieceData, float width = 0.9f) {
		Width = width;
		PieceData = puzzlePieceData;
		AddChild(renderMesh = new MeshInstance3D {
			GIMode = GeometryInstance3D.GIModeEnum.Dynamic
		});
		LoadData(puzzlePieceData);
		CreateCollisionObject();
	}

	public void SetWidth(float width) {
		Width = width;
		UpdateMesh();
	}

	private void UpdateMesh() {
		renderMesh.Mesh = PuzzleUtils.ShapeToMesh(PieceData.Shape, Width);
	}
	
	private void LoadData(PuzzlePiece puzzlePiece) {
		Color = puzzlePiece.Color;
		SetState(puzzlePiece.State);
		UpdateMesh();

		renderMesh.MaterialOverride = new StandardMaterial3D {
			AlbedoColor = Color,
			AlbedoTexture = ResourceLoader.Load<Texture2D>("res://scenes/puzzle/wood/wood_0002_color_1k.jpg"),
			NormalEnabled = true,
			NormalScale = 0.65f,
			NormalTexture = ResourceLoader.Load<Texture2D>("res://scenes/puzzle/wood/wood_0002_normal_opengl_1k.png"),
			RoughnessTexture = ResourceLoader.Load<Texture2D>("res://scenes/puzzle/wood/wood_0002_roughness_1k.jpg"),
			AOTexture = ResourceLoader.Load<Texture2D>("res://scenes/puzzle/wood/wood_0002_ao_1k.jpg"),
			HeightmapEnabled = true,
			HeightmapTexture = ResourceLoader.Load<Texture2D>("res://scenes/puzzle/wood/wood_0002_height_1k.png"),
			ClearcoatEnabled = true,
			Clearcoat = 0.2f,
			DistanceFadeMode = BaseMaterial3D.DistanceFadeModeEnum.PixelDither,
			DistanceFadeMaxDistance = 1,
			DistanceFadeMinDistance = 0.3f,
		};
	}

	private void CreateCollisionObject() {
		uint shapeOwner = CreateShapeOwner(this);
		ShapeOwnerAddShape(shapeOwner, renderMesh.Mesh.CreateTrimeshShape());
	}

	public void SetState(PuzzlePieceState puzzlePieceState) {
		Position = puzzlePieceState.Offset;
		Rotation = puzzlePieceState.Rotation;
	}

	public PuzzlePieceState GetState() {
		return new PuzzlePieceState(Position, Rotation);
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