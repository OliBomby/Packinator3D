using Godot;

namespace Packinator3D.scenes.view;

public partial class ViewScene : Node3D
{
	private PauseMenu pauseMenu;
	private EditMode editMode;
	public bool IsEdit {get; set;}

	public bool IsTargetVisible() {
		return pauseMenu.IsTargetVisible;
	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		editMode = new EditMode(this);
		pauseMenu = GetNode<PauseMenu>("PauseMenu");
		pauseMenu.Hide();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("pause")) {
			pauseMenu.ShowPauseMenu();
		}

		if (IsEdit) {
			if (Input.IsActionJustPressed("edit_mode")) {
				editMode.SwitchMode();
			}
		}
	}

	public override void _Input(InputEvent @event) {
		if (@event.IsActionPressed("ui_cancel")) {
			GetTree().ChangeSceneToFile("res://scenes/menus/select/select.tscn");
		}
	}
}
