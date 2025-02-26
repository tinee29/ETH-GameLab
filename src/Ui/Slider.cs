using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameLab
{
    public class Slider(SpriteBatch sb, int x, int y, int w, int h, int currVal, Action<int> setCurrent, Action grabFocusCallback, int step = 1) : IGameEntityStaticPos
    {
        private readonly SpriteBatch _sb = sb;
        private readonly int _x = x;
        private readonly int _y = y;
        private readonly int _step = step;
        private readonly int _w = w;
        private readonly int _h = h;
        private static readonly int SPRITE_H = 10;
        private static readonly int SPRITE_W = 5;

        private Vector2 _lastGrabbedAt = new();

        private MouseState _lastMouseState;
        private int _current = currVal;
        private readonly Action<int> _setCurrentCallback = setCurrent;
        private readonly Action _grabFocus = grabFocusCallback;
        private Texture2D _texture;
        public void Draw(GameTime gameTime, float xOffset, float yOffset)
        {

            int scale = _h / SPRITE_H;

            int x_coord = (int)(_x - xOffset + (_w * (_current / 100.0)));
            _sb.Draw(_texture, new Rectangle(x_coord, (int)(_y - yOffset), SPRITE_W * scale, SPRITE_H * scale), Color.Wheat);

        }

        public void Draw(GameTime gameTime)
        {
            throw new NotSupportedException("need offset for slider");
        }

        public void Initialize()
        {
        }
        public void SetCurrent(int val)
        {
            int tmp = val < 0 ? 0 : val;
            _current = tmp > 100 ? 100 : tmp;
        }

        public bool IsVisible()
        {
            return true;
        }

        public void LoadContent(ContentManager content)
        {
            _texture = content.Load<Texture2D>("img/slider");
        }
        public void StepUp()
        {
            SetCurrent(_current + _step);
            _setCurrentCallback(_current);
        }
        public void StepDown()
        {
            SetCurrent(_current - _step);
            _setCurrentCallback(_current);
        }

        public void Update(GameTime gameTime)
        {
            //TODO: check for input here
            MouseState ms = Mouse.GetState();
            if (ms.X >= _x && ms.X <= _x + _w && ms.Y >= _y && ms.Y <= _y + _h)
            {
                if (!(ms.X == _lastGrabbedAt.X && ms.Y == _lastGrabbedAt.Y))
                {
                    _grabFocus();
                    _lastGrabbedAt = new(ms.X, ms.Y);
                }
                if (ms.LeftButton == ButtonState.Pressed && _lastMouseState.LeftButton == ButtonState.Released)
                {
                    _current = (int)(100 * (((ms.X * 1.0) - _x) / _w));
                    _setCurrentCallback(_current);

                }


            }
            _lastMouseState = ms;

        }

    }
}