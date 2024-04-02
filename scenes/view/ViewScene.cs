using Godot;
using Packinator3D.scenes.menus.main;

namespace Packinator3D.scenes.view;

public partial class ViewScene : Node3D
{
	private PauseMenu pauseMenu;
	private EditMode editMode;
	public bool IsEdit {get; set;}

	public bool IsPaused => pauseMenu.Visible;

	[Export]
	public AudioStream BackSound { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (IsEdit) {
			editMode = new EditMode(IsEdit);
			AddChild(editMode);
		}

		pauseMenu = GetNode<PauseMenu>("PauseMenu");
		pauseMenu.HidePauseMenu();
	}

	public override void _Input(InputEvent @event) {
		if (@event.IsActionPressed("ui_cancel")) {
			GetTree().Root.GetNode<SoundPlayer>("SoundPlayer").Play(BackSound);
			GetTree().ChangeSceneToFile("res://scenes/menus/select/select.tscn");
		}

		if (@event.IsActionPressed("pause") && !pauseMenu.Visible)
			pauseMenu.ShowPauseMenu();
	}
}
