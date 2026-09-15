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

float2 Resolution;
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
    float3 color = IsHorizontal ? lerp(ColourOne.rgb, ColourTwo.rgb, uv.x) : lerp(ColourOne.rgb, ColourTwo.rgb, uv.y);
    return float4(color.rgb, ColourTwo.a);
}

// This is just a special variant of gradient bar for hue specifically since that needs to lerp through, y'know, EVERY colour and not just two.
float4 HueBar(VertexShaderOutput input) : COLOR
{
    float2 uv = input.TextureCoordinates;
    float hue = IsHorizontal ? uv.x : uv.y;
    float3 rgb = HUEtoRGB(hue);
    return float4(rgb, 1);
}

float4 CheckerboardColourAtPoint(float2 coordinates)
{
    float2 checkerboard = floor(coordinates * Resolution / 8);
    float checkerboardValue = fmod(checkerboard.x + checkerboard.y, 2);
    float lightGrey = 0.7;
    float darkGrey = 0.3;
    return checkerboardValue < 1 ? float4(lightGrey, lightGrey, lightGrey, 1) : float4(darkGrey, darkGrey, darkGrey, 1);
}

float4 AlphaBar(VertexShaderOutput input) : COLOR
{
    float2 uv = input.TextureCoordinates;
    float alpha = IsHorizontal ? uv.x : uv.y;
    alpha *= ColourTwo.a;
    float4 checkerboard = CheckerboardColourAtPoint(uv);
    float4 color = lerp(checkerboard, ColourTwo, alpha);
    return float4(color);
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

technique AlphaBar
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL AlphaBar();
    }
};