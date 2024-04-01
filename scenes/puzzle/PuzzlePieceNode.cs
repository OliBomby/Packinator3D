using System;
using Godot;
using Packinator3D.datastructure;

namespace Packinator3D.scenes.puzzle;

public partial class PuzzlePieceNode : StaticBody3D
{
	[Export]
	public Color Color { get; set; }

	[Export]
	private float Width { get; set; }

	public PuzzlePiece PieceData { get; private set; }

	private readonly MeshInstance3D renderMesh;

	private uint collisionLayer = 1;

	public PuzzlePieceNode(PuzzlePiece puzzlePieceData, float width = 0.9f) {
		Width = width;
		PieceData = puzzlePieceData;
		AddChild(renderMesh = new MeshInstance3D {
			GIMode = GeometryInstance3D.GIModeEnum.Static
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

	public void SetTransparency(float a) {

		var material = (StandardMaterial3D)renderMesh.GetSurfaceOverrideMaterial(0);
		var c = material.AlbedoColor;
		c.A = a;
		material.AlbedoColor = c;
		material.Transparency = Math.Abs(a - 1.0f) < 1E-5f ?
			BaseMaterial3D.TransparencyEnum.Disabled :
			BaseMaterial3D.TransparencyEnum.Alpha;

		renderMesh.SetSurfaceOverrideMaterial(
			0,
			material
		);
	}
	
	private void LoadData(PuzzlePiece puzzlePiece) {
		Color = puzzlePiece.Color;
		Transform = puzzlePiece.State;
		UpdateMesh();

		renderMesh.SetSurfaceOverrideMaterial(0, new StandardMaterial3D {
			AlbedoColor = Color,
			AlbedoTexture = ResourceLoader.Load<Texture2D>("res://scenes/puzzle/wood/wood_0002_color_1k.jpg"),
			NormalEnabled = true,
			NormalScale = 0.95f,
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
		});
	}

	public void PickUp() {
		if (renderMesh.MaterialOverride is not StandardMaterial3D material) return;
		material.AlbedoColor = Color * 1.5f;
	}

	public void PutDown() {
		if (renderMesh.MaterialOverride is not StandardMaterial3D material) return;
		material.AlbedoColor = Color;
	}

	private void CreateCollisionObject() {
		uint shapeOwner = CreateShapeOwner(this);
		ShapeOwnerAddShape(shapeOwner, renderMesh.Mesh.CreateTrimeshShape());
		this.CollisionMask = collisionLayer;
		this.CollisionLayer = collisionLayer;
	}

	public void DisableCollisions() {
		this.CollisionMask = 0;	
		this.CollisionLayer = 0;
	}

	public void EnableCollisions() {
		this.CollisionMask = collisionLayer;
		this.CollisionLayer = collisionLayer;
	}
}
