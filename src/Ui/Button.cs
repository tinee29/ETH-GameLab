
using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameLab
{
    public class Button(SpriteBatch sb, Color fg, Color bg, int x, int y, int w, int h, string text, Action<GameTime> onClick) : IGameEntityStaticPos, IDisposable
    {
        private Texture2D _rectTexture;
        private readonly SpriteBatch _sb = sb;
        private readonly Color _foregroundColor = fg;
        private readonly Color _backgroundColor = bg;

        private SpriteFont _font;

        private readonly int _h = h;
        private readonly int _w = w;
        private readonly int _x = x;
        private readonly int _y = y;
        private readonly string _text = text;
        private readonly Action<GameTime> _onClick = onClick;
        private bool _justClicked = true;
        private bool _isVisible = true;

        private bool _selected = false;


        public bool Enabled => throw new NotImplementedException();

        public int UpdateOrder => throw new NotImplementedException();

        public void SetSelected(bool v)
        {
            _selected = v;
        }


        public void Draw(GameTime gameTime)
        {
            throw new NotImplementedException();
        }
        public void Draw(GameTime gameTime, float xOffset, float yOffset)
        {
            int approxTextWidth = 16 * _text.Length;
            int approxPadding = (_w - approxTextWidth) / 2;
            _sb.Draw(_rectTexture, new Rectangle((int)-xOffset + _x, (int)-yOffset + _y, _w, _h), Color.White);
            string draw_text = _text;
            if (_selected)
            {
                draw_text = "**" + draw_text;
            }
            _sb.DrawString(_font, draw_text, new Vector2((int)-xOffset + _x + approxPadding, (int)-yOffset + _y + 5), _foregroundColor);
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public void Click(GameTime gameTime)
        {
            _onClick(gameTime);
        }

        public void LoadContent(ContentManager content)
        {
            _rectTexture = new Texture2D(_sb.GraphicsDevice, 1, 1);
            _rectTexture.SetData(new Color[] { _backgroundColor });
            _font = content.Load<SpriteFont>("fonts/Default");


        }
        public bool MouseOnButton(MouseState ms)
        {
            return ms.X < _x + _w &&
                   ms.X > _x &&
                    ms.Y < _y + _h &&
                   ms.Y > _y;
        }


        public void Update(GameTime gameTime)
        {
            MouseState ms = Mouse.GetState();

            if (!_justClicked && MouseOnButton(ms) && ms.LeftButton == ButtonState.Pressed)
            {
                _onClick(gameTime);
                _justClicked = true;
            }
            else
            {
                if (ms.LeftButton == ButtonState.Released)
                {
                    _justClicked = false;
                }

            }
        }

        public void Dispose()
        {
            _rectTexture.Dispose();
            GC.SuppressFinalize(this);
        }

        public bool IsVisible()
        {
            return _isVisible;
        }
        public void SetVisible(bool b)
        {
            _isVisible = b;
        }
    }
}