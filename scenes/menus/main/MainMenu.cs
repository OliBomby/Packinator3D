using Godot;
using Packinator3D.datastructure;

namespace Packinator3D.scenes.menus.main;

public partial class MainMenu : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (SaveManager.Init()) {
			GetTree().Root.CallDeferred("add_child", new QuitHandler());
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void _on_play_pressed() {
		GetTree().ChangeSceneToFile("res://scenes/menus/select/select.tscn");
	}

	private void _on_options_pressed() {
		// Replace with function body.
	}

	private void _on_quit_pressed() {
		GetTree().Root.PropagateNotification((int)NotificationWMCloseRequest);
		GetTree().Quit();
	}

	public override void _Input(InputEvent @event) {
		if (@event.IsActionPressed("ui_cancel")) {
			GetTree().Root.PropagateNotification((int)NotificationWMCloseRequest);
			GetTree().Quit();
		}
	}
}
