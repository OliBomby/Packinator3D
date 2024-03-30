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
		editMode = edit_mode;
		pieces = new();
		pieceStates = new();
		viewScene = view_scene;
	}

	public override void _Ready() {
		puzzleNode = GetNode<PuzzleNode>("../PuzzleNode");
		camera = GetNode<Camera3D>("../SpectatorCamera");
		enterEditMode();
	}
	
	private void editDisablePiece(int index) {
		foreach(BuildingBlock block in pieces[index]) {
			block.DisableCollisions();
		}
		puzzleNode.PuzzlePieceNodes[index].SetTransparency(0.4f);
	}

	private void editEnablePiece(int index, bool puzzlePieceCollisions = false) {
		foreach(BuildingBlock block in pieces[index]) {
			block.EnableCollisions();
		}
		puzzleNode.PuzzlePieceNodes[index].SetTransparency(1.0f);

		if (puzzlePieceCollisions) {
			puzzleNode.PuzzlePieceNodes[index].EnableCollisions();
		}
		else {
			puzzleNode.PuzzlePieceNodes[index].DisableCollisions();
		}
	}

	private void buildPuzzlePiece(int index) {
		buildTargetShape();

		puzzleNode.RemoveChild(puzzleNode.PuzzlePieceNodes[index]);

		List<BuildingBlock> piece = pieces[index];

		PuzzlePiece newPiece = new();
		newPiece.Shape = piece.ConvertAll(block => block.PositionInShape);
		newPiece.Color = piece[0].Color;
		newPiece.State = pieceStates[blockIndex];

		puzzleNode.AddPiece(newPiece, index);
		puzzleNode.PuzzlePieceNodes[index].DisableCollisions();
	}

	private void buildTargetShape() {
		List<Vector3> target_shape = new();

		foreach(List<BuildingBlock> p in pieces) {
			foreach(BuildingBlock block in p) {
				target_shape.Add(block.Position); 
			}
		}

		puzzleNode.AddTargetShape(target_shape);
	}

	public override void _PhysicsProcess(double delta) {
		if (Input.IsActionJustPressed("block_build_select")) {
			blockIndex += 1;
			// blockIndex = pieces.Count -> show all
			if (blockIndex > pieces.Count) {
				blockIndex = 0;
			}

			if (blockIndex == pieces.Count) {
				for (int i = 0; i < pieces.Count; i++) {
					editEnablePiece(i, true);
				}
			}
			else {
				for(int i = 0; i < pieces.Count; i++) {
					if (i == blockIndex) {
						editEnablePiece(i);
					}
					else {
						editDisablePiece(i);
					}
				}
			}
		}
		if (Input.IsActionJustPressed("move_piece") && blockIndex != pieces.Count) {
			if (TryPlaceTargetBlock()) {
				buildPuzzlePiece(blockIndex);
			}
		}

		if (Input.IsActionJustPressed("reset_piece") && blockIndex != pieces.Count) {
			if (TryRemoveTargetBlock()) {
				buildPuzzlePiece(blockIndex);
			}
		}
	}


	public void SwitchMode() {
		// if (currentlyEditing) {
		// 	// Switch to view mode
		// 	enterViewMode();
		// }
		// else {
		// 	// Switch to edit mode
		// 	enterEditMode();
		// }
		// currentlyEditing ^= true;
	}

	// private void enterViewMode() {
	// 	// Change target shape
	// 	buildTargetShape();

	// 	// Explicitly remove the pieces -> fixes something with collision
	// 	foreach(PuzzlePieceNode ppn in puzzleNode.PuzzlePieceNodes) {
	// 		puzzleNode.RemoveChild(ppn);
	// 	}
	// 	puzzleNode.PuzzlePieceNodes.Clear();

	// 	for (int i = 0; i < pieces.Count; i++) {
	// 		List<BuildingBlock> piece = pieces[i];

	// 		PuzzlePiece newPiece = new();
	// 		newPiece.Shape = piece.ConvertAll(block => block.PositionInShape);
	// 		newPiece.Color = piece[0].Color;
	// 		newPiece.State = pieceStates[i];

	// 		puzzleNode.AddPiece(newPiece);
	// 	}
	// }

	private void enterEditMode() {
		pieces.Clear();
		pieceStates.Clear();

		foreach (var puzzle_piece in puzzleNode.PuzzlePieceNodes) {
			pieceStates.Add(puzzle_piece.Transform);
			puzzle_piece.PieceData.State = puzzle_piece.Transform;
			List<BuildingBlock> piece = new();

			var piece_data = puzzle_piece.PieceData;
			foreach(var voxel_pos in piece_data.Shape) {
				var block = new BuildingBlock(puzzle_piece.Color);
				block.Hide();

				block.PositionInShape = voxel_pos;
				block.Position = puzzle_piece.Transform * voxel_pos;

				piece.Add(block);
				viewScene.AddChild(block);
			}

			pieces.Add(piece);
		}
		blockIndex = pieces.Count;
		for (int i = 0; i < pieces.Count; i++) {
			editEnablePiece(i, true);
		}
	}

	private bool TryPlaceTargetBlock() {
		if (blockIndex == pieces.Count) {
			return false;
		}

		var spaceState = GetWorld3D().DirectSpaceState;
		var mousePos = GetViewport().GetMousePosition();

		var origin = camera.ProjectRayOrigin(mousePos);
		var normal = camera.ProjectRayNormal(mousePos);
		var end = origin + normal * RayLength;
		var query = PhysicsRayQueryParameters3D.Create(origin, end, uint.MaxValue);
		var result = spaceState.IntersectRay(query);

		if (!result.TryGetValue("position", out var position)) {
			GD.Print("No position");
			return false;
		}
		if (!result.TryGetValue("normal", out var collision_normal)) {
			GD.Print("No normal");
			return false;
		}

		if (!result.TryGetValue("collider", out var collider)) {
			GD.Print("No collider");
			return false;
		}

		var pos = (Vector3) position;
		var norm = (Vector3) collision_normal;
		pos += norm * 0.5f;

		GD.Print("pos: ", pos);
		GD.Print("norm: ", norm);

		int maxIters = Mathf.FloorToInt(origin.DistanceTo(pos)) * 2;
		var i = 0;

		GD.Print("check for block: ", checkForBlock(pos));
		while (checkForBlock(pos.Round())) {
			pos -= normal * 0.5f;
			if (i++ >= maxIters) return false;
		}

		BuildingBlock block = new(pieces[blockIndex][0].Color);
		block.Hide();

		Vector3 block_position = puzzleNode.ToLocal(pos).Round();

		block.Position = block_position;

		block.PositionInShape = pieceStates[blockIndex].Inverse() * block_position;
		pieces[blockIndex].Add(block);
		viewScene.AddChild(block);
		return true;
	}

	private bool TryRemoveTargetBlock() {
		if (blockIndex == pieces.Count) {
			return false;
		}

		var spaceState = GetWorld3D().DirectSpaceState;
		var mousePos = GetViewport().GetMousePosition();

		var origin = camera.ProjectRayOrigin(mousePos);
		var normal = camera.ProjectRayNormal(mousePos);
		var end = origin + normal * RayLength;
		var query = PhysicsRayQueryParameters3D.Create(origin, end, 2);
		var result = spaceState.IntersectRay(query);

		if (!result.TryGetValue("collider", out var collider) || collider.Obj is not BuildingBlock block) return false;
		if (!pieces[blockIndex].Contains(block)) return false;


		bool removed = pieces[blockIndex].Remove(block);
		GD.Print("Removed: ", removed);
		block.QueueFree();
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
