using Godot;

namespace Packinator3D.scenes.menus.options;

public partial class VolumeSlider : HSlider
{
	[Export]
	public string AudioBusName { get; set; }

	private int busIndex;

	public override void _Ready() {
		busIndex = AudioServer.GetBusIndex(AudioBusName);
		Value = Db2Linear(AudioServer.GetBusVolumeDb(busIndex));
		ValueChanged += OnValueChanged;
	}

	private void OnValueChanged(double value) {
		AudioServer.SetBusVolumeDb(busIndex, Linear2Db((float)value));
	}

	private static float Db2Linear(float db) {
		return Mathf.Pow(2, db / 10);
	}

	private static float Linear2Db(float linear) {
		if (linear <= 0) return -80;
		return 10 * Mathf.Log(linear) / Mathf.Log(2);
	}
}