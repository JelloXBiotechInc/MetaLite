sampler2D input : register(s0);
float R : register(C0);
float G : register(C1);
float B : register(C2);
float RIndex : register(C3);
float GIndex : register(C4);
float BIndex : register(C5);



float4 main(float2 uv : TEXCOORD0) : COLOR
{

    float4 clr = tex2D(input, uv);
    float Rratio = 1;
    float Gratio = 1;
    float Bratio = 1;
    float CH1 = clr.r;
    float CH2 = clr.g;
    float CH3 = clr.b;
    float CH4 = clr.a;

    if (RIndex == 0) {
        CH1 = clr.r * R;
    }
    else if (RIndex == 1) {
        CH1 = clr.g * R;
    }
    else if (RIndex == 2) {
        CH1 = clr.b * R;
    }

    if (GIndex == 0) {
        CH2 = clr.r * G;
    }
    else if (GIndex == 1) {
        CH2 = clr.g * G;
    }
    else if (GIndex == 2) {
        CH2 = clr.b * G;
    }

    if (BIndex == 0) {
        CH3 = clr.r * B;
    }
    else if (BIndex == 1) {
        CH3 = clr.g * B;
    }
    else if (BIndex == 2) {
        CH3 = clr.b * B;
    }



    return float4(CH1, CH2, CH3, CH4);
}
