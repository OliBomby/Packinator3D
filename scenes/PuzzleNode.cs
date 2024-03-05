using System.Collections.Generic;
using BlockPuzzleViewerSolverEditor.datastructure;
using Godot;

namespace BlockPuzzleViewerSolverEditor.scenes;

public partial class PuzzleNode : Node3D {
	[Export]
	public string PuzzlePath { get; set; }

	private float width = 0.8f;

	[Export]
	public float Width {
		get => width;
		set => SetWidth(value);
	}

	private Puzzle currentPuzzle;
	private readonly List<PuzzlePieceNode> puzzlePieceNodes = new();

	private void LoadMeshes() {
		if (string.IsNullOrEmpty(PuzzlePath)) return;
		var puzzle = PuzzleImporter.FromSolution(PuzzlePath);
		LoadData(puzzle);
	}

	private void LoadData(Puzzle puzzle) {
		currentPuzzle = puzzle;
		puzzlePieceNodes.Clear();

		// Add the target shape
		AddChild(new MeshInstance3D {
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
			puzzlePieceNodes.Add(puzzlePieceNode);
		}
	}

	public void SetWidth(float width) {
		this.width = width;
		foreach (var piece in puzzlePieceNodes) {
			piece.SetWidth(width);
		}
	} 
	
	public void SetHeightClip(float clip) {

	} 
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		LoadMeshes();
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
