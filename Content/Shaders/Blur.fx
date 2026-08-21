#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

sampler2D TextureSampler : register(s0);

float2 Direction;      // (1/width, 0) or (0, 1/height) - set by C# side
float BlurAmount = 3.0;

float4 MainPS(float2 texCoord : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    float4 sum = float4(0, 0, 0, 0);
    float weights[5] = { 0.227027, 0.1945946, 0.1216216, 0.054054, 0.016216 };

    sum += tex2D(TextureSampler, texCoord) * weights[0];
    for (int i = 1; i < 5; i++)
    {
        float2 offset = Direction * BlurAmount * i;
        sum += tex2D(TextureSampler, texCoord + offset) * weights[i];
        sum += tex2D(TextureSampler, texCoord - offset) * weights[i];
    }

    return sum * color;
}

technique Blur
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}