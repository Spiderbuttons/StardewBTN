#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

static const float PI = 3.14159265359793;
static const int MAX_OFFSET = 12;

float2 Resolution;
float BlurRadius = 5.0f;

Texture2D Texture;
sampler2D Sampler = sampler_state
{
    Texture = <Texture>;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

float GetWeight(int offset)
{
    return exp(-0.5 * (offset * offset) / (BlurRadius * BlurRadius)) / (sqrt(2 * PI) * BlurRadius);
}

float4 Horizontal(VertexShaderOutput input) : COLOR
{
    float4 color = float4(0, 0, 0, 0);
    float2 texelSize = float2(1.0 / Resolution.x, 1.0 / Resolution.y);
    
    for (int i = -MAX_OFFSET; i <= MAX_OFFSET; ++i)
    {
        float weight = GetWeight(i);
        color += tex2D(Sampler, input.TextureCoordinates + float2(i * texelSize.x, 0)) * weight;
    }
    
    return color;
}

float4 Vertical(VertexShaderOutput input) : COLOR
{
    float4 color = float4(0, 0, 0, 0);
    float2 texelSize = float2(1.0 / Resolution.x, 1.0 / Resolution.y);
    
    for (int i = -MAX_OFFSET; i <= MAX_OFFSET; ++i)
    {
        float weight = GetWeight(i);
        color += tex2D(Sampler, input.TextureCoordinates + float2(0, i * texelSize.y)) * weight;
    }
    
    return color;
}

technique GaussianBlur
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL Horizontal();
    }
    pass P1
    {
        PixelShader = compile PS_SHADERMODEL Vertical();
    }
};