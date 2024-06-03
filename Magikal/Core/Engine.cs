// File: Magikal/Core/Engine.cs
using SDL2;
using Magikal.Graphics;
using System;
using System.Collections.Generic;

namespace Magikal.Core
{
    public class Engine
    {
        private IntPtr _window;
        private IntPtr _renderer;
        private bool _running;

        // Public objects
        public PixelScreen PixelScreen;

        // Define events for initialization, update, render, and input
        public event Action OnInitialize;
        public event Action OnUpdate;
        public event Action OnRender;
        public event Action<Key> OnKeyDown;
        public event Action<Key> OnKeyUp;
        public event Action<SDL.SDL_Event> OnMouseButtonDown;
        public event Action<SDL.SDL_Event> OnMouseButtonUp;

        private readonly Dictionary<SDL.SDL_Keycode, Key> _keyMapping = new Dictionary<SDL.SDL_Keycode, Key>
        {
            { SDL.SDL_Keycode.SDLK_ESCAPE, Key.Escape },
            { SDL.SDL_Keycode.SDLK_LEFT, Key.Left },
            { SDL.SDL_Keycode.SDLK_RIGHT, Key.Right },
            { SDL.SDL_Keycode.SDLK_UP, Key.Up },
            { SDL.SDL_Keycode.SDLK_DOWN, Key.Down },
            { SDL.SDL_Keycode.SDLK_SPACE, Key.Space },
            { SDL.SDL_Keycode.SDLK_RETURN, Key.Enter },
            { SDL.SDL_Keycode.SDLK_a, Key.A },
            { SDL.SDL_Keycode.SDLK_b, Key.B },
            { SDL.SDL_Keycode.SDLK_c, Key.C },
            // ... add more mappings as needed
        };

        public void Initialize(string title, int width, int height, int pixelScale)
        {
            PixelScreen = new PixelScreen(width, height, pixelScale);

            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
            {
                throw new Exception($"Could not initialize SDL: {SDL.SDL_GetError()}");
            }

            _window = SDL.SDL_CreateWindow(title, SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED, PixelScreen.WinWidth, PixelScreen.WinHeight, SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);
            if (_window == IntPtr.Zero)
            {
                throw new Exception($"Could not create window: {SDL.SDL_GetError()}");
            }

            _renderer = SDL.SDL_CreateRenderer(_window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
            if (_renderer == IntPtr.Zero)
            {
                throw new Exception($"Could not create renderer: {SDL.SDL_GetError()}");
            }

            // Trigger the OnInitialize event
            OnInitialize?.Invoke();
        }

        public void Run()
        {
            _running = true;

            while (_running)
            {
                HandleEvents();
                SDL.SDL_SetRenderDrawColor(_renderer, 0, 0, 0, 255);
                SDL.SDL_RenderClear(_renderer);

                // Trigger the OnUpdate event
                OnUpdate?.Invoke();

                PixelScreen.Update();
                PixelScreen.Render(_renderer);

                // Trigger the OnRender event
                OnRender?.Invoke();

                SDL.SDL_RenderPresent(_renderer);
            }

            Cleanup();
        }

        private void HandleEvents()
        {
            while (SDL.SDL_PollEvent(out SDL.SDL_Event e) != 0)
            {
                switch (e.type)
                {
                    case SDL.SDL_EventType.SDL_QUIT:
                        _running = false;
                        break;
                    case SDL.SDL_EventType.SDL_KEYDOWN:
                        if (_keyMapping.TryGetValue(e.key.keysym.sym, out var key))
                        {
                            OnKeyDown?.Invoke(key);
                        }
                        break;
                    case SDL.SDL_EventType.SDL_KEYUP:
                        if (_keyMapping.TryGetValue(e.key.keysym.sym, out var key2))
                        {
                            OnKeyUp?.Invoke(key2);
                        }
                        break;
                    case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                        OnMouseButtonDown?.Invoke(e);
                        break;
                    case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                        OnMouseButtonUp?.Invoke(e);
                        break;
                }
            }
        }

        public void Stop()
        {
           _running = false; 
        }

        private void Cleanup()
        {
            SDL.SDL_DestroyRenderer(_renderer);
            SDL.SDL_DestroyWindow(_window);
            SDL.SDL_Quit();
        }
    }
}