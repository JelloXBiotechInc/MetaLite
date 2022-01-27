sampler2D input : register(s0);
float HematoxylinIndex : register(C0);
float DabIndex : register(C1);
float BackgroundIndex : register(C2);

float4 main(float2 uv : TEXCOORD0) : COLOR
{
	float4 clr = tex2D(input, uv);
	clr.r = clr.r * 1.1;
	clr.g = clr.g * 1.1;
	clr.b = clr.b * 1.1;
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

	if (DabIndex == 0) {
		CH2 = clr.r;
	}
	else if (DabIndex == 1) {
		CH2 = clr.g;
	}
	else if (DabIndex == 2) {
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
	CH1 = clamp(CH1, 0.00003921, 1);
	CH2 = clamp(CH2, 0.00003921, 1);
	CH3 = clamp(CH3, 0.00003921, 1);
	float ratio_g = CH2 / (CH1 + CH2);
	if (CH2 > 0.17254) {
		ratio_g = 0.7;
	}
	float ratio_r = 1 - ratio_g;
	
	float H = clamp((1 - CH1 + CH1 * 0 / 255) * ratio_r + (1 - CH2 + CH2 * 98 / 255) * ratio_g, 0, 1);
	float D = clamp((1 - CH1 + CH1 * 102 / 255) * ratio_r + (1 - CH2 + CH2 * 40 / 255) * ratio_g, 0, 1);
	float B = clamp((1 - CH1 + CH1 * 255 / 255) * ratio_r + (1 - CH2 + CH2 * 30 / 255) * ratio_g, 0, 1);


	return float4(H, D, B, 1);
}
