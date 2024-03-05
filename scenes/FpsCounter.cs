using Godot;

namespace BlockPuzzleViewerSolverEditor.scenes;

public partial class FpsCounter : Label
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Text = $"FPS {Engine.GetFramesPerSecond()}";
	}
}
