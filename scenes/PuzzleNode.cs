using System.Collections.Generic;
using BlockPuzzleViewerSolverEditor.datastructure;
using Godot;

namespace BlockPuzzleViewerSolverEditor.scenes;

public partial class PuzzleNode : Node3D {
	private Puzzle currentPuzzle;
	private List<PuzzlePieceNode> puzzlePieceNodes = new();

	public void LoadMeshes(double Width) {
		var puzzle = PuzzleImporter.FromSolution("res://puzzles/cubishmerhan-4x4x4.txt");
		LoadData(puzzle, Width);
	}
	private void LoadData(Puzzle puzzle, double Width) {
		currentPuzzle = puzzle;
		puzzlePieceNodes.Clear();

		// Add the target shape

		// Add puzzle piece nodes as children
		foreach (var piece in puzzle.Pieces) {
			var puzzlePieceNode = new PuzzlePieceNode(piece, (float)Width);
			AddChild(puzzlePieceNode);
			puzzlePieceNodes.Add(puzzlePieceNode);
		}
	}
	public void SetWidth(float Width) {
		for (var i = 0; i < puzzlePieceNodes.Count; i++) {
			puzzlePieceNodes[i].SetWidth(Width);
		}
	} 
	
	public void SetHeightClip(float Clip) {

	} 
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		LoadMeshes(0.8);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		float t = Mathf.Min((float)(Time.Singleton.GetTicksMsec() % 10000) / 8000, 1);
		for (var i = 0; i < puzzlePieceNodes.Count; i++) {
			puzzlePieceNodes[i].Position = currentPuzzle.Pieces[i].State.Offset.Lerp(currentPuzzle.Solutions[0].States[i].Offset, t);
			puzzlePieceNodes[i].Rotation = currentPuzzle.Pieces[i].State.Rotation.Lerp(currentPuzzle.Solutions[0].States[i].Rotation, t);
		}
	}
}
