using BlockPuzzleViewerSolverEditor.datastructure;
using Godot;

namespace BlockPuzzleViewerSolverEditor.scenes;

public partial class PauseMenu : ColorRect
{
	private bool pauseReleased;

	private puzzle.PuzzleNode puzzleNode;
	private ClipPlane xClipPlane;
	private ClipPlane yClipPlane;
	private ClipPlane zClipPlane;
	private Node3D xClip;
	private Node3D yClip;
	private Node3D zClip;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		puzzleNode = GetNode<puzzle.PuzzleNode>("../PuzzleNode");
		xClip = GetNode<Node3D>("../XClip");
		xClipPlane = new ClipPlane(vec => vec.X);
		UpdateXClip(GetNode<HSlider>("ClipLabel/X Clip").Value);
		yClip = GetNode<Node3D>("../YClip");
		yClipPlane = new ClipPlane(vec => vec.Y);
		UpdateYClip(GetNode<HSlider>("ClipLabel/Y Clip").Value);
		zClip = GetNode<Node3D>("../ZClip");
		zClipPlane = new ClipPlane(vec => vec.Z);
		UpdateZClip(GetNode<HSlider>("ClipLabel/Z Clip").Value);
	}

	public void ShowPauseMenu() {
		if (!GetTree().Paused) {
			GetTree().Paused = true;
			pauseReleased = false;
			Show();
		}
		
		xClip.Show();
		yClip.Show();
		zClip.Show();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Input.MouseMode = Input.MouseModeEnum.Visible;

		if (!pauseReleased && !Input.IsActionJustPressed("pause")) {
			pauseReleased = true;
		}

		if (GetTree().Paused && Input.IsActionJustPressed("pause") && pauseReleased) {
			OnCloseButtonPressed();
		}
	}

	private void OnCloseButtonPressed()
	{
		Hide();
		xClip.Hide();
		yClip.Hide();
		zClip.Hide();
		Input.MouseMode = Input.MouseModeEnum.Captured;
		GetTree().Paused = false;
	}
		
	private void HideClippedPieces() {
		foreach(var node in puzzleNode.PuzzlePieceNodes) {
			node.Show();
			Transform3D transform = node.Transform;
			foreach(Vector3 voxel in node.PieceData.Shape) {
				Vector3 position = (transform * voxel);
				if (xClipPlane.ClipTest(position) ||
					yClipPlane.ClipTest(position) ||
					zClipPlane.ClipTest(position)
					) {
					node.Hide();
				}
			}
		}
	}

	private void _on_close_button_pressed()
	{
		OnCloseButtonPressed();
	}

	private void _on_piece_width_value_changed(double value)
	{
		puzzleNode.SetWidth((float) value);
	}
	private void _on_x_clip_value_changed(double value)
	{
		UpdateXClip(value);
		HideClippedPieces();
	}

	private void UpdateXClip(double value) {
		xClip.Position = new Vector3((float) value, 0.0f, 0.0f);
		xClipPlane.AxisOffset = (float) value;
	}

	private void _on_invert_x_toggled(bool toggledOn)
	{
		xClipPlane.Inverted = toggledOn;
		HideClippedPieces();
	}
	
	//private void _on_x_clip_drag_started()
	//{
		//xClip.Show();
	//}
//
	//private void _on_x_clip_drag_ended(bool value_changed)
	//{
		//xClip.Hide();
	//}
	
	private void _on_y_clip_value_changed(double value)
	{
		if (value == 0.0) {
			yClip.Hide();
		}
		else {
			yClip.Show();
		}
		
		UpdateYClip(value);
		HideClippedPieces();
	}

	private void UpdateYClip(double value) {
		yClip.Position = new Vector3(0.0f, (float) value, 0.0f);
		yClipPlane.AxisOffset = (float) value;
	}
	
	//private void _on_y_clip_drag_started()
	//{
		//yClip.Show();
	//}
//
	//private void _on_y_clip_drag_ended(bool value_changed)
	//{
		//yClip.Hide();
	//}
	
	
	private void _on_invert_y_toggled(bool toggledOn)
	{
		yClipPlane.Inverted = toggledOn;
		HideClippedPieces();
	}
	
	private void _on_z_clip_value_changed(double value)
	{
		UpdateZClip(value);
		HideClippedPieces();
	}

	private void UpdateZClip(double value) {
		zClip.Position = new Vector3(0.0f, 0.0f, (float) value);
		zClipPlane.AxisOffset = (float) value;
	}

	private void _on_invert_z_toggled(bool toggledOn)
	{
		zClipPlane.Inverted = toggledOn;
		HideClippedPieces();
	}
	
	//private void _on_z_clip_drag_started()
	//{
		//zClip.Show();
	//}
//
	//private void _on_z_clip_drag_ended(bool value_changed)
	//{
		//zClip.Hide();
	//}
	private void _on_check_box_toggled(bool toggledOn)
	{
		puzzleNode.SetTargetShapeVisible(toggledOn);
	}
}
