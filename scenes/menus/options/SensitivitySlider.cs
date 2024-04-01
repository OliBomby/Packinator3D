using Godot;
using Packinator3D.datastructure;

namespace Packinator3D.scenes.menus.options;

public partial class SensitivitySlider : HSlider
{
	public override void _Ready() {
		Value = SaveManager.SaveData.Sensitivity;
		ValueChanged += OnValueChanged;
	}

	private void OnValueChanged(double value) {
		SaveManager.SaveData.Sensitivity = (float)value;
	}
}