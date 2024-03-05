using BlockPuzzleViewerSolverEditor.datastructure;
using Godot;
using Godot.Collections;

namespace BlockPuzzleViewerSolverEditor.scenes.puzzle;

/// <summary>
/// This class is responsible for controlling the placement of blocks in the puzzle.
/// It requires a PuzzleNode and a Camera3D to be present in the scene.
/// </summary>
public partial class BlockPlacementController : Node3D {
	private const float RayLength = 1000;

	private Camera3D camera;
	private PuzzleNode puzzleNode;
	private PuzzlePieceNode heldPiece;
	private PuzzlePieceState heldPieceOriginalState;
	private Array<Rid> exclude = new();

	[Export]
	private bool ViewSolution { get; set; }

	public override void _Ready() {
		camera = GetNode<Camera3D>("../SpectatorCamera");
		puzzleNode = GetNode<PuzzleNode>("../PuzzleNode");
	}

	public override void _UnhandledInput(InputEvent @event) {
		if (@event.IsActionPressed("move_piece")) {

		}
	}

	public override void _PhysicsProcess(double delta) {
		if (heldPiece != null) {
			var result = ShootRay(3);
			if (result.TryGetValue("position", out var position))
				heldPiece.Position = ((Vector3)position).Round();
		}

		if (Input.IsActionJustPressed("move_piece")) {
			if (heldPiece == null) {
				// Try to pick up a piece
				var result = ShootRay(1);
				if (!result.TryGetValue("collider", out var collider) || collider.Obj is not PuzzlePieceNode piece) return;
				heldPiece = piece;
				heldPieceOriginalState = new PuzzlePieceState(piece.Position, piece.Rotation);
				exclude.Add(heldPiece.GetRid());
			}
			else {
				// Try to place the piece
				ClearHeldPiece();
			}
		}

		if (Input.IsActionJustPressed("reset_piece")) {
			if (heldPiece == null) {
				// Try reset the piece we are looking at
				var result = ShootRay(1);
				if (!result.TryGetValue("collider", out var collider) || collider.Obj is not PuzzlePieceNode piece) return;

				var originalState = piece.PieceData.State;
				var currentState = piece.GetState();

				if (ViewSolution) {
					// Reset the piece to the solution state if not in the solution state
					// Reset the piece to the start state if in the solution state
					int index = puzzleNode.PuzzleData.Pieces.IndexOf(piece.PieceData);
					var solutionState = puzzleNode.PuzzleData.Solutions[0].States[index];

					piece.SetState(currentState.Equals(solutionState) ? originalState : solutionState);
				} else {
					piece.SetState(originalState);
				}
			}
			else {
				// Place the piece back to its original position when we picked it up
				heldPiece.SetState(heldPieceOriginalState);
				ClearHeldPiece();
			}
		}
	}

	private void ClearHeldPiece() {
		heldPiece = null;
		heldPieceOriginalState = null;
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
}
