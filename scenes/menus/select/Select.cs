using System.Globalization;
using Godot;
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

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		tabContainer = GetNode<TabContainer>("MarginContainer/TabContainer");
		normalPuzzleList = GetNode<ItemList>("MarginContainer/TabContainer/Normal Levels");
		customPuzzleList = GetNode<ItemList>("MarginContainer/TabContainer/Custom Levels");
		tasksPanel = GetNode<TasksPanel>("TasksPanel");

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

	private void LoadPuzzle(Puzzle puzzle, int solutionIndex=-1) {
		if (puzzle == null) return;
		var viewScene = ResourceLoader.Load<PackedScene>("res://scenes/view/view.tscn").Instantiate();
		viewScene.GetNode<PuzzleNode>("PuzzleNode").LoadData(puzzle, solutionIndex);
		viewScene.GetNode<BlockPlacementController>("BlockPlacementController").ViewSolution = solutionIndex;
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
		var puzzle = GetSelectedPuzzle();
		if (puzzle is null) return;

		if (puzzle.Solutions.Count == 1) {
			LoadPuzzle(currentSolutionMenuPuzzle, 0);
		}

		var solutionMenu = GetNode<PopupMenu>("SolutionMenu");

		// Add all puzzle solutions to the menu
		solutionMenu.Clear();
		foreach (var solution in puzzle.Solutions) {
			solutionMenu.AddItem(solution.Time.ToString(CultureInfo.CurrentCulture));
		}

		// Move solution menu to the top of view button
		var viewButton = GetNode<Button>("MarginContainer2/HBoxContainer/ViewButton");
		int yOffset = Mathf.Max(90, puzzle.Solutions.Count * 24);
		var rect = viewButton.GetGlobalRect();
		var irect = new Rect2I(
			Mathf.RoundToInt(rect.Position.X),
			Mathf.RoundToInt(rect.Position.Y - yOffset),
			Mathf.RoundToInt(rect.Size.X),
			Mathf.RoundToInt(rect.Size.Y));

		currentSolutionMenuPuzzle = puzzle;
		solutionMenu.PopupOnParent(irect);
	}

	private Puzzle currentSolutionMenuPuzzle;

	private void OnSolutionMenuIndexPressed(int index) {
		if (currentSolutionMenuPuzzle is null) return;
		LoadPuzzle(currentSolutionMenuPuzzle, index);
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

	private void Solve() {
		var puzzle = GetSelectedPuzzle();
		if (puzzle is null) return;

		TaskManager.Solve(puzzle);
		tasksPanel.Expand();
	}
}