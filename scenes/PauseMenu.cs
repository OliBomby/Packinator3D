using Godot;

namespace BlockPuzzleViewerSolverEditor.scenes;

public partial class PauseMenu : Node2D
{
	private bool pauseReleased;

	private puzzle.PuzzleNode puzzleNode;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		puzzleNode = GetNode<puzzle.PuzzleNode>("../PuzzleNode");
	}

	public void ShowPauseMenu() {
		if (!GetTree().Paused) {
			GetTree().Paused = true;
			pauseReleased = false;
			Show();
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Input.MouseMode = Input.MouseModeEnum.Visible;

		if (!pauseReleased && !Input.IsActionJustPressed("pause")) {
			pauseReleased = true;
		}

		if (GetTree().Paused && Input.IsActionJustPressed("pause") && pauseReleased) {
			OnCloseButtonPressed();
		}
	}

	private void OnCloseButtonPressed()
	{
		Hide();
		Input.MouseMode = Input.MouseModeEnum.Captured;
		GetTree().Paused = false;
	}

	private void _on_close_button_pressed()
	{
		OnCloseButtonPressed();
	}

	private void _on_piece_width_value_changed(double value)
	{
		puzzleNode.SetWidth((float) value);
	}

	private void _on_height_clip_value_changed(double value)
	{
		Node3D Clip = GetNode<Node3D>("../HeightClip");
		if (value == 0.0) {
			Clip.Hide();
		}
		else {
			Clip.Show();
		}


		Clip.Position = new Vector3(0.0f, (float) value, 0.0f);
	}

	private void _on_check_box_toggled(bool toggledOn)
	{
		puzzleNode.SetTargetShapeVisible(toggledOn);
	}
}
