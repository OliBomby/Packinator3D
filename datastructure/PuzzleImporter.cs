using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace BlockPuzzleViewerSolverEditor.datastructure;

public static class PuzzleImporter {
    public static Puzzle FromSolution(string path) {
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        var pieces = new List<PuzzlePiece>();
        var solution = new Solution { States = new List<PuzzlePieceState>(), Time = DateTime.Now };
        var index = 0;

        while (!file.EofReached()) {
            string line = file.GetLine();
            if (string.IsNullOrWhiteSpace(line) || line[0] == '#') continue;
            var piece = PieceFromString(line, PuzzleUtils.DefaultColors[index++ % PuzzleUtils.DefaultColors.Length]);
            pieces.Add(piece);
            solution.States.Add(new PuzzlePieceState {
                Offset = piece.State.Offset,
                Rotation = piece.State.Rotation,
            });
        }

        // Move the pieces to the start position
        var states = PuzzleUtils.GetStartStates(pieces);
        for (var i = 0; i < pieces.Count; i++) {
            pieces[i].State = states[i];
        }

        return new Puzzle {
            Name = path.GetFile(),
            Pieces = pieces,
            TargetShape = pieces.SelectMany(o => o.Shape).ToList(),
            Solutions = new List<Solution> { solution }
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
            State = new PuzzlePieceState {
                Offset = center,
                Rotation = Vector3.Zero,
            },
        };
    }

    public static List<Vector3> ShapeFromString(string str) {
        string[] coordSplit = str.Split("   ");
        var shape = new List<Vector3>();
        foreach (string coord in coordSplit) {
            string[] xyz = coord.Split(" ");
            shape.Add(new Vector3(float.Parse(xyz[0]), float.Parse(xyz[1]), float.Parse(xyz[2])));
        }

        return shape;
    }
}