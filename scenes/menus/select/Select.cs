using Godot;
using Packinator3D.datastructure;
using Packinator3D.scenes.puzzle;

namespace Packinator3D.scenes.menus.@select;

public partial class Select : Control
{
	private ItemList puzzleList;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		puzzleList = GetNode<ItemList>("MarginContainer/VBoxContainer/ItemList");

		foreach (var puzzle in SaveManager.Puzzles) {
			puzzleList.AddItem(puzzle.Name);
		}
	}

	private void _on_item_list_item_activated(int index) {
		var viewScene = ResourceLoader.Load<PackedScene>("res://scenes/view/view.tscn").Instantiate();
		viewScene.GetNode<PuzzleNode>("PuzzleNode").LoadData(SaveManager.Puzzles[index]);
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

	public override void _ExitTree() {
		SaveManager.Save();
	}
}