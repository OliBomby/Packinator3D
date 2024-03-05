using Godot;

namespace BlockPuzzleViewerSolverEditor.scenes.view;

public partial class ViewScene : Node3D
{
	private PauseMenu pauseMenu;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		pauseMenu = GetNode<PauseMenu>("PauseMenu");
		pauseMenu.Hide();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("pause")) {
			pauseMenu.ShowPauseMenu();
		}
	}

	public override void _Input(InputEvent @event) {
		if (@event.IsActionPressed("ui_cancel")) {
			GetTree().Quit();
		}
	}
}
