using BlockPuzzleViewerSolverEditor.datastructure;
using Godot;

namespace BlockPuzzleViewerSolverEditor.scenes;

public partial class PuzzlePieceNode : Node3D
{
	[Export]
	public Color Color { get; set; }

	public PuzzlePieceNode(PuzzlePiece puzzlePiece) {
		LoadData(puzzlePiece);
	}

	private void LoadData(PuzzlePiece puzzlePiece) {
		Color = puzzlePiece.Color;
		LoadState(puzzlePiece.State);

		// Create the mesh out of the shape voxels
		foreach (var pos in puzzlePiece.Shape) {
			var cube = new Cube();
			cube.Position = pos;
			cube.Color = Color;
			AddChild(cube);
		}
	}

	private void LoadState(PuzzlePieceState puzzlePieceState) {
		Position = puzzlePieceState.Offset;
		Rotation = puzzlePieceState.Rotation;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	private float rotation;
	public override void _Process(double delta) {
		rotation += (float)delta;
		if (rotation > 1) {
			RotateY(Mathf.Pi / 2);
			rotation -= 1;
		}
	}
}
