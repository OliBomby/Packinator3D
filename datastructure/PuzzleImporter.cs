using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace BlockPuzzleViewerSolverEditor.datastructure;

public static class PuzzleImporter {
    public static readonly Color[] DefaultColors = {
        Colors.Yellow,
        Colors.Orange,
        Colors.Green,
        Colors.Pink,
        Colors.Red,
        Colors.SkyBlue,
        Colors.Blue,
        Colors.LightGreen,
    };

    public static Puzzle FromSolution(string path) {
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        var pieces = new List<PuzzlePiece>();
        var index = 0;

        while (!file.EofReached()) {
            string line = file.GetLine();
            if (string.IsNullOrWhiteSpace(line)) continue;
            var piece = PieceFromString(line, DefaultColors[index++ % DefaultColors.Length]);
            pieces.Add(piece);
        }

        return new Puzzle {
            Name = path.GetFile(),
            Pieces = pieces,
            TargetShape = pieces.SelectMany(o => o.Shape).ToList(),
            Solutions = new List<Solution> {
                new() {
                    States = pieces.Select(o => o.State).ToList(),
                    Time = DateTime.Now,
                }
            }
        };
    }

    public static PuzzlePiece PieceFromString(string line, Color color) {
        return new PuzzlePiece {
            Shape = ShapeFromString(line),
            Color = color,
            State = new PuzzlePieceState {
                Offset = Vector3.Zero,
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