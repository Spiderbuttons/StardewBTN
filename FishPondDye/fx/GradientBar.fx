#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

float4 ColourOne;
float4 ColourTwo;
bool IsHorizontal;

float3 HUEtoRGB(in float hue)
{
    float r = abs(hue * 6 - 3) - 1;
    float g = 2 - abs(hue * 6 - 2);
    float b = 2 - abs(hue * 6 - 4);
    return saturate(float3(r,g,b));
}

float4 GradientBar(VertexShaderOutput input) : COLOR
{
    float2 uv = input.TextureCoordinates;
    float4 color = IsHorizontal ? lerp(ColourOne, ColourTwo, uv.x) : lerp(ColourOne, ColourTwo, uv.y);
    return float4(color.rgb, 1.0);
}

// This is just a special variant of gradient bar for hue specifically since that needs to lerp through, y'know, EVERY colour and not just two.
float4 HueBar(VertexShaderOutput input) : COLOR
{
    float2 uv = input.TextureCoordinates;
    float hue = IsHorizontal ? uv.x : uv.y;
    float3 rgb = HUEtoRGB(hue);
    return float4(rgb, 1);
}

technique GradientBar
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL GradientBar();
    }
};

technique HueBar
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL HueBar();
    }
};