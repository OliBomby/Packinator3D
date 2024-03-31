using Godot;
using Packinator3D.scenes.view;
using Packinator3D.datastructure;
using Packinator3D.scenes.menus.select.tasks;
using Packinator3D.scenes.puzzle;

namespace Packinator3D.scenes.menus.@select;

public partial class Select : Control
{
	private TabContainer tabContainer;
	private ItemList normalPuzzleList;
	private ItemList customPuzzleList;
	private TasksPanel tasksPanel;
	private FileDialog importFileDialog;

	[ExportGroup("Paths")]
	[Export]
	public NodePath TabContainerPath { get; set; }

	[Export]
	public NodePath NormalPuzzleListPath { get; set; }

	[Export]
	public NodePath CustomPuzzleListPath { get; set; }

	[Export]
	public NodePath TasksPanelPath { get; set; }

	[Export]
	public NodePath ImportFileDialogPath { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		tabContainer = GetNode<TabContainer>(TabContainerPath);
		normalPuzzleList = GetNode<ItemList>(NormalPuzzleListPath);
		customPuzzleList = GetNode<ItemList>(CustomPuzzleListPath);
		tasksPanel = GetNode<TasksPanel>(TasksPanelPath);
		importFileDialog = GetNode<FileDialog>(ImportFileDialogPath);

		importFileDialog.FilesSelected += ImportPuzzle;

		foreach (var puzzle in SaveManager.SaveData.Puzzles) {
			normalPuzzleList.AddItem(puzzle.Name);
		}

		ReloadCustomPuzzles();
	}

	public override void _ExitTree() {
		SaveManager.Save();
	}

	private void ReloadCustomPuzzles() {
		customPuzzleList.Clear();
		foreach (var puzzle in SaveManager.SaveData.CustomPuzzles) {
			customPuzzleList.AddItem(puzzle.Name);
		}
	}

	private void OnNormalPuzzleListItemActivated(int index) {
		LoadPuzzle(SaveManager.SaveData.Puzzles[index]);
	}

	private void OnCustomPuzzleListItemActivated(int index) {
		LoadPuzzle(SaveManager.SaveData.CustomPuzzles[index]);
	}

	private void LoadPuzzle(Puzzle puzzle, int solutionIndex=-1, bool edit=false) {
		if (puzzle == null) return;

		var viewScene = ResourceLoader.Load<PackedScene>("res://scenes/view/view.tscn").Instantiate();
		viewScene.GetNode<ViewScene>(".").IsEdit = edit;
		viewScene.GetNode<PuzzleNode>("PuzzleNode").LoadData(puzzle, solutionIndex, solutionIndex < 0);
		var placementController = viewScene.GetNode<BlockPlacementController>("BlockPlacementController");
		placementController.ViewSolution = solutionIndex;
		placementController.IsSolved = solutionIndex >= 0;

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
		return tabContainer.CurrentTab == 0 ? SaveManager.SaveData.Puzzles[selected[0]] : SaveManager.SaveData.CustomPuzzles[selected[0]];
	}

	private void Play() {
		LoadPuzzle(GetSelectedPuzzle());
	}

	private Puzzle currentSolutionMenuPuzzle;
	private bool currentSolutionMenuEditMode;

	private void View() {
		var puzzle = GetSelectedPuzzle();
		if (puzzle is null) return;

		if (puzzle.Solutions.Count == 1) {
			LoadPuzzle(puzzle, 0);
			return;
		}

		currentSolutionMenuPuzzle = puzzle;
		currentSolutionMenuEditMode = false;
		PopupSolutionMenu(puzzle);
	}

	private void Edit() {
		var puzzle = GetSelectedPuzzle();
		if (puzzle is null) return;

		if (puzzle.Solutions.Count == 1) {
			LoadPuzzle(puzzle, 0, true);
			return;
		}

		currentSolutionMenuPuzzle = puzzle;
		currentSolutionMenuEditMode = true;
		PopupSolutionMenu(puzzle);
	}

	private void PopupSolutionMenu(Puzzle puzzle) {
		var solutionMenu = GetNode<PopupMenu>("SolutionMenu");

		// Add all puzzle solutions to the menu
		solutionMenu.Clear();
		foreach (string solutionName in PuzzleUtils.CreateSolutionNames(puzzle.Solutions)) {
			solutionMenu.AddItem(solutionName);
		}

		if (puzzle.Solutions.Count == 0) {
			solutionMenu.AddItem("No solutions");
			currentSolutionMenuPuzzle = null;
		}

		// Move solution menu to the top of view button
		var viewButton = GetNode<Button>("MarginContainer2/HBoxContainer/ViewButton");
		int yOffset = Mathf.Clamp(puzzle.Solutions.Count * 28, 90, 500);
		var rect = viewButton.GetGlobalRect();
		var irect = new Rect2I(
			Mathf.RoundToInt(rect.Position.X),
			Mathf.RoundToInt(rect.Position.Y - yOffset),
			Mathf.RoundToInt(rect.Size.X),
			Mathf.RoundToInt(rect.Size.Y));

		solutionMenu.PopupOnParent(irect);
	}

	private void OnSolutionMenuIndexPressed(int index) {
		if (currentSolutionMenuPuzzle is null) return;
		LoadPuzzle(currentSolutionMenuPuzzle, index, currentSolutionMenuEditMode);
	}


	private void New() { }

	private void Delete() {
		if (tabContainer.CurrentTab != 1) return;
		var puzzle = GetSelectedPuzzle();
		if (puzzle is null) return;
		SaveManager.RemovePuzzle(puzzle);
		ReloadCustomPuzzles();
	}

	private void Import() {
		importFileDialog.Show();
	}

	private void ImportPuzzle(string[] paths) {
		if (paths.Length == 0) return;
		if (paths.Length == 1 && paths[0].EndsWith(".json"))
			SaveManager.ImportPuzzleJson(paths[0]);
		else if (paths.Length == 1 && paths[0].EndsWith(".txt"))
			SaveManager.ImportPuzzle(PuzzleImporter.FromSolution(paths[0]));
		else if (paths.Length == 2 && paths[0].EndsWith(".txt") && paths[1].EndsWith(".txt"))
			SaveManager.ImportPuzzle(PuzzleImporter.FromPiecesAndGoal(paths[0], paths[1]));
		else {
			GD.PrintErr("Invalid import files");
			return;
		}

		ReloadCustomPuzzles();
		tabContainer.CurrentTab = 1;
	}

	private void Export() {
		var puzzle = GetSelectedPuzzle();
		if (puzzle is null) return;
		string path = SaveManager.ExportPuzzle(GetSelectedPuzzle());
		OS.ShellShowInFileManager(ProjectSettings.GlobalizePath(path));
	}

	private void Solve() {
		var puzzle = GetSelectedPuzzle();
		if (puzzle is null) return;

		TaskManager.Solve(puzzle);
		tasksPanel.Expand();
	}
}
