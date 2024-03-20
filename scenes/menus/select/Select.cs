using Godot;
using Packinator3D.datastructure;
using Packinator3D.scenes.puzzle;

namespace Packinator3D.scenes.menus.@select;

public partial class Select : Control
{
	private ItemList normalPuzzleList;
	private ItemList customPuzzleList;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		normalPuzzleList = GetNode<ItemList>("MarginContainer/TabContainer/Normal Levels");
		customPuzzleList = GetNode<ItemList>("MarginContainer/TabContainer/Custom Levels");

		foreach (var puzzle in SaveManager.Puzzles) {
			normalPuzzleList.AddItem(puzzle.Name);
		}

		foreach (var puzzle in SaveManager.SaveData.CustomPuzzles) {
			customPuzzleList.AddItem(puzzle.Name);
		}
	}

	private void OnNormalPuzzleListItemActivated(int index) {
		LoadPuzzle(SaveManager.Puzzles[index]);
	}

	private void OnCustomPuzzleListItemActivated(int index) {
		LoadPuzzle(SaveManager.SaveData.CustomPuzzles[index]);
	}

	private void LoadPuzzle(Puzzle puzzle) {
		var viewScene = ResourceLoader.Load<PackedScene>("res://scenes/view/view.tscn").Instantiate();
		viewScene.GetNode<PuzzleNode>("PuzzleNode").LoadData(puzzle);
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

	public override void _ExitTree() {
		SaveManager.Save();
	}
}