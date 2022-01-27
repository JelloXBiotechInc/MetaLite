sampler2D input : register(s0);
float HematoxylinIndex : register(C0);
float EosinIndex : register(C1);
float BackgroundIndex : register(C2);

float4 main(float2 uv : TEXCOORD0) : COLOR
{

	float4 clr = tex2D(input, uv);
	clr.r = clr.r + clr.r;
	clr.g = clr.g + clr.g;
	clr.b = clr.b + clr.b;
	float CH1 = clr.r;
	float CH2 = clr.g;
	float CH3 = clr.b;
	if (HematoxylinIndex == 0) {
		CH1 = clr.r;
	}
	else if (HematoxylinIndex == 1) {
		CH1 = clr.g;
	}
	else if (HematoxylinIndex == 2) {
		CH1 = clr.b;
	}

	if (EosinIndex == 0) {
		CH2 = clr.r;
	}
	else if (EosinIndex == 1) {
		CH2 = clr.g;
	}
	else if (EosinIndex == 2) {
		CH2 = clr.b;
	}

	if (BackgroundIndex == 0) {
		CH3 = clr.r;
	}
	else if (BackgroundIndex == 1) {
		CH3 = clr.g;
	}
	else if (BackgroundIndex == 2) {
		CH3 = clr.b;
	}
	CH1 = clamp(CH1, 0, 1);
	CH2 = clamp(CH2, 0, 1);
	CH3 = clamp(CH3, 0, 1);
	float ratio_r = CH1 / (CH1 + CH2);
	if (CH1 > 0.2862) {
		ratio_r = 0.9;
	}
	float ratio_g = 1 - ratio_r;
	float H = clamp(((1 - CH1 + CH1 * 85 / 255)* ratio_r + (1 - CH2 + CH2 * 255 / 255) * ratio_g), 0, 1);
	float E = clamp(((1 - CH1 + CH1 * 0 / 255) * ratio_r + (1 - CH2 + CH2 * 136 / 255) * ratio_g), 0, 1);
	float B = clamp(((1 - CH1 + CH1 * 136 / 255) * ratio_r + (1 - CH2 + CH2 * 196 / 255) * ratio_g), 0, 1);


	return float4(H, E, B, 1);
}
