using Godot;
using Packinator3D.datastructure;

namespace Packinator3D.scenes.menus.main;

public partial class SoundPlayer : Node {
    private AudioStreamPlayer soundPlayer;

    public override void _Ready() {
        Name = "SoundPlayer";
        soundPlayer = new AudioStreamPlayer();
        soundPlayer.Bus = "Effects";
        AddChild(soundPlayer);
    }

    public void Play(AudioStream sound) {
        soundPlayer.Stream = sound;
        soundPlayer.Play();
    }
}