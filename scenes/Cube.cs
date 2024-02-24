using Godot;

namespace BlockPuzzleViewerSolverEditor.scenes;

public partial class Cube : MeshInstance3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		RotateX((float)delta);
		RotateY((float)delta);
		RotateZ((float)delta);
		RotateObjectLocal(Vector3.Up, (float)delta);
	}
}
