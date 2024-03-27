using Godot;

namespace Packinator3D.scenes.view;

public partial class Congratulations : Label {
	public override void _Ready() {
		Modulate = new Color(1, 1, 1, 0);
	}

	private void _on_block_placement_controller_puzzle_solved() {
		// Tween the label to fade out after 3 seconds
		var tween = GetTree().CreateTween();
		tween.TweenProperty(this, "modulate", new Color(1, 1, 1, 1), 0.1);
		tween.TweenProperty(this, "modulate", new Color(1, 1, 1, 0), 3);
		GetNode<AudioStreamPlayer>("AudioStreamPlayer").Play();
	}
}