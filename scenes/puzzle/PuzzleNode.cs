using System.Collections.Generic;
using Godot;
using Packinator3D.datastructure;

namespace Packinator3D.scenes.puzzle;

public partial class PuzzleNode : Node3D {
	[Export]
	public string DebugPuzzlePath { get; set; }

	private float width = 0.9f;

	[Export]
	public float Width {
		get => width;
		set => SetWidth(value);
	}
	public Puzzle PuzzleData { get; private set; }

	public readonly List<PuzzlePieceNode> PuzzlePieceNodes = new();
	private MeshInstance3D targetShape;

	public void LoadData(Puzzle puzzle, bool solved=false) {
		PuzzleData = puzzle;

		foreach (var puzzlePieceNode in PuzzlePieceNodes) {
			RemoveChild(puzzlePieceNode);
			puzzlePieceNode.QueueFree();
		}
		PuzzlePieceNodes.Clear();

		if (targetShape != null) {
			RemoveChild(targetShape);
			targetShape.QueueFree();
		}

		// Add the target shape
		AddChild(targetShape = new MeshInstance3D {
			Mesh = PuzzleUtils.ShapeToMesh(puzzle.TargetShape),
			MaterialOverride = new StandardMaterial3D {
				AlbedoColor = Color.Color8(255, 100, 0, 100),
				Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
				DistanceFadeMode = BaseMaterial3D.DistanceFadeModeEnum.PixelAlpha,
				DistanceFadeMaxDistance = 1,
				DistanceFadeMinDistance = 0.3f,
			}
		});

		// Add puzzle piece nodes as children
		foreach (var piece in puzzle.Pieces) {
			var puzzlePieceNode = new PuzzlePieceNode(piece, Width);
			AddChild(puzzlePieceNode);
			PuzzlePieceNodes.Add(puzzlePieceNode);
		}

		if (solved && puzzle.Solutions.Count > 0) {
			// Set the initial state of the puzzle pieces to the solution state
			for (var i = 0; i < PuzzlePieceNodes.Count; i++) {
				PuzzlePieceNodes[i].Transform = PuzzleData.Solutions[0].States[i];
			}
		}
	}

	public void SetWidth(float value) {
		width = value;
		foreach (var piece in PuzzlePieceNodes) {
			piece.SetWidth(value);
		}
	}

	public void SetTargetShapeVisible(bool visible) {
		targetShape.Visible = visible;
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		if (DebugPuzzlePath is not null && PuzzleData is null) {
			LoadData(PuzzleImporter.FromSolution(DebugPuzzlePath));
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		// float t = Mathf.Min((float)(Time.Singleton.GetTicksMsec() % 10000) / 8000, 1);
		// for (var i = 0; i < PuzzlePieceNodes.Count; i++) {
			// PuzzlePieceNodes[i].Position = PuzzleData.Pieces[i].State.Offset.Lerp(currentPuzzle.Solutions[0].States[i].Offset, t);
			// PuzzlePieceNodes[i].Rotation = PuzzleData.Pieces[i].State.Rotation.Lerp(currentPuzzle.Solutions[0].States[i].Rotation, t);
		// }
	}
}
