using System.Collections.Generic;
using System.Linq;
using System.Text;
using Godot;

namespace Packinator3D.datastructure;

public static class PuzzleExporter {
    /// <summary>
    /// Exports the puzzle pieces to a file.
    /// </summary>
    public static void ToPieces(Puzzle puzzle, string path) {
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);

        foreach (var piece in puzzle.Pieces) {
            string line = PieceToString(piece);
            file.StoreLine(line);
        }

        file.Close();
    }

    /// <summary>
    /// Exports the goal shape to a file.
    /// </summary>
    public static void ToGoal(Puzzle puzzle, string path) {
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
        string line = ShapeToString(puzzle.TargetShape);
        file.StoreLine(line);
        file.Close();
    }

    private static string PieceToString(PuzzlePiece piece) {
        return ShapeToString(piece.Shape);
    }

    private static string ShapeToString(IEnumerable<Vector3> shape) {
        var sb = new StringBuilder();
        sb.AppendJoin("   ", shape.Select(vertex =>
            $"{Mathf.RoundToInt(vertex.X)} {Mathf.RoundToInt(vertex.Z)} {Mathf.RoundToInt(vertex.Y)}"
        ));

        return sb.ToString();
    }
}