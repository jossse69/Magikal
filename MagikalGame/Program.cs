using Magikal.Core;
using Magikal.Graphics;

namespace MagikalGame
{
    class Program
    {
        static void Main(string[] args)
        {
            var engine = new Engine();
            int ballx = 0;
            int bally = 0;
            int ballvx = 1;
            int ballvy = 1;

            // Load the sprite
            Sprite ballSprite = Sprite.LoadFromImage("assets/ball.png", 0, 0, 16, 16);

            // Attach event handlers
            engine.OnInitialize += () => Console.WriteLine("Engine initialized.");
            engine.OnUpdate += () =>
            {
                // Update the ball
                ballx += ballvx;
                bally += ballvy;
            };
            engine.OnRender += () =>
            {
                engine.PixelScreen.Clear(0x0000ff);
                // Draw the ball sprite
                ballSprite.Draw(engine.PixelScreen, ballx, bally);
            };

            // Attach input event handlers
            engine.OnKeyDown += (key) =>
            {
                if (key == Key.Escape)
                {
                    Console.WriteLine("Escape key pressed. Exiting...");
                    engine.Stop();
                }

                if (key == Key.Right)
                {
                    ballvx = 1;
                }
                else if (key == Key.Left) {
                    ballvx = -1;
                }
                else
                {
                    ballvx = 0;
                }

                if (key == Key.Up)
                {
                    ballvy = -1;
                }
                else if (key == Key.Down)
                {
                    ballvy = 1;
                }
                else {
                    ballvy = 0;
                }
            };

            engine.Initialize("Magikal Game", 160, 120, 5); // 160x120 buffer scaled by 5
            engine.Run();
        }
    }
}