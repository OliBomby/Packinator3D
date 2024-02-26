using Godot;
using System;

public partial class PuzzlePiece : Node3D
{
	MeshInstance3D Voxel;
	public Vector3[] Pieces = {};

	[Export]
	public Color Color { get; set; }
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Voxel = GetNode<MeshInstance3D>("Voxel");
		((StandardMaterial3D)Voxel.MaterialOverride).AlbedoColor = Color;
	
		
		foreach (Vector3 VPos in Pieces) {
			var V = Voxel.Duplicate();
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
