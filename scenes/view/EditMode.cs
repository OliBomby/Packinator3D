using System.Collections.Generic;
using System;
using Godot;
using Packinator3D.scenes.puzzle;
using Packinator3D.datastructure;

namespace Packinator3D.scenes.view;

partial class EditMode: Node3D {
	private const float RayLength = 1000;

	// Whether we are currently editing or viewing
	private bool currentlyEditing = false;
	private bool editMode = false;
	private int blockIndex = 0;
	private ViewScene viewScene;
	private Camera3D camera;
	private PuzzleNode puzzleNode;

	// List of pieces
	private List<List<BuildingBlock>> pieces;
	private List<Transform3D> pieceStates;

	public EditMode(ViewScene view_scene, bool edit_mode) {
		GD.Print("Constructor called");
		editMode = edit_mode;
		pieces = new();
		pieceStates = new();
		viewScene = view_scene;
	}

	public override void _Ready() {
		GD.Print("Ready called");
		puzzleNode = GetNode<PuzzleNode>("../PuzzleNode");
		camera = GetNode<Camera3D>("../SpectatorCamera"); 
	}
	public override void _Process(double delta) {
		if (Input.IsActionJustPressed("block_build_select")) {
			blockIndex += 1;
			// blockIndex = pieces.Count -> show all
			if (blockIndex > pieces.Count) {
				blockIndex = 0;
			}

			if (blockIndex == pieces.Count) {
				foreach(List<BuildingBlock> piece in pieces) {
					foreach(BuildingBlock block in piece) {
						block.SetTransparency(1.0f);
					}
				}
			}
			else {

				for(int i = 0; i < pieces.Count; i++) {
					foreach(BuildingBlock block in pieces[i]) {
						if (i == blockIndex) {
							block.SetTransparency(1.0f);
						}
						else {
							block.SetTransparency(0.2f);
						}
					}
				}
			}
		}
	}

	public override void _PhysicsProcess(double delta) {
		if (Input.IsActionJustPressed("move_piece")) {
			TryPlaceTargetBlock();
		}
	}


	public void SwitchMode() {
		if (currentlyEditing) {
			// Switch to view mode
			enterViewMode();
		}
		else {
			// Switch to edit mode
			enterEditMode();
		}
		currentlyEditing ^= true;
	}

	private void enterViewMode() {
		// Change target shape
		List<Vector3> target_shape = new();

		foreach(List<BuildingBlock> piece in pieces) {
			foreach(BuildingBlock block in piece) {
				target_shape.Add(block.Position); 
			}
		}

		puzzleNode.AddTargetShape(target_shape);
		puzzleNode.SetTargetShapeVisible(viewScene.IsTargetVisible());
		// Explicitly remove the pieces -> fixes something with collision
		foreach(PuzzlePieceNode ppn in puzzleNode.PuzzlePieceNodes) {
			puzzleNode.RemoveChild(ppn);
		}
		puzzleNode.PuzzlePieceNodes.Clear();

		for (int i = 0; i < pieces.Count; i++) {
			List<BuildingBlock> piece = pieces[i];
			foreach (BuildingBlock block in piece) {
				viewScene.RemoveChild(block);
			}

			PuzzlePiece newPiece = new();
			newPiece.Shape = piece.ConvertAll(block => block.PositionInShape);
			newPiece.Color = piece[0].Color;
			newPiece.State = pieceStates[i];

			puzzleNode.AddPiece(newPiece);
		}

		pieces.Clear();
	}

	private void enterEditMode() {
		puzzleNode.SetTargetShapeVisible(false);
		pieceStates.Clear();

		foreach (var puzzle_piece in puzzleNode.PuzzlePieceNodes) {
			puzzle_piece.Hide();
			pieceStates.Add(puzzle_piece.Transform);

			List<BuildingBlock> piece = new();

			var piece_data = puzzle_piece.PieceData;
			foreach(var voxel_pos in piece_data.Shape) {
				var block = new BuildingBlock(puzzle_piece.Color);
				block.PositionInShape = voxel_pos;
				block.Position = puzzle_piece.Transform * voxel_pos;

				piece.Add(block);
				viewScene.AddChild(block);
			}

			pieces.Add(piece);
		}
	}

	private bool TryPlaceTargetBlock() {
		if (blockIndex == pieces.Count) {
			return true;
		}

		var spaceState = GetWorld3D().DirectSpaceState;
		var mousePos = GetViewport().GetMousePosition();

		var origin = camera.ProjectRayOrigin(mousePos);
		var normal = camera.ProjectRayNormal(mousePos);
		var end = origin + normal * RayLength;
		var query = PhysicsRayQueryParameters3D.Create(origin, end, 3);
		var result = spaceState.IntersectRay(query);

		if (!result.TryGetValue("position", out var position)) return false;
		var pos = (Vector3) position;


		int maxIters = Mathf.FloorToInt(origin.DistanceTo(pos)) * 2;
		var i = 0;

		while (checkForBlock(pos)) {
			pos -= normal * 0.5f;
			if (i++ >= maxIters) return false;
		}

		BuildingBlock block = new(pieces[blockIndex][0].Color);

		GD.Print("Position is ", pos);
		Vector3 block_position = new();
		block_position.X = (float) Math.Floor(pos.X);
		block_position.Y = (float) Math.Floor(pos.Y);
		block_position.Z = (float) Math.Floor(pos.Z);
		GD.Print("block_position is ", block_position);
		block.Position = block_position;

		block.PositionInShape = pieceStates[blockIndex].Inverse() * block_position;
		pieces[blockIndex].Add(block);
		viewScene.AddChild(block);
		return true;
	}

	private bool checkForBlock(Vector3 position) {
		foreach(List<BuildingBlock> piece in pieces) {
			foreach(BuildingBlock block in piece) {
				if (block.Position == position) return true;
			}
		}
		return false;
	}
}
