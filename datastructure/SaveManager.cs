using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Godot;
using Packinator3D.datastructure.converters;
using FileAccess = Godot.FileAccess;

namespace Packinator3D.datastructure;

public static class SaveManager {
    private const string SavePath = "user://save_data.json";

    public static SaveData SaveData { get; private set; } = new();

    private static bool initialized;
    private static string lastSaveString;

    private static readonly JsonSerializerOptions jsonOptions = new() {
        Converters = { new Vector3Converter(), new ColorConverter(), new Transform3DConverter() }
    };

    public static bool Init() {
        if (initialized) return false;
        Load();
        initialized = true;
        return true;
    }

    public static void Load() {
        if (!FileAccess.FileExists(SavePath)) return;
        string jsonString = FileAccess.GetFileAsString(SavePath);
        SaveData = JsonSerializer.Deserialize<SaveData>(jsonString, jsonOptions);

        InitNormalPuzzles();
    }

    private static void InitNormalPuzzles() {
        // Load all puzzles from the puzzles directory
        const string puzzleDirectoryPath = "res://puzzles";
        var puzzleDirectory = DirAccess.Open(puzzleDirectoryPath);
        string[] puzzles = puzzleDirectory.GetFiles();

        if (SaveData.Puzzles.Count == puzzles.Length) return;

        SaveData.Puzzles.Clear();
        foreach (string puzzleFileName in puzzles) {
            var puzzle = PuzzleImporter.FromSolution(puzzleDirectoryPath + '/' + puzzleFileName);
            SaveData.Puzzles.Add(puzzle);
        }
    }

    public static void Save() {
        string jsonString = JsonSerializer.Serialize(SaveData, jsonOptions);
        if (jsonString == lastSaveString) return;

        using var saveGame = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
        saveGame.StoreString(jsonString);
        lastSaveString = jsonString;
    }

    public static void ImportPuzzleJson(string path) {
        string jsonString = FileAccess.GetFileAsString(path);
        var puzzle = JsonSerializer.Deserialize<Puzzle>(jsonString, jsonOptions);
        ImportPuzzle(puzzle);
    }

    public static void ImportPuzzle(Puzzle puzzle) {
        SaveData.CustomPuzzles.Add(puzzle);
    }

    public static string ExportPuzzle(Puzzle puzzle) {
        DirAccess.MakeDirAbsolute("user://exports");
        string path = "user://exports/" + MakeValidFileName(puzzle.Name) + ".json";
        string jsonString = JsonSerializer.Serialize(puzzle, jsonOptions);
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
        file.StoreString(jsonString);
        return path;
    }

    public static void RemovePuzzle(Puzzle puzzle) {
        SaveData.CustomPuzzles.Remove(puzzle);
    }

    #region helpers

    private static char[] invalidsCached;

    /// <summary>Replaces characters in <c>text</c> that are not allowed in
    /// file names with the specified replacement character.</summary>
    /// <param name="text">Text to make into a valid filename. The same string is returned if it is valid already.</param>
    /// <param name="replacement">Replacement character, or null to simply remove bad characters.</param>
    /// <param name="fancy">Whether to replace quotes and slashes with the non-ASCII characters ” and ⁄.</param>
    /// <returns>A string that can be used as a filename. If the output string would otherwise be empty, returns "_".</returns>
    public static string MakeValidFileName(string text, char? replacement = '_', bool fancy = false)
    {
        var sb = new StringBuilder(text.Length);
        char[] invalids = invalidsCached ??= Path.GetInvalidFileNameChars();
        var changed = false;
        foreach (char c in text) {
            if (invalids.Contains(c)) {
                changed = true;
                char repl = replacement ?? '\0';
                if (fancy) {
                    if (c == '"')       repl = '”'; // U+201D right double quotation mark
                    else if (c == '\'') repl = '’'; // U+2019 right single quotation mark
                    else if (c == '/')  repl = '⁄'; // U+2044 fraction slash
                }
                if (repl != '\0')
                    sb.Append(repl);
            } else
                sb.Append(c);
        }
        if (sb.Length == 0)
            return "_";
        return changed ? sb.ToString() : text;
    }

    #endregion
}