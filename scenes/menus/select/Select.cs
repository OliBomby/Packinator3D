using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using Packinator3D.datastructure;
using Packinator3D.scenes.puzzle;

namespace Packinator3D.scenes.menus.@select;

public partial class Select : Control
{
	private TabContainer tabContainer;
	private ItemList normalPuzzleList;
	private ItemList customPuzzleList;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		tabContainer = GetNode<TabContainer>("MarginContainer/TabContainer");
		normalPuzzleList = GetNode<ItemList>("MarginContainer/TabContainer/Normal Levels");
		customPuzzleList = GetNode<ItemList>("MarginContainer/TabContainer/Custom Levels");

		foreach (var puzzle in SaveManager.Puzzles) {
			normalPuzzleList.AddItem(puzzle.Name);
		}

		ReloadCustomPuzzles();
	}

	private void ReloadCustomPuzzles() {
		customPuzzleList.Clear();
		foreach (var puzzle in SaveManager.SaveData.CustomPuzzles) {
			customPuzzleList.AddItem(puzzle.Name);
		}
	}

	public override void _ExitTree() {
		SaveManager.Save();
	}

	private void OnNormalPuzzleListItemActivated(int index) {
		LoadPuzzle(SaveManager.Puzzles[index]);
	}

	private void OnCustomPuzzleListItemActivated(int index) {
		LoadPuzzle(SaveManager.SaveData.CustomPuzzles[index]);
	}

	private void LoadPuzzle(Puzzle puzzle, bool solved=false) {
		if (puzzle == null) return;
		var viewScene = ResourceLoader.Load<PackedScene>("res://scenes/view/view.tscn").Instantiate();
		viewScene.GetNode<PuzzleNode>("PuzzleNode").LoadData(puzzle, solved);
		viewScene.GetNode<BlockPlacementController>("BlockPlacementController").ViewSolution = solved;
		var tree = GetTree();
		tree.Root.AddChild(viewScene);
		tree.CurrentScene = viewScene;
		tree.Root.RemoveChild(this);
		QueueFree();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void _Input(InputEvent @event) {
		if (@event.IsActionPressed("ui_cancel")) {
			GoBack();
		}
	}

	private void GoBack() {
		GetTree().ChangeSceneToFile("res://scenes/menus/main/main_menu.tscn");
	}

	private Puzzle GetSelectedPuzzle() {
		int[] selected = tabContainer.CurrentTab == 0 ? normalPuzzleList.GetSelectedItems() : customPuzzleList.GetSelectedItems();
		if (selected.Length == 0) return null;
		return tabContainer.CurrentTab == 0 ? SaveManager.Puzzles[selected[0]] : SaveManager.SaveData.CustomPuzzles[selected[0]];
	}

	private void Play() {
		LoadPuzzle(GetSelectedPuzzle());
	}

	private void View() {
		LoadPuzzle(GetSelectedPuzzle(), true);
	}

	private void Edit() { }

	private void New() { }

	private void Delete() {
		if (tabContainer.CurrentTab != 1) return;
		var puzzle = GetSelectedPuzzle();
		if (puzzle is null) return;
		SaveManager.RemovePuzzle(puzzle);
		ReloadCustomPuzzles();
	}

	private void Import() {
		var dialog = GetNode<FileDialog>("ImportFileDialog");
		dialog.Show();
		if (!FileAccess.FileExists(dialog.CurrentFile)) return;
		SaveManager.ImportPuzzle(dialog.CurrentFile);
		ReloadCustomPuzzles();
		tabContainer.CurrentTab = 1;
	}

	private void Export() {
		var puzzle = GetSelectedPuzzle();
		if (puzzle is null) return;
		string path = SaveManager.ExportPuzzle(GetSelectedPuzzle());
		OS.ShellShowInFileManager(ProjectSettings.GlobalizePath(path));
	}

	private readonly System.Collections.Generic.Dictionary<string, string> blockSolverPaths = new() {
		{ "Windows", "res://lib/blocks-win.exe" },
	};

	private void Solve() {
		if (!blockSolverPaths.TryGetValue(OS.GetName(), out string path)) {
			GD.PrintErr("No block solver available for this platform.");
		} else {
			var puzzle = GetSelectedPuzzle();

			Task.Run(() => {
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

				var output = new Array();
				OS.Execute(exePath, new[] {
					"--solve",
					$"--pieces={piecesPath}",
					$"--goal={goalPath}",
					$"--output={outputPath}"
				}, output);
				GD.Print(output);

				if (output[0].AsString().Contains("Saving solution coordinates to file")) {
					var solution = PuzzleImporter.PuzzleSolutionFromSolution(solutionPath, puzzle);
                    puzzle.Solutions.Add(solution);
                    GD.Print("Successfully added solution to puzzle.");
				}
				else {
					GD.PrintErr("Failed to solve puzzle.");
				}
			});
		}
	}
}