using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Godot;

namespace Packinator3D.datastructure;

public static class TaskManager {
    private static List<SolveTask> MyTasks { get; } = new();

    public static IReadOnlyList<SolveTask> Tasks => MyTasks;

    public static event Action<SolveTask> TaskAdded;

    public static event Action<SolveTask> TaskRemoved;

    public static void Add(SolveTask task) {
	    MyTasks.Add(task);
	    TaskAdded?.Invoke(task);
	}

    public static void Remove(SolveTask task) {
	    // Make sure the task is cancelled before removing it
	    if (task.Status == Status.Running)
			task.CancellationToken.Cancel();

	    MyTasks.Remove(task);
	    TaskRemoved?.Invoke(task);
	}

    public static void CancelAll() {
	    foreach (var task in MyTasks) {
		    Cancel(task);
	    }
    }

    public static void Cancel(SolveTask task) {
	    if (task.Status == Status.Running)
		    task.CancellationToken.Cancel();
	}

	private static readonly Dictionary<string, string> blockSolverPaths = new() {
		{ "Windows", "res://lib/blocks-win.exe" },
	};

	public static void Solve(Puzzle puzzle) {
		var solveTask = new SolveTask {
			Id = Guid.NewGuid(),
			Puzzle = puzzle,
			StartTime = DateTime.Now,
			Status = Status.Running,
			CancellationToken = new CancellationTokenSource(),
		};

		solveTask.Task = Task.Run(() => {
			try {
				if (!blockSolverPaths.TryGetValue(OS.GetName(), out string path)) {
					solveTask.Error = "No block solver available for this platform.";
					solveTask.Status = Status.Error;
					GD.PrintErr(solveTask.Error);
					return;
				}

				// Make sure the solver exists as an executable file in the file system
				string exePath = ProjectSettings.GlobalizePath("user://lib/" + path.Split('/')[^1]);
				if (!FileAccess.FileExists(exePath) && FileAccess.FileExists(path)) {
					GD.Print("Copying solver executable file.");
					DirAccess.MakeDirAbsolute("user://lib");
					using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
					using var newFile = FileAccess.Open(exePath, FileAccess.ModeFlags.Write);
					newFile.StoreBuffer(file.GetBuffer((long)file.GetLength()));
					file.Close();
					newFile.Close();
				}

				string piecesPath = ProjectSettings.GlobalizePath("user://temp/pieces.txt");
				string goalPath = ProjectSettings.GlobalizePath("user://temp/goal.txt");
				string outputPath = ProjectSettings.GlobalizePath("user://temp/output.wrl");
				string solutionPath = ProjectSettings.GlobalizePath("user://temp/output.txt");
				DirAccess.MakeDirAbsolute("user://temp");
				PuzzleExporter.ToPieces(puzzle, piecesPath);
				PuzzleExporter.ToGoal(puzzle, goalPath);

				var startInfo = new ProcessStartInfo {
					FileName = exePath,
					Arguments = $"--solve --pieces=\"{piecesPath}\" --goal=\"{goalPath}\" --output=\"{outputPath}\"",
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					CreateNoWindow = true
				};
				var process = Process.Start(startInfo);

				if (process is null) {
					solveTask.Error = "Failed to start process.";
					solveTask.Status = Status.Error;
					return;
				}

				var token = solveTask.CancellationToken.Token;
				while (!process.HasExited) {
					if (!token.IsCancellationRequested) continue;
					process.Kill(true);
					solveTask.Status = Status.Cancelled;
					return;
				}
				string output = process.StandardOutput.ReadToEnd();
				string error = process.StandardError.ReadToEnd();

				if (!string.IsNullOrWhiteSpace(error)) {
					solveTask.Error = error;
					solveTask.Status = Status.Error;
					GD.PrintErr(solveTask.Error);
					return;
				}

				if (output.Contains("Saving solution coordinates to file")) {
					var solution = PuzzleImporter.PuzzleSolutionFromSolution(solutionPath, puzzle);
					puzzle.Solutions.Add(solution);
					solveTask.Solution = solution;
					solveTask.Status = Status.FoundSolution;
					GD.Print("Successfully added solution to puzzle.");
				}
				else {
					solveTask.Status = Status.NoSolution;
					GD.PrintErr("Failed to solve puzzle.");
				}
			}
			catch (Exception e) {
				solveTask.Error = e.Message;
				solveTask.Status = Status.Error;
				GD.PrintErr(solveTask.Error);
			}
		});

		Add(solveTask);
	}
}

public record SolveTask {
    public Guid Id { get; set; }
    public Puzzle Puzzle { get; set; }
    public Solution Solution { get; set; }
    public Status Status { get; set; }
    public string Error { get; set; }
    public DateTime StartTime { get; set; }
    public Task Task { get; set; }
    public CancellationTokenSource CancellationToken { get; set; }
}

public enum Status {
    Running,
    FoundSolution,
    NoSolution,
    Error,
    Cancelled,
}