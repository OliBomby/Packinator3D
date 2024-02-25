using Godot;
using System;

public partial class PuzzlePiece : Node3D
{
	Node Voxel;
	public Vector3[] Pieces = {};
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Voxel = GetNode("Voxel");
	
		
		foreach (Vector3 VPos in Pieces) {
			Node V = Voxel.Duplicate();
			AddChild(V);
			(V as Node3D).Visible = true;
			(V as Node3D).Position = VPos;
		}

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
