using Godot;

namespace BlockPuzzleViewerSolverEditor;

public partial class SpectatorCamera : Camera3D {
	private Vector2 mouseDelta;
	private float totalPitch;

	[Export] private float Sensitivity { get; set; } = 0.01f;
	[Export] private float Speed { get; set; } = 10f;

	public override void _Ready() {
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _ExitTree() {
		Input.MouseMode = Input.MouseModeEnum.Visible;
	}

	public override void _UnhandledInput(InputEvent @event) {
		if (@event is InputEventMouseMotion mouseMotion) {
			mouseDelta += mouseMotion.Relative;
		}
	}

	public override void _Process(double delta) {
		ProcessMouseLook();
		ProcessMovement(delta);
	}

	private void ProcessMouseLook() {
		if (Input.MouseMode != Input.MouseModeEnum.Captured)
			return;

		mouseDelta *= Sensitivity;
		float yaw = mouseDelta.X;
		float pitch = mouseDelta.Y;
		mouseDelta = Vector2.Zero;

		pitch = Mathf.Clamp(pitch, -Mathf.Pi / 2 - totalPitch, Mathf.Pi / 2 - totalPitch);
		totalPitch += pitch;

		RotateY(-yaw);
		RotateObjectLocal(new Vector3(1, 0, 0), -pitch);
	}

	private void ProcessMovement(double delta) {
		var moveDir = new Vector3();
		if (Input.IsActionPressed("move_forward")) moveDir += Vector3.Forward;
		if (Input.IsActionPressed("move_backward")) moveDir += Vector3.Back;
		if (Input.IsActionPressed("move_left")) moveDir += Vector3.Left;
		if (Input.IsActionPressed("move_right")) moveDir += Vector3.Right;
		if (Input.IsActionPressed("move_up")) moveDir += Vector3.Up;
		if (Input.IsActionPressed("move_down")) moveDir += Vector3.Down;

		var t = Transform;
		moveDir = moveDir.Normalized() * (float)delta * Speed;
		moveDir = moveDir.Rotated(Vector3.Up, Rotation.Y);
		t.Origin += moveDir.Normalized() * (float)delta * Speed;
		Transform = t;
	}
}
