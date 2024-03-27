using System.Linq;
using Godot;
using Godot.Collections;
using Packinator3D.datastructure;

namespace Packinator3D.scenes.puzzle;

/// <summary>
/// This class is responsible for controlling the placement of blocks in the puzzle.
/// It requires a PuzzleNode and a Camera3D to be present in the scene.
/// </summary>
public partial class BlockPlacementController : Node3D {
	private const float RayLength = 1000;

	private Camera3D camera;
	private PuzzleNode puzzleNode;
	private PuzzlePieceNode heldPiece;
	private Transform3D heldPieceOriginalState;
	private Basis targetBasis;
	private Array<Rid> exclude = new();

	[Export]
	public bool IsSolved { get; set; }

	[Export]
	public int ViewSolution { get; set; }

	[Signal]
	public delegate void PuzzleSolvedEventHandler();

	public override void _Ready() {
		camera = GetNode<Camera3D>("../SpectatorCamera");
		puzzleNode = GetNode<PuzzleNode>("../PuzzleNode");
	}

	public override void _UnhandledInput(InputEvent @event) {
		if (heldPiece == null) return;

		if (@event.IsActionPressed("rotate_x+")) DoRotation(Vector3.Right, Mathf.Pi / 2);
		if (@event.IsActionPressed("rotate_x-")) DoRotation(Vector3.Right, -Mathf.Pi / 2);
		if (@event.IsActionPressed("rotate_y+")) DoRotation(Vector3.Up, Mathf.Pi / 2);
		if (@event.IsActionPressed("rotate_y-")) DoRotation(Vector3.Up, -Mathf.Pi / 2);
		if (@event.IsActionPressed("rotate_z+")) DoRotation(Vector3.Forward, Mathf.Pi / 2);
		if (@event.IsActionPressed("rotate_z-")) DoRotation(Vector3.Forward, -Mathf.Pi / 2);
	}

	private void DoRotation(Vector3 axis, float angle) {
		targetBasis = targetBasis.Rotated(axis, angle);
		var tween = CreateTween().SetTrans(Tween.TransitionType.Quint).SetEase(Tween.EaseType.Out);
		tween.TweenProperty(heldPiece, "transform:basis", targetBasis, 0.3);
	}

	public override void _PhysicsProcess(double delta) {
		if (heldPiece != null) {
			SetHeldPieceToValidMousePosition();
		}

		if (Input.IsActionJustPressed("move_piece")) {
			if (heldPiece == null) {
				// Try to pick up a piece
				var result = ShootRay(1);
				if (!result.TryGetValue("collider", out var collider) || collider.Obj is not PuzzlePieceNode piece) return;
				heldPiece = piece;
				heldPieceOriginalState = piece.Transform;
				targetBasis = piece.Basis;
				exclude.Add(heldPiece.GetRid());
				heldPiece.PickUp();
			}
			else {
				// Try to place the piece
				ClearHeldPiece();
				OnStateChanged();
			}
		}

		if (Input.IsActionJustPressed("reset_piece")) {
			if (heldPiece == null) {
				// Try reset the piece we are looking at
				var result = ShootRay(1);
				if (!result.TryGetValue("collider", out var collider) || collider.Obj is not PuzzlePieceNode piece) return;

				var originalState = piece.PieceData.State;
				var currentState = piece.Transform;

				if (ViewSolution >= 0 && ViewSolution < puzzleNode.PuzzleData.Solutions.Count) {
					// Reset the piece to the solution state if not in the solution state
					// Reset the piece to the start state if in the solution state
					int index = puzzleNode.PuzzleData.Pieces.IndexOf(piece.PieceData);
					var solutionState = puzzleNode.PuzzleData.Solutions[ViewSolution].States[index];

					piece.Transform = currentState.Equals(solutionState) ? originalState : solutionState;
				} else {
					piece.Transform = originalState;
				}
				OnStateChanged();
			}
			else {
				// Place the piece back to its original position when we picked it up
				heldPiece.Transform = heldPieceOriginalState;
				ClearHeldPiece();
				OnStateChanged();
			}
		}
	}

	private void SetHeldPieceToValidMousePosition() {
		var spaceState = GetWorld3D().DirectSpaceState;
		var mousePos = GetViewport().GetMousePosition();

		var origin = camera.ProjectRayOrigin(mousePos);
		var normal = camera.ProjectRayNormal(mousePos);
		var end = origin + normal * RayLength;
		var query = PhysicsRayQueryParameters3D.Create(origin, end, 3, exclude);
		var result = spaceState.IntersectRay(query);

		if (!result.TryGetValue("position", out var position)) return;

		// Find a valid position for the piece where it doesnt intersect with the ground or any other pieces
		(bool[,,] voxels, var offset) = PuzzleUtils.ShapeToVoxels(PuzzleUtils.PieceNodesToShape(puzzleNode.PuzzlePieceNodes.Except(new[] { heldPiece})));
		var pos = (Vector3)position;
		int maxIters = Mathf.FloorToInt(origin.DistanceTo(pos)) * 2;
		var i = 0;

		while (!isValidPosition(pos)) {
			pos -= normal * 0.5f;
			if (i++ >= maxIters) return;
		}

		heldPiece.Position = puzzleNode.ToLocal(pos).Round();
		return;

		bool isValidPosition(Vector3 p) {
			var newPos = puzzleNode.ToLocal(p).Round();
			var newState = new Transform3D(targetBasis, newPos);
			foreach (var v in heldPiece.PieceData.Shape) {
				var vp = PuzzleUtils.Transform(v, newState);
				if (vp.Y < 0) return false;
				vp -= offset;
				if (PuzzleUtils.IsFull(voxels, vp)) return false;
			}

			return true;
		}
	}

	private void ClearHeldPiece() {
		heldPiece?.PutDown();
		heldPiece = null;
		exclude.Clear();
	}

	private Dictionary ShootRay(uint collisionMask = 4294967295U) {
		var spaceState = GetWorld3D().DirectSpaceState;
		var mousePos = GetViewport().GetMousePosition();

		var origin = camera.ProjectRayOrigin(mousePos);
		var end = origin + camera.ProjectRayNormal(mousePos) * RayLength;
		var query = PhysicsRayQueryParameters3D.Create(origin, end, collisionMask, exclude);

		return spaceState.IntersectRay(query);
	}

	private void OnStateChanged() {
		if (IsSolved) return;
		if (!puzzleNode.IsSolved()) return;

		var solution = puzzleNode.GetState();
		IsSolved = true;
		puzzleNode.PuzzleData.Solutions.Add(solution);
		EmitSignal(SignalName.PuzzleSolved);
	}
}
