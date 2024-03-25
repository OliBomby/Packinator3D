using System;
using System.Collections.Generic;
using Godot;
using Packinator3D.datastructure;

namespace Packinator3D.scenes.menus.@select.tasks;

public partial class TasksDisplay : VBoxContainer
{
	private readonly Dictionary<Guid, TaskDisplay> taskDisplays = new();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		foreach (var task in TaskManager.Tasks) {
			AddTask(task);
		}
		TaskManager.TaskAdded += AddTask;
		TaskManager.TaskRemoved += RemoveTask;
	}

	private void AddTask(SolveTask task)
	{
		var taskDisplay = ResourceLoader.Load<PackedScene>("res://scenes/menus/select/tasks/task_display.tscn").Instantiate<TaskDisplay>();
		taskDisplay.Task = task;
		AddChild(taskDisplay);
		taskDisplays.Add(task.Id, taskDisplay);
	}

	private void RemoveTask(SolveTask task) {
		if (!taskDisplays.Remove(task.Id, out var taskDisplay)) return;
		RemoveChild(taskDisplay);
		taskDisplay.QueueFree();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}