#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

sampler2D TextureSampler : register(s0);

float Desaturation = 0.55;   // 0 = full color, 1 = full grayscale
float Darken = 0.35;         // 0 = no darkening, 1 = black

float4 MainPS(float2 texCoord : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    float4 texColor = tex2D(TextureSampler, texCoord);
    float gray = dot(texColor.rgb, float3(0.299, 0.587, 0.114));
    float3 desaturated = lerp(texColor.rgb, float3(gray, gray, gray), Desaturation);
    float3 darkened = desaturated * (1.0 - Darken);
    return float4(darkened, texColor.a) * color;
}

technique MapStylize
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}