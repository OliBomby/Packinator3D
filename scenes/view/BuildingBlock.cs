using Godot;
using System;


public partial class BuildingBlock : StaticBody3D
{
	public Color Color {get; set;}
	public Vector3 PositionInShape {get; set;}
	private readonly MeshInstance3D renderMesh;
	private uint collisionLayer = 0b110;
	

	public BuildingBlock(Color? color = null) {
		PositionInShape = new();
		Color = color.GetValueOrDefault(Color.Color8(255, 100, 0, 255));

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

		StandardMaterial3D material = renderMesh.GetSurfaceOverrideMaterial(0) as StandardMaterial3D;
		Color c = material.AlbedoColor;
		c.A = a;
		material.AlbedoColor = c;
		
		if (a == 1.0f) {
			material.Transparency = BaseMaterial3D.TransparencyEnum.Disabled;
		}
		else {
			material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
		}


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
		ShapeOwnerAddShape(shapeOwner, renderMesh.Mesh.CreateTrimeshShape());
		this.CollisionMask = collisionLayer;
		this.CollisionLayer = collisionLayer;
	}

	public void DisableCollisions() {
		this.CollisionMask = 0b100;	
		this.CollisionLayer = 0b100;
	}

	public void EnableCollisions() {
		this.CollisionMask = collisionLayer;
		this.CollisionLayer = collisionLayer;
	}
}
