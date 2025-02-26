using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLab
{
    public class UiText(int x, int y, Fonts font, SpriteBatch sb, Color color, string text = "", Func<string> generator = null, bool center = false, Vector2 parentDims = new(), Func<bool> doShow = null) : IGameEntityStaticPos
    {

        private float _x = x;
        private float _y = y;
        private readonly Fonts _font = font;
        private SpriteFont _spriteFont;
        private readonly SpriteBatch _sb = sb;
        private Color _color = color;
        private string _text = text;
        private readonly Func<string> _generator = generator;
        private readonly Func<bool> _doShow = doShow;
        private bool _isVisible = true;
        private readonly bool _center = center;
        private readonly Vector2 _parentDimensions = parentDims;




        public void Draw(GameTime gameTime, float xOffset, float yOffset)
        {
            if (_doShow != null)
            {
                if (!_doShow())
                {
                    return;
                }
            }
            Vector2 textPosition;
            if (_center)
            {
                int text_w = (int)MeasureText().X + 1;
                textPosition = new(_x - xOffset + ((_parentDimensions.X - text_w) / 2), _y - yOffset);

            }
            else
            {
                textPosition = new(_x - xOffset, _y - yOffset);
            }
            if (!_isVisible)
            {
                return;
            }

            if ((_generator != null && !(_text.Length == 0)) || (_generator == null && (_text.Length == 0)))
            {
                throw new ArgumentException("Invalid Arguments for UiText, must have either text or generator given, but not both!");
            }
            if (_generator != null)
            {
                _sb.DrawString(_spriteFont, _generator(), textPosition, _color);

            }
            else
            {
                _sb.DrawString(_spriteFont, _text, textPosition, _color);
            }
        }

        public void Draw(GameTime gameTime)
        {
            throw new NotSupportedException();
        }

        public void Initialize()
        {
        }

        public void LoadContent(ContentManager content)
        {
            _spriteFont = content.Load<SpriteFont>("fonts/" + _font.ToString());

        }

        public void Update(GameTime gameTime)
        {
            return;
        }

        public bool IsVisible()
        {
            return _isVisible;
        }
        public void SetVisible(bool visible)
        {
            _isVisible = visible;
        }
        public Vector2 MeasureText()
        {
            string currentText = _generator != null ? _generator() : _text;
            return _spriteFont.MeasureString(currentText);
        }

        // Method to set the position
        public void SetPosition(Vector2 position)
        {
            _x = position.X;
            _y = position.Y;
        }

        // Method to set the text
        public void SetText(string text)
        {
            _text = text;
        }
        public string GetText()
        {
            return _text;
        }
        public void SetFont(SpriteFont font)
        {
            _spriteFont = font;
        }
    }
}