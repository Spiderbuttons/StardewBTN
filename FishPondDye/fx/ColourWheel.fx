#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

static const float PI = 3.14159265359;

Texture2D ColourWheelTex;

float2 Resolution;
float BlurMultiplier = 1.0f;

float Smoothing = 1.0f;
float Value = 1.0f;

sampler2D Sampler = sampler_state
{
    Texture = <ColourWheelTex>;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

float4 ColourWheel(VertexShaderOutput input) : COLOR
{
    float2 coords = input.TextureCoordinates;
    // return float4(coords.x, coords.y, 0, 1);
    
    coords -= 0.5f;
    
    float distFromCenter = length(coords * 2.0f);
    if (distFromCenter > 1)
    {
        return float4(255, 255, 255, 0);
    }
    
    float hue = atan2(coords.y, coords.x) * 3.0 / PI;
    
    float4 hueRGB = float4(hue, hue - 2.0, hue + 2.0, 0);
    hueRGB = clamp(abs(3.0 - abs(hueRGB)) - 1.0, 0.0, 1.0);
    
    float4 smoothHueRGB = hueRGB * hueRGB * (3.0 - hueRGB * 2.0);
    
    float saturation = clamp(distFromCenter / 1, 0.0, 1.0);
    
    float3 rgb = lerp(hueRGB.rgb, smoothHueRGB.rgb, Smoothing);
    rgb = lerp(float3(1, 1, 1), rgb, saturation);
    
    return float4 (rgb * Value, 1.0);
}

float4 BlurHoriz(VertexShaderOutput input) : COLOR
{
	float4 colour = float4(0, 0, 0, 0);
    colour += tex2D(Sampler, input.TextureCoordinates + float2(-4.0f * BlurMultiplier / Resolution.x, 0)) * 0.05f;
    colour += tex2D(Sampler, input.TextureCoordinates + float2(-3.0f * BlurMultiplier / Resolution.x, 0)) * 0.09f;
    colour += tex2D(Sampler, input.TextureCoordinates + float2(-2.0f * BlurMultiplier / Resolution.x, 0)) * 0.12f;
    colour += tex2D(Sampler, input.TextureCoordinates + float2(-1.0f * BlurMultiplier / Resolution.x, 0)) * 0.15f;
    colour += tex2D(Sampler, input.TextureCoordinates) * 0.16f;
    colour += tex2D(Sampler, input.TextureCoordinates + float2(1.0f * BlurMultiplier / Resolution.x, 0)) * 0.15f;
    colour += tex2D(Sampler, input.TextureCoordinates + float2(2.0f * BlurMultiplier / Resolution.x, 0)) * 0.12f;
    colour += tex2D(Sampler, input.TextureCoordinates + float2(3.0f * BlurMultiplier / Resolution.x, 0)) * 0.09f;
    colour += tex2D(Sampler, input.TextureCoordinates + float2(4.0f * BlurMultiplier / Resolution.x, 0)) * 0.05f;
    
    return colour;
}

float4 BlurVert(VertexShaderOutput input) : COLOR
{
    float4 colour = float4(0, 0, 0, 0);
    colour += tex2D(Sampler, input.TextureCoordinates + float2(0, -4.0f * BlurMultiplier / Resolution.y)) * 0.05f;
    colour += tex2D(Sampler, input.TextureCoordinates + float2(0, -3.0f * BlurMultiplier / Resolution.y)) * 0.09f;
    colour += tex2D(Sampler, input.TextureCoordinates + float2(0, -2.0f * BlurMultiplier / Resolution.y)) * 0.12f;
    colour += tex2D(Sampler, input.TextureCoordinates + float2(0, -1.0f * BlurMultiplier / Resolution.y)) * 0.15f;
    colour += tex2D(Sampler, input.TextureCoordinates) * 0.16f;
    colour += tex2D(Sampler, input.TextureCoordinates + float2(0, 1.0f * BlurMultiplier / Resolution.y)) * 0.15f;
    colour += tex2D(Sampler, input.TextureCoordinates + float2(0, 2.0f * BlurMultiplier / Resolution.y)) * 0.12f;
    colour += tex2D(Sampler, input.TextureCoordinates + float2(0, 3.0f * BlurMultiplier / Resolution.y)) * 0.09f;
    colour += tex2D(Sampler, input.TextureCoordinates + float2(0, 4.0f * BlurMultiplier / Resolution.y)) * 0.05f;
    
    return colour;
}

technique ColourWheel
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL ColourWheel();
    }
 //    pass P1
 //    {
 //        PixelShader = compile PS_SHADERMODEL BlurHoriz();
 //    }
	// pass P2
 //    {
 //        PixelShader = compile PS_SHADERMODEL BlurVert();
 //    }
};