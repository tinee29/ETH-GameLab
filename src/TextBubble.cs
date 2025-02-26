using System;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace GameLab
{
    public class TextBubble(SpriteBatch spriteBatch, Texture2D bubbleTexture, Vector2 position, bool toMirror) : IGameEntity
    {
        private readonly SpriteBatch _spriteBatch = spriteBatch;
        private readonly Texture2D _bubbleTexture = bubbleTexture;
        private UiText _uiText;
        private Vector2 _position = position;
        private bool _isVisible = true;
        private float _scale;
        private readonly bool _toMirror = toMirror;


        public void Draw(GameTime gameTime)
        {
            if (!_isVisible)
            {
                return;
            }
            SpriteEffects effects = _toMirror ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Vector2 shift = new(10, -(_bubbleTexture.Height * _scale) + (Player.PLAYER_DIM / 4));
            Vector2 bubblePosition = _position + shift;
            _spriteBatch.Draw(_bubbleTexture, bubblePosition, null, Color.White, 0f, Vector2.Zero, new Vector2(_scale, _scale), effects, 0f);
            // Correctly center the text within the scaled and positioned bubble
            Vector2 textSize = _uiText.MeasureText();
            float xOffset = ((_bubbleTexture.Width * _scale) - textSize.X) / 2;
            float yOffset = (_bubbleTexture.Height * _scale * 2 / 5) - (textSize.Y / 2);
            _uiText.SetPosition(bubblePosition);
            _uiText.Draw(gameTime, -xOffset, -yOffset);
        }
        private void CalculateUniformScale()
        {
            if (_uiText == null)
            {
                return;
            }


            float requiredTextWidth = _uiText.MeasureText().X + 20;
            _scale = requiredTextWidth / _bubbleTexture.Width;
            _scale = Math.Min(_scale, 1.0f);
        }
        private void UpdateBubbleSizeAndWrapping()
        {
            string wrappedText = WrapTextToFitWidth(_uiText.GetText(), 25);
            _uiText.SetText(wrappedText);
            CalculateUniformScale();
        }

        private string WrapTextToFitWidth(string text, int maxLineLength)
        {
            string[] words = text.Split(' ');
            StringBuilder result = new();
            string currentLine = "";

            foreach (string word in words)
            {
                if (word.Length > maxLineLength)
                {
                    // If the current line is not empty, add it to result before breaking the word
                    if (!string.IsNullOrEmpty(currentLine))
                    {
                        _ = result.AppendLine(currentLine);
                        currentLine = "";
                    }
                    // Break the word across multiple lines
                    for (int i = 0; i < word.Length; i += maxLineLength - 1) // Allow space for hyphen
                    {
                        int chunkLength = Math.Min(maxLineLength - 1, word.Length - i);
                        if (i + chunkLength < word.Length)
                        {
                            // Add a hyphen only if the chunk ends before the word does
                            _ = result.AppendLine(word.Substring(i, chunkLength) + "-");
                        }
                        else
                        {
                            _ = result.Append(word.Substring(i, chunkLength));
                        }
                    }
                    if (!string.IsNullOrEmpty(currentLine))
                    {
                        _ = result.AppendLine();
                    }
                }
                else if (currentLine.Length + word.Length + 1 > maxLineLength)
                {
                    // Current word will exceed the line length, put it on a new line
                    _ = result.AppendLine(currentLine);
                    currentLine = word;
                }
                else
                {
                    // Add word to current line
                    currentLine += (currentLine.Length > 0 ? " " : "") + word;
                }
            }

            // Append the last line if it's not empty
            if (!string.IsNullOrEmpty(currentLine))
            {
                _ = result.AppendLine(currentLine);
            }

            return result.ToString().TrimEnd('\n');
        }


        public void SetVisible(bool visible)
        {
            _isVisible = visible;
        }

        public void SetPosition(Vector2 position)
        {
            _position = position;
        }

        public void SetText(UiText text)
        {
            _uiText = text;
            ResetBubbleScaleAndSize();
            _uiText.SetVisible(_isVisible);
        }
        private void ResetBubbleScaleAndSize()
        {
            UpdateBubbleSizeAndWrapping();
            CalculateUniformScale();
        }
        public bool IsVisible()
        {
            return _isVisible;
        }

        public void LoadContent(ContentManager content)
        {
            throw new NotImplementedException();
        }
        public void Update(GameTime gameTime)
        {
            throw new NotImplementedException();
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }
    }
}