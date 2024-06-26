using System;
using System.Collections.Generic;
using System.Linq;
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

	public void AddPiece(PuzzlePiece piece, int? index = null) {
		var puzzlePieceNode = new PuzzlePieceNode(piece, Width);
		AddChild(puzzlePieceNode);
		if (index is { } i && i < PuzzlePieceNodes.Count) {
			PuzzlePieceNodes[i] = puzzlePieceNode;
		}
		else {
			PuzzlePieceNodes.Add(puzzlePieceNode);
		}
	}

	public void AddTargetShape(List<Vector3> shape) {
		var visible = true;

		if (targetShape is not null) {
			visible = targetShape.Visible;

			RemoveChild(targetShape);
			targetShape.QueueFree();
			targetShape = null;
		}

		AddChild(targetShape = new MeshInstance3D {
			Mesh = PuzzleUtils.ShapeToMesh(shape),
			MaterialOverride = new StandardMaterial3D {
				AlbedoColor = Color.Color8(255, 100, 0, 100),
				Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
				DistanceFadeMode = BaseMaterial3D.DistanceFadeModeEnum.PixelAlpha,
				DistanceFadeMaxDistance = 1,
				DistanceFadeMinDistance = 0.3f,
			}
		});
		SetTargetShapeVisible(visible);
	}

	public void LoadData(Puzzle puzzle, int solutionIndex=-1, bool generateStartStates=false) {
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
		AddTargetShape(puzzle.TargetShape);

		// Only show the target shape initially if there is no solution
		SetTargetShapeVisible(solutionIndex < 0 && generateStartStates);

		// Add puzzle piece nodes as children
		foreach (var piece in puzzle.Pieces) {
			AddPiece(piece);
		}

		if (generateStartStates) {
			// Move the pieces to the start position
			var states = PuzzleUtils.GetStartStates(puzzle.Pieces);
			for (var i = 0; i < PuzzlePieceNodes.Count; i++) {
				PuzzlePieceNodes[i].Transform = states[i];
				PuzzlePieceNodes[i].InitialState = states[i];
				PuzzlePieceNodes[i].OtherState = states[i];
			}
		}

		if (solutionIndex >= 0 && solutionIndex < puzzle.Solutions.Count) {
			// Set the initial state of the puzzle pieces to the solution state
			for (var i = 0; i < PuzzlePieceNodes.Count; i++) {
				PuzzlePieceNodes[i].Transform = PuzzleData.Solutions[solutionIndex].States[i];
				PuzzlePieceNodes[i].OtherState = PuzzleData.Solutions[solutionIndex].States[i];
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
		if (targetShape is null) return;
		targetShape.Visible = visible;
	}

	public bool TargetShapeVisible => targetShape?.Visible ?? false;
	
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

	public bool IsSolved() {
		return PuzzleData.TargetShape.Count == 0 ||
		       PuzzleUtils.IsSolution(PuzzleData.Pieces, PuzzlePieceNodes.Select(p => p.Transform), PuzzleData.TargetShape);
	}

	public Solution GetState() {
		return new Solution {
			States = PuzzlePieceNodes.Select(p => p.Transform).ToList(),
			Time = DateTime.Now
		};
	}
}
