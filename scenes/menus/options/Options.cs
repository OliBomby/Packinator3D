using Godot;
using Packinator3D.scenes.menus.main;

namespace Packinator3D.scenes.menus.options;

public partial class Options : Control
{
	[Export]
	public AudioStream BackSound { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void _Input(InputEvent @event) {
		if (@event.IsActionPressed("ui_cancel")) {
			GoBack();
		}
	}

	private void GoBack() {
		GetTree().Root.GetNode<SoundPlayer>("SoundPlayer").Play(BackSound);
		GetTree().ChangeSceneToFile("res://scenes/menus/main/main_menu.tscn");
	}
}