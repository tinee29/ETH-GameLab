using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameLab
{

    public class GraphicsMenu(int x, int y, int spriteWidth, int spriteHeight, int numSprites, string textureName, SpriteBatch spriteBatch, int spriteStartIdx = 0, double scale = 1.0, Direction direction = Direction.Vertical) : IGameEntityStaticPos, IKeyBoardNavigable
    {
        private readonly int _spriteWidth = spriteWidth;
        private readonly int _spriteHeight = spriteHeight;
        private readonly double _scale = scale;

        private readonly string _textureName = textureName;
        private readonly int _x = x;
        private readonly int _y = y;
        private Texture2D _texture;
        private int _currentIdx = spriteStartIdx;
        private readonly int _lowestIdx = spriteStartIdx;
        private readonly int _highestIdx = spriteStartIdx + numSprites - 1;
        private readonly int _numSprites = numSprites;
        private readonly SpriteBatch _spriteBatch = spriteBatch;
        private readonly Dictionary<int, Action<GameTime>> _actions = [];
        private readonly Direction _direction = direction;

        public void Draw(GameTime gameTime, float xOffset, float yOffset)
        {
            _spriteBatch.Draw(_texture, new Rectangle(_x - (int)xOffset, _y - (int)yOffset, (int)(_spriteWidth * _scale), (int)(_scale * _spriteHeight)), new Rectangle(_currentIdx * _spriteWidth, 0, _spriteWidth, _spriteHeight), Color.White);

        }

        public void SetAction(int idx, Action<GameTime> action)
        {

            if (_lowestIdx > idx || idx >= _numSprites + _lowestIdx)
            {
                throw new InvalidOperationException("bad index for SetAction");
            }
            _actions[idx] = action;
        }

        public void LoadContent(ContentManager content)
        {
            _texture = content.Load<Texture2D>(_textureName);
        }

        public void Draw(GameTime gameTime)
        {
            throw new NotSupportedException();
        }

        public void Update(GameTime gameTime)
        {
            //TODO
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        private void Click(GameTime gameTime)
        {
            if (_actions.ContainsKey(_currentIdx))
            {
                _actions[_currentIdx](gameTime);
            }
            else
            {
                // Console.WriteLine("no action set for menu item {0} on menu with graphic {1}", _currentIdx, _textureName);
            }

        }
        public void HandleInput(GameTime gameTime, KeyboardState newks, KeyboardState oldks, GamePadState newgs, GamePadState oldgs)
        {



            bool enter = Globals.AnyKeyOrButtonNewlyPressed([Keys.Enter, Keys.Space], [Buttons.A, Buttons.RightTrigger], newks, oldks, newgs, oldgs);
            if (enter)
            {

                Click(gameTime);
            }

            bool goNext, goPrev;

            if (_direction == Direction.Vertical)
            {

                goNext = Globals.AnyKeyOrButtonNewlyPressed([Keys.S, Keys.Down], [Buttons.DPadDown, Buttons.LeftThumbstickDown, Buttons.RightThumbstickDown], newks, oldks, newgs, oldgs);
                goPrev = Globals.AnyKeyOrButtonNewlyPressed([Keys.W, Keys.Up], [Buttons.DPadUp, Buttons.LeftThumbstickUp, Buttons.RightThumbstickUp], newks, oldks, newgs, oldgs);

            }
            else if (_direction == Direction.Horizontal)
            {

                goNext = Globals.AnyKeyOrButtonNewlyPressed([Keys.D, Keys.Right], [Buttons.DPadRight, Buttons.LeftThumbstickRight, Buttons.RightThumbstickRight], newks, oldks, newgs, oldgs);
                goPrev = Globals.AnyKeyOrButtonNewlyPressed([Keys.A, Keys.Left], [Buttons.DPadLeft, Buttons.LeftThumbstickLeft, Buttons.RightThumbstickLeft], newks, oldks, newgs, oldgs);
            }
            else
            {
                throw new InvalidOperationException("Invalid direction given to GraphicsMenu");
            }

            int indx_mod = 0;
            if (goNext)
            {
                indx_mod++;
            }
            if (goPrev)
            {
                indx_mod--;
            }

            {
                if (indx_mod != 0)
                {

                    if (indx_mod < 0 && _currentIdx == _lowestIdx)
                    {
                        _currentIdx = _highestIdx;
                    }
                    else if (indx_mod > 0 && _currentIdx == _highestIdx)
                    {
                        _currentIdx = _lowestIdx;
                    }
                    else
                    {

                        _currentIdx += indx_mod;

                    }




                }
            }

        }

        public bool IsVisible()
        {
            return true;
        }

        public void Update(GameTime gameTime, KeyboardState newks, KeyboardState oldks, GamePadState newgs, GamePadState oldgs)
        {
            HandleInput(gameTime, newks, oldks, newgs, oldgs);
            // throw new System.NotImplementedException();
        }
    }
}