using Godot;

namespace Packinator3D.scenes;

public partial class Cube : MeshInstance3D
{
	[Export]
	public Color Color { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		Mesh = new BoxMesh { Material = new StandardMaterial3D {
			AlbedoColor = Color,
			DistanceFadeMode = BaseMaterial3D.DistanceFadeModeEnum.PixelDither,
			DistanceFadeMaxDistance = 1,
			DistanceFadeMinDistance = 0.3f,
		}};
		GIMode = GIModeEnum.Dynamic;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// RotateX((float)delta);
		// RotateY((float)delta);
		// RotateZ((float)delta);
		// RotateObjectLocal(Vector3.Up, (float)delta);
	}
}
