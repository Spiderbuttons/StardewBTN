#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

static const float PI = 3.14159265359793;

float2 Resolution;
float Smoothing;
float Value;

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

bool isTransparentAtPoint(float2 coordinates)
{
    return length((coordinates - 0.5) * 2) > 0.99; // Setting it to exactly 1 makes the bottom strangely flat.
}

float4 PointToColour(float2 coordinates)
{
    // https://www.shadertoy.com/view/3fcfWr
    
    coordinates -= 0.5f;
    
    float hue = atan2(coordinates.y, coordinates.x) * 3.0 / PI;
    float3 hueRGB = float3(hue, hue - 2.0, hue + 2.0);
    hueRGB = clamp(abs(3.0 - abs(hueRGB)) - 1.0, 0.0, 1.0);
    
    float3 smoothHueRGB = hueRGB * hueRGB * (3.0 - hueRGB * 2.0);
    
    float distFromCenter = length(coordinates * 2.0f);
    float saturation = clamp(distFromCenter / 1, 0.0, 1.0);
    
    float3 rgb = lerp(hueRGB.rgb, smoothHueRGB.rgb, Smoothing);
    rgb = lerp(float3(1, 1, 1), rgb, saturation);
    
    return float4(rgb * Value, 1);
}

float4 ColourWheel(VertexShaderOutput input) : COLOR
{
    float2 uv = input.TextureCoordinates;
    float alpha = 0.0;
    for (int x = -3; x <= 3; x++)
    {
        for (int y = -3; y <= 3; y++)
        {
            float2 neighbourUV = uv + float2(x, y) / Resolution;
            alpha += isTransparentAtPoint(neighbourUV) ? 0.0 : 1.0;
        }
    }
    alpha /= 45.0; // It's 49 cuz going from -3 to 3 maks a 7x7 grid. Convolution? Idk.
    alpha = clamp(alpha, 0.0, 1.0);
    return PointToColour(uv) * alpha * input.Color;
}

technique ColourWheel
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL ColourWheel();
    }
};