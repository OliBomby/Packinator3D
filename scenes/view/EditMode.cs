using System.Collections.Generic;
using Godot;
using Packinator3D.scenes.puzzle;
using Packinator3D.datastructure;

namespace Packinator3D.scenes.view;

public class EditMode {
	// Whether we are currently editing or viewing
	private bool currentlyEditing = false;
	private ViewScene viewScene;
	private PuzzleNode puzzleNode;

	// List of pieces
	private List<List<BuildingBlock>> pieces;
	private List<Transform3D> pieceStates;

	public EditMode(ViewScene view_scene) {
		pieces = new();
		pieceStates = new();
		viewScene = view_scene;
		puzzleNode = viewScene.GetNode<PuzzleNode>("PuzzleNode");
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
}
