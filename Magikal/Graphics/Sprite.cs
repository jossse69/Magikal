// File: Magikal/Graphics/Sprite.cs
using SDL2;
using System;
using System.Drawing;

namespace Magikal.Graphics
{
    public class Sprite
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public uint[] Pixels { get; private set; }

        private Sprite(int width, int height)
        {
            Width = width;
            Height = height;
            Pixels = new uint[width * height];
        }

        public static Sprite LoadFromImage(string imagePath, int x, int y, int width, int height)
        {
            Bitmap bitmap = new Bitmap(imagePath);
            Sprite sprite = new Sprite(width, height);

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    Color color = bitmap.GetPixel(x + j, y + i);
                    sprite.Pixels[i * width + j] = (uint)(color.A << 24 | color.R << 16 | color.G << 8 | color.B);
                }
            }

            return sprite;
        }

        public void Draw(PixelScreen screen, int posX, int posY)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    uint fgColor = Pixels[y * Width + x];
                    if ((fgColor & 0xFF000000) == 0) // Skip fully transparent pixels
                    {
                        continue;
                    }

                    uint bgColor = screen.GetColor(posX + x, posY + y);
                    uint blendedColor = PixelScreen.AlphaBlend(fgColor, bgColor);
                    screen.SetPixel(posX + x, posY + y, blendedColor);
                }
            }
        }

       
    }
}
