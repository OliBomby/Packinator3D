using Godot;

namespace BlockPuzzleViewerSolverEditor;

public partial class SpectatorCamera : Camera3D {
	private Vector2 mouseDelta;
	private float totalPitch;

	[Export]
	public float Sensitivity { get; set; } = 0.01f;

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
		mouseDelta *= Sensitivity;
		float yaw = mouseDelta.X;
		float pitch = mouseDelta.Y;
		mouseDelta = Vector2.Zero;
		
		pitch = Mathf.Clamp(pitch, -Mathf.Pi / 2 - totalPitch, Mathf.Pi / 2 - totalPitch);
		totalPitch += pitch;

		RotateY(-yaw);
		RotateObjectLocal(new Vector3(1, 0, 0), -pitch);
	}
}
