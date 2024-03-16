using System.Collections.Generic;
using System.Text.Json;
using Godot;

namespace Packinator3D.datastructure;

public static class SaveManager {
    private const string SavePath = "user://save_data.json";

    public static SaveData SaveData { get; private set; } = new();
    public static List<Puzzle> Puzzles { get; set; }

    private static string lastSaveString;

    public static void Init() {
        if (Puzzles is not null) return;
        Load();
    }

    public static void Load() {
        // Load all puzzles from the puzzles directory
        const string puzzleDirectoryPath = "res://puzzles";
        Puzzles = new List<Puzzle>();
        var puzzleDirectory = DirAccess.Open(puzzleDirectoryPath);
        string[] puzzles = puzzleDirectory.GetFiles();
        foreach (string puzzleFileName in puzzles) {
            var puzzle = PuzzleImporter.FromSolution(puzzleDirectoryPath + '/' + puzzleFileName);
            puzzle.Name = puzzleFileName.GetFile().GetBaseName();
            Puzzles.Add(puzzle);
        }

        if (!FileAccess.FileExists(SavePath)) return;
        string jsonString = FileAccess.GetFileAsString(SavePath);
        SaveData = JsonSerializer.Deserialize<SaveData>(jsonString);
    }

    public static void Save() {
        string jsonString = JsonSerializer.Serialize(SaveData);
        if (jsonString == lastSaveString) return;

        using var saveGame = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
        saveGame.StoreString(jsonString);
        lastSaveString = jsonString;
    }
}