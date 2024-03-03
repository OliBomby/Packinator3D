using Godot;
using System;

public partial class PauseMenu : Node2D
{
	private bool PauseReleased = false;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	public void ShowPauseMenu() {
		if (!GetTree().Paused) {
			GetTree().Paused = true;
			PauseReleased = false;
			Show();
		}
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Input.MouseMode = Input.MouseModeEnum.Visible;
		
		if (!PauseReleased && !Input.IsActionJustPressed("pause")) {
			PauseReleased = true;
		}
		
		if (GetTree().Paused && Input.IsActionJustPressed("pause") && PauseReleased) {
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
}


