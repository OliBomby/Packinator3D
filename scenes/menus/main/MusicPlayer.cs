using Godot;

namespace Packinator3D.scenes.menus.main;

public partial class MusicPlayer : Node {
    private AudioStreamPlayer soundPlayer;

    public override void _Ready() {
        Name = "MusicPlayer";
        soundPlayer = new AudioStreamPlayer();
        soundPlayer.Bus = "Music";
        soundPlayer.Stream = ResourceLoader.Load<AudioStreamOggVorbis>("res://sounds/music_loop.ogg");
        AddChild(soundPlayer);
        soundPlayer.Play();
    }
}