using Godot;
using Packinator3D.datastructure;
using Packinator3D.scenes.menus.main;

namespace Packinator3D.scenes.menus.@select.tasks;

public partial class TaskDisplay : Control {
	private Label label;
	private TextureProgressBar progressBar;
	private Button cancelButton;

	public SolveTask Task { get; set; }

	[Export]
	public AudioStream CancelSound { get; set; }

	[Export]
	public AudioStream RemoveSound { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		label = GetNode<Label>("ColorRect/Label");
		progressBar = GetNode<TextureProgressBar>("ColorRect/TextureProgressBar");
		cancelButton = GetNode<Button>("ColorRect/TextureProgressBar/Button");

		var tween = GetTree().CreateTween().SetLoops().BindNode(this);
		tween.TweenProperty(progressBar, "radial_initial_angle", 360, 1.5).AsRelative();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		if (Task == null) return;

		switch (Task.Status) {
			case Status.Running:
				label.Text = "Solving\n" + Task.Puzzle.Name;
				progressBar.RadialFillDegrees = 60;
				break;
			case Status.FoundSolution:
				label.Text = Task.Puzzle.Name + "\nSolved!";
				progressBar.RadialFillDegrees = 360;
				break;
			case Status.NoSolution:
				label.Text = Task.Puzzle.Name + "\nNo solution found";
				progressBar.RadialFillDegrees = 360;
				break;
			case Status.Cancelled:
				label.Text = Task.Puzzle.Name + "\nCancelled";
				progressBar.RadialFillDegrees = 0;
				break;
			case Status.Error:
				label.Text = Task.Puzzle.Name + "\nError";
				progressBar.RadialFillDegrees = 0;
				break;
		}
	}

	private void _on_mouse_entered() {
		cancelButton.Show();
	}

	private void _on_mouse_exited() {
		cancelButton.Hide();
	}

	private void _on_button_pressed() {
		if (Task.Status == Status.Running && Task != null) {
			GetTree().Root.GetNode<SoundPlayer>("SoundPlayer").Play(CancelSound);
			TaskManager.Cancel(Task);
		}
		else if (Task != null) {
			GetTree().Root.GetNode<SoundPlayer>("SoundPlayer").Play(RemoveSound);
			TaskManager.Remove(Task);
		}
	}
}