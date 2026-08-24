using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SurvivorGame.Utilitarios
{
    public static class UiAuxiliar
    {
        private static Texture2D _pixel;

        public static void DrawRectangle(SpriteBatch spriteBatch, Rectangle rect, Color color)
        {
            if (_pixel == null)
            {
                _pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                _pixel.SetData(new[] { Color.White });
            }
            spriteBatch.Draw(_pixel, rect, color);
        }
    }
}
