using Godot;
using Packinator3D.datastructure;
using Packinator3D.scenes.puzzle;

namespace Packinator3D.scenes;

public partial class PauseMenu : Control
{
	private bool pauseReleased;

	private PuzzleNode puzzleNode;
	private ClipPlane xClipPlane;
	private ClipPlane yClipPlane;
	private ClipPlane zClipPlane;
	private Node3D xClip;
	private Node3D yClip;
	private Node3D zClip;

	[ExportGroup("Sounds")]
	[Export]
	public AudioStream ShowSound { get; set; }

	[Export]
	public AudioStream HideSound { get; set; }

	private AudioStreamPlayer showSoundPlayer;
	private AudioStreamPlayer hideSoundPlayer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		puzzleNode = GetNode<PuzzleNode>("../PuzzleNode");
		xClip = GetNode<Node3D>("../XClip");
		xClipPlane = new ClipPlane(vec => vec.X);
		UpdateXClip(GetNode<HSlider>("ClipLabel/X Clip").Value);
		yClip = GetNode<Node3D>("../YClip");
		yClipPlane = new ClipPlane(vec => vec.Y);
		UpdateYClip(GetNode<HSlider>("ClipLabel/Y Clip").Value);
		zClip = GetNode<Node3D>("../ZClip");
		zClipPlane = new ClipPlane(vec => vec.Z);
		UpdateZClip(GetNode<HSlider>("ClipLabel/Z Clip").Value);
		var targetShapeToggle = GetNode<CheckButton>("CheckBox");
		targetShapeToggle.ButtonPressed = puzzleNode.TargetShapeVisible;
		var nameEdit = GetNode<LineEdit>("NameEdit");
		nameEdit.Text = puzzleNode.PuzzleData.Name;

		showSoundPlayer = CreateSoundPlayer(ShowSound);
		hideSoundPlayer = CreateSoundPlayer(HideSound);
	}

	private AudioStreamPlayer CreateSoundPlayer(AudioStream stream) {
		var soundPlayer = new AudioStreamPlayer {
			Bus = "Effects",
			Stream = stream,
		};
		AddChild(soundPlayer);
		return soundPlayer;
	}

	public void ShowPauseMenu() {
		if (Visible)
			return;

		SetProcess(true);
		Input.MouseMode = Input.MouseModeEnum.Visible;
		pauseReleased = false;
		xClip.Show();
		yClip.Show();
		zClip.Show();
		Show();
		showSoundPlayer.Play();
	}

	public void HidePauseMenu() {
		if (!Visible)
			return;

		Hide();
		xClip.Hide();
		yClip.Hide();
		zClip.Hide();
		Input.MouseMode = Input.MouseModeEnum.Captured;
		SetProcess(false);
		hideSoundPlayer.Play();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		if (!pauseReleased && !Input.IsActionJustPressed("pause")) {
			pauseReleased = true;
		}

		if (Visible && Input.IsActionJustPressed("pause") && pauseReleased) {
			HidePauseMenu();
		}
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
	
	private void HintMovePiece() {

		if (puzzleNode.PuzzleData.Solutions.Count == 0) {
			return;
		}
		
		for(int i = 0; i < puzzleNode.PuzzlePieceNodes.Count; i++) {
			var node = puzzleNode.PuzzlePieceNodes[i];
			var solution = puzzleNode.PuzzleData.Solutions[0];
			if (node.Transform != solution.States[i]) {
				bool found = false;
				
				for(int j = 0; j < puzzleNode.PuzzlePieceNodes.Count; j++) {
					if (i == j) continue;
					var piece_data = puzzleNode.PuzzlePieceNodes[j].PieceData;
					foreach(var pos in piece_data.Shape) {
						var world_pos = puzzleNode.PuzzlePieceNodes[j].Transform * pos;
						foreach(var pos2 in node.PieceData.Shape) {
							var world_pos2 = solution.States[i] * pos2;
							if (world_pos.Round() == world_pos2.Round()) {
								found = true;
								break;
							}
						}
						if (found) break;
					}
					if (found) break;
				}
				if (found) continue;
				
				node.Transform = solution.States[i];
				break;
			}
		}
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
	
	// Hint button
	private void _on_button_pressed()
	{
		HintMovePiece();
	}
	
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

	private void _on_name_edit_text_changed(string newText)
	{
		puzzleNode.PuzzleData.Name = newText;
	}
}



