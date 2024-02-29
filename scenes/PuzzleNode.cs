using System.Collections.Generic;
using BlockPuzzleViewerSolverEditor.datastructure;
using Godot;

namespace BlockPuzzleViewerSolverEditor.scenes;

public partial class PuzzleNode : Node3D
{
	private void LoadData(Puzzle puzzle) {
		// Add the target shape

		// Add puzzle piece nodes as children
		foreach (var piece in puzzle.Pieces) {
			var puzzlePieceNode = new PuzzlePieceNode(piece);
			AddChild(puzzlePieceNode);
		}
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		var puzzle = PuzzleImporter.FromSolution("res://puzzles/cubishmerhan-4x4x4.txt");
		LoadData(puzzle);
		// LoadData(new Puzzle {
		// 	Pieces = new List<PuzzlePiece> {
		// 		new() {
		// 			Shape = new List<Vector3> {
		// 				new(0, 0, 0),
		// 				new(0, 0, 1),
		// 				new(0, 1, 1),
		// 				new(0, 1, 2),
		// 			},
		// 			Color = Colors.Aqua,
		// 			State = new() {
		// 				Offset = new Vector3(1, 1, 1),
		// 				Rotation = Vector3.Zero,
		// 			}
		// 		},
		// 		new() {
		// 			Shape = new List<Vector3> {
		// 				new(0, 0, 0),
		// 				new(0, 0, 1),
		// 				new(0, 1, 1),
		// 				new(0, 1, 2),
		// 			},
		// 			Color = Colors.Red,
		// 			State = new() {
		// 				Offset = new Vector3(0, 0, 0),
		// 				Rotation = new Vector3(0, Mathf.Pi / 2, 0),
		// 			}
		// 		},
		// 	}
		// });
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}