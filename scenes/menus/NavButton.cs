using Godot;
using Packinator3D.scenes.menus.main;

namespace Packinator3D.scenes.menus;

public partial class NavButton : Button
{
	[Export]
	public PackedScene Scene { get; set; }

	[Export]
	public AudioStream Sound { get; set; }

	public override void _Ready() {
		Pressed += OnPressed;
	}

	private void OnPressed() {
		GetTree().Root.GetNode<SoundPlayer>("SoundPlayer").Play(Sound);
		GetTree().ChangeSceneToPacked(Scene);
	}
}