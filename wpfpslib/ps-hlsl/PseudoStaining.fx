sampler2D input : register(s0);
float HematoxylinIndex : register(C0);
float EosinIndex : register(C1);
float DabIndex : register(C2);
float HematoxylinIntensity : register(C3);
float EosinIntensity : register(C4);
float DabIntensity : register(C5);
float4 HematoxylinColor : register(C6);
float4 EosinColor : register(C7);
float4 DabColor : register(C8);

float4 main(float2 uv : TEXCOORD0) : COLOR
{
    
    float4 clr = tex2D(input, uv);
	
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

	if (DabIndex == 0) {
		CH3 = clr.r;
	}
	else if (DabIndex == 1) {
		CH3 = clr.g;
	}
	else if (DabIndex == 2) {
		CH3 = clr.b;
	}
	CH1 = clamp(CH1* HematoxylinIntensity, 0, 1);
	CH2 = clamp(CH2* EosinIntensity, 0, 1);
	CH3 = clamp(CH3* DabIntensity, 0, 1);


	
	float max_r = clamp(1 - CH1 * (1-HematoxylinColor.r) - CH2 * (1-EosinColor.r) - CH3 * (1-DabColor.r), 0, 1);
	float max_g = clamp(1 - CH1 * (1-HematoxylinColor.g) - CH2 * (1-EosinColor.g) - CH3 * (1-DabColor.g), 0, 1);
	float max_b = clamp(1 - CH1 * (1-HematoxylinColor.b) - CH2 * (1-EosinColor.b) - CH3 * (1-DabColor.b), 0, 1);
	
	float H = max_r;// clamp(min(min((1 - CH1 + CH1 * HematoxylinColor.r), (1 - CH2 + CH2 * EosinColor.r)), (1 - CH3 + CH3 * DabColor.r)), 0, 1);
	float E = max_g;//clamp(min(min((1 - CH1 + CH1 * HematoxylinColor.g), (1 - CH2 + CH2 * EosinColor.g)), (1 - CH3 + CH3 * DabColor.g)), 0, 1);
	float B = max_b;//clamp(min(min((1 - CH1 + CH1 * HematoxylinColor.b), (1 - CH2 + CH2 * EosinColor.b)), (1 - CH3 + CH3 * DabColor.b)), 0, 1);

    return float4(H, E, B, 1);
}
