using Godot;
using System;


public partial class TargetBuildingBlock : StaticBody3D
{
	private readonly MeshInstance3D renderMesh;
	public TargetBuildingBlock() {
		GD.Print("AAA");
		var st = new SurfaceTool();

		st.Begin(Mesh.PrimitiveType.Triangles);

		// Prepare attributes for AddVertex.
		st.AddVertex(new Vector3(0.5f, 1, 0.5f));
		st.AddVertex(new Vector3(-0.5f, 1, 0.5f));
		st.AddVertex(new Vector3(-0.5f, 0, 0.5f));
		st.AddVertex(new Vector3(0.5f, 0, 0.5f));

		st.AddVertex(new Vector3(0.5f, 0, -0.5f));
		st.AddVertex(new Vector3(0.5f, 1, -0.5f));
		st.AddVertex(new Vector3(-0.5f, 1, -0.5f));
		st.AddVertex(new Vector3(-0.5f, 0, -0.5f));

		st.AddIndex(0);
		st.AddIndex(2);
		st.AddIndex(1);

		st.AddIndex(0);
		st.AddIndex(3);
		st.AddIndex(2);

		st.AddIndex(0);
		st.AddIndex(4);
		st.AddIndex(3);

		st.AddIndex(0);
		st.AddIndex(5);
		st.AddIndex(4);

		st.AddIndex(0);
		st.AddIndex(6);
		st.AddIndex(5);

		st.AddIndex(0);
		st.AddIndex(1);
		st.AddIndex(6);

		st.AddIndex(1);
		st.AddIndex(7);
		st.AddIndex(6);

		st.AddIndex(1);
		st.AddIndex(2);
		st.AddIndex(7);

		st.AddIndex(7);
		st.AddIndex(3);
		st.AddIndex(4);

		st.AddIndex(7);
		st.AddIndex(2);
		st.AddIndex(3);

		st.AddIndex(4);
		st.AddIndex(6);
		st.AddIndex(7);

		st.AddIndex(4);
		st.AddIndex(5);
		st.AddIndex(6);

		st.GenerateNormals();

		// Commit to a mesh.
		var mesh = st.Commit();
		renderMesh = new MeshInstance3D {
			Mesh = mesh,
			MaterialOverride = new StandardMaterial3D {
				AlbedoColor = Color.Color8(255, 100, 0, 255),
				Transparency = BaseMaterial3D.TransparencyEnum.Disabled,
			}
		};
		CreateCollisionObject();
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
	}
}
