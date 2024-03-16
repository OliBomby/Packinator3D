using System.Collections.Generic;
using System.Text.Json;
using Godot;

namespace Packinator3D.datastructure;

public static class SaveManager {
    private const string SavePath = "user://save_data.json";
    public static SaveData SaveData { get; private set; }

    private static string lastSaveString;

    public static void Init() {
        if (SaveData is not null) return;
        Load();
    }

    public static void Load() {
        // Create new save data
        SaveData = new SaveData();

        // Load all puzzles from the puzzles directory
        const string puzzleDirectoryPath = "res://puzzles";
        var puzzleDirectory = DirAccess.Open(puzzleDirectoryPath);
        string[] puzzles = puzzleDirectory.GetFiles();
        foreach (string puzzleFileName in puzzles) {
            var puzzle = PuzzleImporter.FromSolution(puzzleDirectoryPath + '/' + puzzleFileName);
            puzzle.Name = puzzleFileName.GetFile().GetBaseName();
            SaveData.Puzzles.Add(puzzle);
        }

        if (!FileAccess.FileExists(SavePath)) return;
        string jsonString = FileAccess.GetFileAsString(SavePath);
        SaveData.CustomPuzzles = JsonSerializer.Deserialize<List<Puzzle>>(jsonString);
    }

    public static void Save() {
        string jsonString = JsonSerializer.Serialize(SaveData.CustomPuzzles);
        if (jsonString == lastSaveString) return;

        using var saveGame = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
        saveGame.StoreString(jsonString);
        lastSaveString = jsonString;
    }
}