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
		if (IsEdit) {
			editMode = new EditMode(IsEdit);
			AddChild(editMode);
		}

		pauseMenu = GetNode<PauseMenu>("PauseMenu");
		pauseMenu.Hide();
	}

	public override void _Input(InputEvent @event) {
		if (@event.IsActionPressed("ui_cancel"))
			GetTree().ChangeSceneToFile("res://scenes/menus/select/select.tscn");

		if (@event.IsActionPressed("pause"))
			pauseMenu.ShowPauseMenu();
	}
}
