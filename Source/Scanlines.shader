shader_type canvas_item;

uniform float PosScale = 200f;
uniform float TimeScale = 10f;
uniform float StrengthMultiplier = 0.1f;

void fragment()
{
	vec4 color = texture(TEXTURE, UV);
	float offset = sin(FRAGCOORD.y * PosScale + -TIME * TimeScale) * StrengthMultiplier;
	COLOR = color + vec4(offset, offset, offset, 0);
}
