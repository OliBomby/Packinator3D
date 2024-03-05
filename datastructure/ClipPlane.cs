using Godot;
using System;

class ClipPlane {
	public bool Inverted = false;
	public float AxisOffset = 0.0f;

	private Func<Vector3, float> getAxis;

	public ClipPlane(Func<Vector3, float> getAxis) {
		this.getAxis = getAxis;		
	}

	public bool ClipTest(Vector3 point) {
		if (Inverted)
		{ 
			bool below = getAxis(point) + 1.0f < AxisOffset;
			return !below;
		}
		else {
			bool below = getAxis(point) < AxisOffset;
			return below;
		}
	}
}
