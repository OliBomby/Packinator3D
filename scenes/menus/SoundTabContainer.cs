using Godot;

namespace Packinator3D.scenes.menus;

public partial class SoundTabContainer : TabContainer
{
	[Export]
	public AudioStream Sound { get; set; }

	private AudioStreamPlayer soundPlayer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		soundPlayer = new AudioStreamPlayer();
		soundPlayer.Bus = "Effects";
		soundPlayer.Stream = Sound;
		AddChild(soundPlayer);

		TabChanged += _ => soundPlayer.Play();
	}
}