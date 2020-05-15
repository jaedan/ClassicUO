#region license
// Copyright (C) 2020 ClassicUO Development Community on Github
// 
// This project is an alternative client for the game Ultima Online.
// The goal of this is to develop a lightweight client considering
// new technologies.
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
// 
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;

using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Utility;

using SDL2;

namespace ClassicUO.Game.UI.Controls
{
    internal class KeyBindBox : Control
    {
        private readonly HoveredLabel _label;

        private bool _capturing = false;

        private KeyCombination _keyCombo;

        public KeyBindBox(int x, int y, int width, int height, KeyCombination keyCombo)
        {
            CanMove = false;
            AcceptMouseInput = true;
            AcceptKeyboardInput = true;

            _keyCombo = keyCombo;

            X = x;
            Y = y;
            Width = width;
            Height = height;

            ResizePic background = new ResizePic(0x0BB8)
            {
                Width = width,
                Height = height,
                AcceptKeyboardInput = true
            };

            Add(background);

            background.MouseUp += (sender, e) => { OnMouseUp(0, 0, e.Button); };

            Add(_label = new HoveredLabel(string.Empty, false, 1, 0x0021, 0x0021, 150, 1, FontStyle.Italic, TEXT_ALIGN_TYPE.TS_CENTER)
            {
                Y = 5
            });

            _label.MouseUp += (sender, e) => { OnMouseUp(0, 0, e.Button); };

            UpdateLabel();
            WantUpdateSize = false;
        }

        public SDL.SDL_Keycode Key
        {
            get
            {
                return _keyCombo.Key;
            }
            private set
            {
                _keyCombo.Key = value;
            }
        }

        public SDL.SDL_Keymod Mod
        {
            get
            {
                return _keyCombo.Mod;
            }
            private set
            {
                _keyCombo.Mod = value;
            }
        }

        public event EventHandler OnKeyBindSet, OnKeyBindCleared;

        private void UpdateLabel()
        {
            if (Key == SDL.SDL_Keycode.SDLK_UNKNOWN)
            {
                _label.Text = "Not bound";
                return;
            }

            string s = KeysTranslator.TryGetKey(Key, Mod);

            if (string.IsNullOrEmpty(s))
            {
                _label.Text = "Not bound";
                return;
            }

            _label.Text = s;
        }

        protected override void OnKeyDown(SDL.SDL_Keycode key, SDL.SDL_Keymod mod)
        {
            if (!_capturing)
                return;

            switch (key)
            {
                case SDL.SDL_Keycode.SDLK_UNKNOWN:
                case SDL.SDL_Keycode.SDLK_LCTRL:
                case SDL.SDL_Keycode.SDLK_RCTRL:
                case SDL.SDL_Keycode.SDLK_LALT:
                case SDL.SDL_Keycode.SDLK_RALT:
                case SDL.SDL_Keycode.SDLK_LSHIFT:
                case SDL.SDL_Keycode.SDLK_RSHIFT:
                    break;
                case SDL.SDL_Keycode.SDLK_ESCAPE:
                    _capturing = false;
                    UpdateLabel();
                    break;
                case SDL.SDL_Keycode.SDLK_BACKSPACE:
                    // Hitting backspace clears the keybind
                    _capturing = false;
                    Key = SDL.SDL_Keycode.SDLK_UNKNOWN;
                    Mod = SDL.SDL_Keymod.KMOD_NONE;
                    OnKeyBindCleared?.Invoke(this, null);
                    UpdateLabel();
                    break;
                case SDL.SDL_Keycode.SDLK_KP_ENTER:
                case SDL.SDL_Keycode.SDLK_RETURN:
                    _capturing = false;
                    if (string.IsNullOrWhiteSpace(_label.Text))
                    {
                        Key = SDL.SDL_Keycode.SDLK_UNKNOWN;
                        Mod = SDL.SDL_Keymod.KMOD_NONE;
                        OnKeyBindCleared?.Invoke(this, null);
                    }
                    UpdateLabel();
                    break;
                default:
                    if (mod != SDL.SDL_Keymod.KMOD_NONE)
                        Mod = mod;

                    // As soon as they press a valid key, we're done capturing
                    _capturing = false;
                    Key = key;
                    UpdateLabel();
                    OnKeyBindSet?.Invoke(this, null);
                    break;
            }      
        }

        protected override void OnMouseUp(int x, int y, MouseButtonType button)
        {
            switch (button)
            {
                case MouseButtonType.Left:
                    _label.Text = "";
                    _capturing = true;
                    SetKeyboardFocus();
                    break;
            }

        }
    }
}