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
		for (int x = 0; x < puzzlePiece.Shape.GetLength(0); x++) {
			for (int y = 0; y < puzzlePiece.Shape.GetLength(1); y++) {
				for (int z = 0; z < puzzlePiece.Shape.GetLength(2); z++) {
					if (!puzzlePiece.Shape[x, y, z]) continue;
					var cube = new Cube();
					cube.Position = new Vector3(x, y, z);
					cube.Color = Color;
					AddChild(cube);
				}
			}
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
