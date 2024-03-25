using Godot;

namespace Packinator3D.scenes.menus.@select;

public partial class TasksPanel : Panel {
	private const float MaxWidth = 200;

	private bool expanded;
	private Button collapseArea;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		collapseArea = GetNode<Button>("../CollapseArea");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void _on_tasks_button_pressed()
	{
		expanded = !expanded;

		if (expanded)
			Expand();
		else
			Collapse();
	}

	public override void _Input(InputEvent @event) {
		if (@event.IsActionPressed("ui_cancel")) {
			Collapse();
		}
	}

	private void Expand() {
		expanded = true;

		var tween = CreateTween().SetTrans(Tween.TransitionType.Quint).SetEase(Tween.EaseType.Out);
		tween.TweenProperty(this, "custom_minimum_size", new Vector2(MaxWidth, 0), 0.5);

		collapseArea.Visible = true;
	}

	private void Collapse() {
		expanded = false;

		var tween = CreateTween().SetTrans(Tween.TransitionType.Quint).SetEase(Tween.EaseType.Out);
		tween.TweenProperty(this, "custom_minimum_size", new Vector2(0, 0), 0.5);

		collapseArea.Visible = false;
	}
}