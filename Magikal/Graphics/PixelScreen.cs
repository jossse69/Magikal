// File: Magikal/Graphics/PixelScreen.cs
using SDL2;
using System;
using System.Drawing;

namespace Magikal.Graphics
{
    public class PixelScreen
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int PixelScale { get; private set; }
        public int WinWidth { get; private set; }
        public int WinHeight { get; private set; }
        private uint[] Buffer;
        private uint[] WinBuffer;

        public PixelScreen(int width, int height, int pixelScale)
        {
            Width = width;
            Height = height;
            PixelScale = pixelScale;
            WinWidth = width * pixelScale;
            WinHeight = height * pixelScale;
            Buffer = new uint[Width * Height];
            WinBuffer = new uint[WinWidth * WinHeight];
        }

        public void SetPixel(int x, int y, uint color)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) return;
            uint bgColor = GetColor(x, y);
            uint blendedColor = AlphaBlend(color, bgColor);
            Buffer[y * Width + x] = blendedColor;
        }

        public uint GetColor(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) return 0;
            return Buffer[y * Width + x];
        }

        public void Clear(uint color)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Buffer[y * Width + x] = color;
                }
            }
        }

        public void Update()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    uint color = Buffer[y * Width + x];
                    for (int sy = 0; sy < PixelScale; sy++)
                    {
                        for (int sx = 0; sx < PixelScale; sx++)
                        {
                            WinBuffer[(y * PixelScale + sy) * WinWidth + (x * PixelScale + sx)] = color;
                        }
                    }
                }
            }
        }

        public void Render(IntPtr renderer)
        {
            IntPtr texture = SDL.SDL_CreateTexture(renderer, SDL.SDL_PIXELFORMAT_ARGB8888, (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING, WinWidth, WinHeight);
            unsafe
            {
                fixed (uint* winBufferPtr = WinBuffer)
                {
                    SDL.SDL_UpdateTexture(texture, IntPtr.Zero, (IntPtr)winBufferPtr, WinWidth * sizeof(uint));
                }
            }
            SDL.SDL_RenderCopy(renderer, texture, IntPtr.Zero, IntPtr.Zero);
            SDL.SDL_DestroyTexture(texture);
        }

        // Primitive rendering methods
        public void DrawLine(int x1, int y1, int x2, int y2, uint color)
        {
            int dx = Math.Abs(x2 - x1), sx = x1 < x2 ? 1 : -1;
            int dy = -Math.Abs(y2 - y1), sy = y1 < y2 ? 1 : -1;
            int err = dx + dy, e2;

            while (true)
            {
                SetPixel(x1, y1, color);
                if (x1 == x2 && y1 == y2) break;
                e2 = 2 * err;
                if (e2 >= dy) { err += dy; x1 += sx; }
                if (e2 <= dx) { err += dx; y1 += sy; }
            }
        }

        public void DrawRect(int x, int y, int width, int height, uint color)
        {
            for (int i = 0; i < width; i++)
            {
                SetPixel(x + i, y, color);
                SetPixel(x + i, y + height - 1, color);
            }
            for (int i = 0; i < height; i++)
            {
                SetPixel(x, y + i, color);
                SetPixel(x + width - 1, y + i, color);
            }
        }

        public void FillRect(int x, int y, int width, int height, uint color)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    SetPixel(x + i, y + j, color);
                }
            }
        }

        public void DrawCircle(int x0, int y0, int radius, uint color)
        {
            int x = radius;
            int y = 0;
            int err = 0;

            while (x >= y)
            {
                SetPixel(x0 + x, y0 + y, color);
                SetPixel(x0 + y, y0 + x, color);
                SetPixel(x0 - y, y0 + x, color);
                SetPixel(x0 - x, y0 + y, color);
                SetPixel(x0 - x, y0 - y, color);
                SetPixel(x0 - y, y0 - x, color);
                SetPixel(x0 + y, y0 - x, color);
                SetPixel(x0 + x, y0 - y, color);

                if (err <= 0)
                {
                    y += 1;
                    err += 2 * y + 1;
                }

                if (err > 0)
                {
                    x -= 1;
                    err -= 2 * x + 1;
                }
            }
        }

        public void FillCircle(int x0, int y0, int radius, uint color)
        {
            int x = radius;
            int y = 0;
            int err = 0;

            while (x >= y)
            {
                for (int i = -x; i <= x; i++)
                {
                    SetPixel(x0 + i, y0 + y, color);
                    SetPixel(x0 + i, y0 - y, color);
                }
                for (int i = -y; i <= y; i++)
                {
                    SetPixel(x0 + i, y0 + x, color);
                    SetPixel(x0 + i, y0 - x, color);
                }

                if (err <= 0)
                {
                    y += 1;
                    err += 2 * y + 1;
                }

                if (err > 0)
                {
                    x -= 1;
                    err -= 2 * x + 1;
                }
            }
        }

        public void DrawPolygon(int[] points, uint color)
        {
            if (points.Length < 4 || points.Length % 2 != 0)
                throw new ArgumentException("Points array must contain pairs of coordinates.");

            for (int i = 0; i < points.Length - 2; i += 2)
            {
                DrawLine(points[i], points[i + 1], points[i + 2], points[i + 3], color);
            }

            DrawLine(points[points.Length - 2], points[points.Length - 1], points[0], points[1], color);
        }
        public static uint AlphaBlend(uint fgColor, uint bgColor)
        {
            byte fgA = (byte)((fgColor >> 24) & 0xFF);
            byte fgR = (byte)((fgColor >> 16) & 0xFF);
            byte fgG = (byte)((fgColor >> 8) & 0xFF);
            byte fgB = (byte)(fgColor & 0xFF);

            byte bgA = (byte)((bgColor >> 24) & 0xFF);
            byte bgR = (byte)((bgColor >> 16) & 0xFF);
            byte bgG = (byte)((bgColor >> 8) & 0xFF);
            byte bgB = (byte)(bgColor & 0xFF);

            byte outA = (byte)(fgA + bgA * (255 - fgA) / 255);
            byte outR = (byte)((fgR * fgA + bgR * bgA * (255 - fgA) / 255) / 255);
            byte outG = (byte)((fgG * fgA + bgG * bgA * (255 - fgA) / 255) / 255);
            byte outB = (byte)((fgB * fgA + bgB * bgA * (255 - fgA) / 255) / 255);

            return (uint)(outA << 24 | outR << 16 | outG << 8 | outB);
        }

        public void FillPolygon(int[] points, uint color)
        {
            // Simple scanline fill algorithm
            if (points.Length < 6 || points.Length % 2 != 0)
                throw new ArgumentException("Points array must contain pairs of coordinates.");

            int minY = int.MaxValue, maxY = int.MinValue;
            for (int i = 1; i < points.Length; i += 2)
            {
                if (points[i] < minY) minY = points[i];
                if (points[i] > maxY) maxY = points[i];
            }

            for (int y = minY; y <= maxY; y++)
            {
                var nodes = new System.Collections.Generic.List<int>();
                int j = points.Length - 2;
                for (int i = 0; i < points.Length; i += 2)
                {
                    int yi = points[i + 1];
                    int yj = points[j + 1];
                    if (yi < y && yj >= y || yj < y && yi >= y)
                    {
                        int xi = points[i];
                        int xj = points[j];
                        int x = xi + (y - yi) * (xj - xi) / (yj - yi);
                        nodes.Add(x);
                    }
                    j = i;
                }

                nodes.Sort();
                for (int i = 0; i < nodes.Count; i += 2)
                {
                    if (i + 1 < nodes.Count)
                    {
                        for (int x = nodes[i]; x <= nodes[i + 1]; x++)
                        {
                            SetPixel(x, y, color);
                        }
                    }
                }
            }
        }
    }
}
