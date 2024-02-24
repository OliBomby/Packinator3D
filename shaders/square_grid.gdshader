shader_type spatial;

uniform int n_rows = 10;
uniform int n_columns = 10;
uniform float thickness = 0.1;

void fragment() {
	float num_x = float(n_rows);
	float num_y = float(n_columns);
	
	float margin_x = 0.5 / num_x * thickness;
	float margin_y = 0.5 / num_y * thickness;
	bool column = mod((UV.x + margin_x) * num_x, 1) < thickness;
	bool row = mod((UV.y + margin_y) * num_y, 1) < thickness;
	bool inner_x = abs(UV.x - 0.5) < 0.5 - 1. / num_x + margin_x;
	bool inner_y = abs(UV.y - 0.5) < 0.5 - 1. / num_y + margin_y;
	ALPHA = float((column || row) && inner_x && inner_y);
	ALPHA_SCISSOR_THRESHOLD = 0.5;
}
