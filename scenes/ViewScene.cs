using System;
using Godot;

namespace BlockPuzzleViewerSolverEditor.scenes;

public partial class ViewScene : Node3D
{
	PauseMenu PM;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Console.WriteLine("hello world");
		PM = GetNode<PauseMenu>("PauseMenu");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("pause")) {
			PM.ShowPauseMenu();
		}
	}

	public override void _Input(InputEvent @event) {
		if (@event.IsActionPressed("ui_cancel")) {
			GetTree().Quit();
		}
	}
}
