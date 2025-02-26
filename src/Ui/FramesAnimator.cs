using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLab
{

    public class FramesAnimator : IGameEntityStaticPos
    {

        private readonly string _textureName;
        private readonly SpriteBatch _sb;
        private readonly int _startIdx;
        private readonly int _x, _y;
        private readonly int _spriteHeight;
        private readonly int _spriteWidth;
        private readonly int _frameDuration;
        private event Action DoneCallback;

        private readonly int _step = 1;
        private readonly int _numFrames;

        private int _start_time_ms = -1;
        private Texture2D _texture;

        private readonly float _scale;

        public FramesAnimator(string spriteSheetName, int x, int y, int startIdx, int endIdx, int spriteWidth, int spriteHeight, int frameDuration, SpriteBatch sb, Action doneCallback = null, float scale = 1)
        {
            _x = x;
            _y = y;
            _textureName = spriteSheetName;
            _startIdx = startIdx;
            _spriteHeight = spriteHeight;
            _spriteWidth = spriteWidth;
            _frameDuration = frameDuration;
            _sb = sb;
            _scale = scale;
            if (doneCallback != null)
            {

                DoneCallback += doneCallback;
            }
            _numFrames = endIdx - startIdx;
            if (endIdx < startIdx)
            {
                _step = -1;
                _numFrames = startIdx - endIdx;
            }
            _numFrames++;

            // Initialize();


        }
        public void Draw(GameTime gameTime, float xOffset, float yOffset)
        {
            if (_start_time_ms == -1)
            {
                Update(gameTime);
            }
            int anim_progress = _step * ((int)gameTime.TotalGameTime.TotalMilliseconds - _start_time_ms) / _frameDuration;
            int curr_sprite_idx = _startIdx + anim_progress;
            _sb.Draw(_texture, new Rectangle((int)(_x - xOffset), (int)(_y - yOffset), (int)(_spriteWidth * _scale), (int)(_spriteHeight * _scale)), new Rectangle(curr_sprite_idx * _spriteWidth, 0, _spriteWidth, _spriteHeight), Color.White);
        }
        public void Update(GameTime gameTime)
        {
            if (_start_time_ms == -1)
            {
                _start_time_ms = (int)gameTime.TotalGameTime.TotalMilliseconds;
            }
            int anim_progress = ((int)gameTime.TotalGameTime.TotalMilliseconds - _start_time_ms) / _frameDuration;
            if (anim_progress >= _numFrames)
            {
                DoneCallback?.Invoke();
            }
        }

        public void Draw(GameTime gameTime)
        {
            throw new NotSupportedException("FramesAnimator needs offset values for Draw");
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }


        public bool IsVisible()
        {
            throw new NotImplementedException();
        }

        public void LoadContent(ContentManager content)
        {
            _texture = content.Load<Texture2D>(_textureName);
        }

    }
}