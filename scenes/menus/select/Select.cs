using BlockPuzzleViewerSolverEditor.scenes.puzzle;
using Godot;

namespace BlockPuzzleViewerSolverEditor.scenes.menus.@select;

public partial class Select : Control
{
	private ItemList puzzleList;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		puzzleList = GetNode<ItemList>("MarginContainer/VBoxContainer/ItemList");

		// Load all puzzles from the puzzles directory
		var puzzleDirectory = DirAccess.Open("res://puzzles");
		string[] puzzles = puzzleDirectory.GetFiles();
		foreach (string puzzlePath in puzzles) {
			puzzleList.AddItem(puzzlePath);
		}
	}

	private void _on_item_list_item_activated(int index) {
		string puzzlePath = puzzleList.GetItemText(index);
		var viewScene = ResourceLoader.Load<PackedScene>("res://scenes/view/view.tscn").Instantiate();
		viewScene.GetNode<PuzzleNode>("PuzzleNode").PuzzlePath = "res://puzzles/" + puzzlePath;
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
			GetTree().ChangeSceneToFile("res://scenes/menus/main/main_menu.tscn");
		}
	}
}