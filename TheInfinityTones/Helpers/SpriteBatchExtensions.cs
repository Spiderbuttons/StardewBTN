using Microsoft.Xna.Framework.Graphics;

namespace TheInfinityTones.Helpers;

public static class SpriteBatchExtensions
{
    extension(SpriteBatch b)
    {
        public void BeginDefault()
        {
            b.Begin(
                sortMode: SpriteSortMode.Deferred,
                blendState: BlendState.AlphaBlend,
                samplerState: SamplerState.PointClamp
            );
        }

        public void BeginWithShader(Effect shader)
        {
            b.Begin(
                sortMode: SpriteSortMode.Deferred,
                blendState: BlendState.AlphaBlend,
                samplerState: SamplerState.PointClamp,
                effect: shader
            );
        }
    }
}