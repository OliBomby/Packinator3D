using Godot;
using System;

public partial class Puzzle : Node3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		PuzzlePiece PPNode = GetNode<PuzzlePiece>("PuzzlePiece");
		PuzzlePiece PP = PPNode.Duplicate() as PuzzlePiece;
		PP.Pieces = new Vector3[] {
			new Vector3(0, 0, 0),
			new Vector3(1, 0, 0),
			new Vector3(1, 1, 0),
			new Vector3(2, 1, 0)
		};
		
		
		PuzzlePiece PP2 = PPNode.Duplicate() as PuzzlePiece;
		PP2.Pieces = new Vector3[] {
			new Vector3(0, 0, 2),
			new Vector3(1, 0, 2),
			new Vector3(1, 1, 2),
			new Vector3(2, 1, 2)
		};
		
		AddChild(PP);
		AddChild(PP2);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
