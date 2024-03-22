using Godot;
using System;


public partial class TargetBuildingBlock : StaticBody3D
{
	public TargetBuildingBlock() {
		var st = new SurfaceTool();

		st.Begin(Mesh.PrimitiveType.Triangles);

		// Prepare attributes for AddVertex.
		st.AddVertex(new Vector3(1.5f, 1, 1.5f));
		st.AddVertex(new Vector3(0.5f, 1, 1.5f));
		st.AddVertex(new Vector3(0.5f, 0, 1.5f));
		st.AddVertex(new Vector3(1.5f, 0, 1.5f));

		st.AddVertex(new Vector3(1.5f, 0, 0.5f));
		st.AddVertex(new Vector3(1.5f, 1, 0.5f));
		st.AddVertex(new Vector3(0.5f, 1, 0.5f));
		st.AddVertex(new Vector3(0.5f, 0, 0.5f));

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

		// Commit to a mesh.
		var mesh = st.Commit();

		AddChild(renderMesh = new MeshInstance3D {
			Mesh = mesh,
			MaterialOverride = new StandardMaterial3D {
				AlbedoColor = Color.Color8(255, 100, 0, 255),
				Transparency = BaseMaterial3D.TransparencyEnum.Disabled,
			}
		});
		CreateCollisionObject();
	}
	private readonly MeshInstance3D renderMesh;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	
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