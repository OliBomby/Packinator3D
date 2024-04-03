using Godot;
using System;
using System.Collections.Generic;

using Packinator3D.scenes.puzzle;
using Packinator3D.datastructure;

public partial class PuzzleBackground : Godot.Node3D
{
	private bool loaded = false;
	private PuzzleNode puzzleNode;
	private List<Vector3> cogs = new();
	private Camera3D camera;

	private float explodeFactor = 2.0f;
	private float explodeSpeed = 0.1f;
	private float explodeState = 0.0f;
	private bool explodeForwards = true;
	private List<Vector3> startingPositions = new();
	private List<Vector3> endPositions = new();

	private float cameraRadius = 10.0f;
	private float currentAngle = 0.0f;
	private float rotationSpeed = 0.1f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		puzzleNode = new PuzzleNode();
		camera = GetNode<Camera3D>("../Camera3D");
		AddChild(puzzleNode);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {

		// This fixes some race condition
		if ((!loaded) && SaveManager.SaveData.Puzzles.Count > 0) {
			loaded = true;
			var random = new Random();
			puzzleNode.LoadData(SaveManager.SaveData.Puzzles[random.Next(SaveManager.SaveData.Puzzles.Count)], 0);

			computeCenterOfGravityPieces();
			Vector3 centerOfGravity = computeCenterOfGravity();
			puzzleNode.Transform = puzzleNode.Transform.Translated(-centerOfGravity);

			var i = 0;
			foreach(var ppn in puzzleNode.PuzzlePieceNodes) {

				startingPositions.Add(ppn.Position);
				endPositions.Add(cogs[i] * explodeFactor);
				i += 1;
			}


			centerOfGravity = computeCenterOfGravity();
		}
		
		currentAngle += (float)delta * rotationSpeed;
		var cam_pos = camera.Position;
		cam_pos.X = cameraRadius * (float)Math.Cos(currentAngle);
		cam_pos.Z = cameraRadius * (float)Math.Sin(currentAngle);
		camera.Position = cam_pos;

		camera.LookAt(new Vector3());

		if (explodeForwards) {
			explodeState += explodeSpeed * (float) delta;
			if (explodeState > 1.0f) {
				explodeForwards = false;
			}
		}
		else {
			explodeState -= explodeSpeed * (float) delta;
			if (explodeState < 0.0f) {
				explodeForwards = true;
			}
		}

		for (int i = 0; i < puzzleNode.PuzzlePieceNodes.Count; i++) {
			puzzleNode.PuzzlePieceNodes[i].Position = 
				startingPositions[i].Lerp(endPositions[i], explodeState);	
		}
	}
	
	private Vector3 computeCenterOfGravity() {

		Vector3 sum = new();
		foreach(var cog in cogs) {
			sum += cog;
		}

		return sum / cogs.Count;
	}

	private void computeCenterOfGravityPieces() {
		cogs.Clear();
		foreach(var ppn in puzzleNode.PuzzlePieceNodes) {
			Vector3 sum = new();
			var shape = ppn.PieceData.Shape;
			foreach(Vector3 p in shape) {
				sum += ppn.Transform * (p + new Vector3(0f, 0f, -0.5f));
			}
			cogs.Add(sum / shape.Count);
		}
	}
}
