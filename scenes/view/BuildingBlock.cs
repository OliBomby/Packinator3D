using System;
using Godot;

namespace Packinator3D.scenes.view;

public partial class BuildingBlock : StaticBody3D
{
	public Color Color {get; set;}
	public Vector3 PositionInShape {get; set;}
	private readonly MeshInstance3D renderMesh;
	private const uint SelectedCollisionLayer = 0b101;
	private const uint DeselectedCollisionLayer = 0b001;


	public BuildingBlock(Color? color = null) {
		PositionInShape = new Vector3();
		Color = color.GetValueOrDefault(Color.Color8(255, 100, 0));

		var mesh = new BoxMesh();
		var material = new StandardMaterial3D {
			AlbedoColor = Color,
			Transparency = BaseMaterial3D.TransparencyEnum.Disabled,
			NormalEnabled = true,
		};

		renderMesh = new MeshInstance3D {
			Mesh = mesh,
			MaterialOverride =	material,
		};

		renderMesh.SetSurfaceOverrideMaterial(
			0,
			material
		);
		CreateCollisionObject();
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

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		AddChild(renderMesh);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void CreateCollisionObject() {
		uint shapeOwner = CreateShapeOwner(this);
		ShapeOwnerAddShape(shapeOwner, new BoxShape3D());
		CollisionMask = SelectedCollisionLayer;
		CollisionLayer = SelectedCollisionLayer;
	}

	public void DisableCollisions() {
		CollisionMask = DeselectedCollisionLayer;
		CollisionLayer = DeselectedCollisionLayer;
	}

	public void EnableCollisions() {
		CollisionMask = SelectedCollisionLayer;
		CollisionLayer = SelectedCollisionLayer;
	}
}