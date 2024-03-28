using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Packinator3D.datastructure;

public static class PuzzleImporter {
	/// <summary>
	/// Creates a solution for the puzzle from a solution file.
	/// </summary>
	public static Solution PuzzleSolutionFromSolution(string path, Puzzle puzzle) {
		using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
		var solution = new Solution { States = new List<Transform3D>(), Time = DateTime.Now };
		var index = 0;

		while (!file.EofReached()) {
			string line = file.GetLine();
			if (string.IsNullOrWhiteSpace(line) || line[0] == '#') continue;

			var piece = puzzle.Pieces[index++];
			var shape = ShapeFromString(line);
			var state = PuzzleUtils.FindTransform(piece.Shape, shape);
			solution.States.Add(state);
		}

		return solution;
	}

	public static Puzzle FromSolution(string path) {
		using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
		var pieces = new List<PuzzlePiece>();
		var solution = new Solution { States = new List<Transform3D>(), Time = DateTime.UnixEpoch };
		var targetShape = new List<Vector3>();
		var index = 0;

		while (!file.EofReached()) {
			string line = file.GetLine();
			if (string.IsNullOrWhiteSpace(line) || line[0] == '#') continue;
			var piece = PieceFromString(line, PuzzleUtils.DefaultColors[index++ % PuzzleUtils.DefaultColors.Length]);
			pieces.Add(piece);
			solution.States.Add(piece.State);
			targetShape.AddRange(piece.Shape.Select(v => PuzzleUtils.Transform(v, piece.State)));
		}

		// Move the pieces to the start position
		var states = PuzzleUtils.GetStartStates(pieces);
		for (var i = 0; i < pieces.Count; i++) {
			pieces[i].State = states[i];
		}

		return new Puzzle {
			Name = path.GetFile().GetBaseName(),
			Pieces = pieces,
			TargetShape = targetShape,
			Solutions = new List<Solution> { solution }
		};
	}

	public static Puzzle FromPiecesAndGoal(string piecesPath, string goalPath) {
		using var file1 = FileAccess.Open(piecesPath, FileAccess.ModeFlags.Read);
		using var file2 = FileAccess.Open(goalPath, FileAccess.ModeFlags.Read);
		var pieces = getPieces(file1);
		var goal = getPieces(file2);

		List<PuzzlePiece> getPieces(FileAccess file) {
			var p = new List<PuzzlePiece>();
			var index = 0;
			while (!file.EofReached()) {
				string line = file.GetLine();
				if (string.IsNullOrWhiteSpace(line) || line[0] == '#') continue;
				var piece = PieceFromString(line, PuzzleUtils.DefaultColors[index++ % PuzzleUtils.DefaultColors.Length]);
				p.Add(piece);
			}
			return p;
		}

		// The file with fewer pieces is the goal
		if (pieces.Count < goal.Count) {
			(pieces, goal) = (goal, pieces);
			(piecesPath, goalPath) = (goalPath, piecesPath);
		}

		// Move the pieces to the start position
		var states = PuzzleUtils.GetStartStates(pieces);
		for (var i = 0; i < pieces.Count; i++) {
			pieces[i].State = states[i];
		}

		return new Puzzle {
			Name = $"{piecesPath.GetFile().GetBaseName()}-{goalPath.GetFile().GetBaseName()}",
			Pieces = pieces,
			TargetShape = goal.SelectMany(piece => piece.Shape.Select(v => PuzzleUtils.Transform(v, piece.State))).ToList(),
			Solutions = new List<Solution>()
		};
	}

	public static PuzzlePiece PieceFromString(string line, Color color) {
		var shape = ShapeFromString(line);
		var center = PuzzleUtils.GetCenter(shape);
		for (var i = 0; i < shape.Count; i++) {
			shape[i] -= center;
		}

		return new PuzzlePiece {
			Shape = shape,
			Color = color,
			State = new Transform3D(Basis.Identity, center),
		};
	}

	public static List<Vector3> ShapeFromString(string str) {
		string[] coordSplit = str.Split("   ");
		var shape = new List<Vector3>();
		foreach (string coord in coordSplit) {
			string[] xyz = coord.Split(" ");
			if (xyz.Length != 3) continue;
			shape.Add(new Vector3(float.Parse(xyz[0]), float.Parse(xyz[2]), float.Parse(xyz[1])));
		}

		return shape;
	}
}
