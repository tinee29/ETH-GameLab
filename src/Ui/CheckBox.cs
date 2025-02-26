using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameLab
{
    public class CheckBox(SpriteBatch sb, int x, int y, int w, int h, bool val, Action<bool> clickCallback, Action grabFocusCallback) : IGameEntityStaticPos
    {
        private readonly int _x = x;
        private readonly int _y = y;
        private readonly int _w = w;
        private readonly int _h = h;
        private bool _currentState = val;

        private Vector2 _lastGrabbedAt = new();
        private readonly Action<bool> _clickCallback = clickCallback;
        private readonly SpriteBatch _sb = sb;
        private MouseState _lastMouseState;

        private readonly Action _grabFocus = grabFocusCallback;

        private Texture2D _texture;
        public void Draw(GameTime gameTime, float xOffset, float yOffset)
        {
            if (_currentState)
            {
                _sb.Draw(_texture, new Rectangle((int)(_x - xOffset), (int)(_y - yOffset), _w, _h), Color.White);
            }
        }

        public void Draw(GameTime gameTime)
        {
            throw new NotSupportedException("need offset for static ui element");
        }

        public void Initialize()
        {
            throw new NotSupportedException();
        }

        public bool IsVisible()
        {
            return _currentState;
        }

        public void LoadContent(ContentManager content)
        {
            _texture = content.Load<Texture2D>("img/checkmark");
        }

        public void SetCurrent(bool val)
        {
            _currentState = val;
        }

        public void Click()
        {
            _currentState = !_currentState;
            _clickCallback(_currentState);
        }

        public void Update(GameTime gameTime)
        {
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
                    Click();
                }
            }
            _lastMouseState = ms;

        }
    }
}