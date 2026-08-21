#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

sampler2D SceneSampler : register(s0);
sampler2D GlowSampler : register(s1);

float GlowIntensity = 0.9;
float VignetteStrength = 0.55;
float ScanlineIntensity = 0.08;
float ScanlineDensity = 400.0;

float4 MainPS(float2 texCoord : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    float4 sceneColor = tex2D(SceneSampler, texCoord);
    float4 glowColor = tex2D(GlowSampler, texCoord);

    float3 combined = sceneColor.rgb + glowColor.rgb * GlowIntensity;

    float2 centered = texCoord - 0.5;
    float vignette = 1.0 - dot(centered, centered) * VignetteStrength * 2.0;
    combined *= saturate(vignette);

    float scanline = sin(texCoord.y * ScanlineDensity * 3.14159) * 0.5 + 0.5;
    combined *= 1.0 - (scanline * ScanlineIntensity);

    return float4(combined, sceneColor.a) * color;
}
technique FinalComposite
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}