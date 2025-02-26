using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLab
{
    public class Timer(SpriteBatch sb) : IGameEntityStaticPos, IDisposable

    {
        private long _endTime = -1;
        private int _pausedTotalMS = 0;
        private bool _paused = false;
        private static readonly int GRAPHIC_HEIGHT = 64;
        private static readonly int GRAPHIC_WIDTH = 1827;
        private static readonly int STARTING_TIME_SECONDS = 40;
        private static readonly int STARTING_TIME_MILLISECONDS = STARTING_TIME_SECONDS * 1000;

        private static readonly int BLINKING_TIME_MS = 450;
        private long _whiteUntil = 0;
        private int _whiteBarWidth = 0;
        private long _pausedRemainingMs = STARTING_TIME_MILLISECONDS;
        // private SpriteFont _font;
        private Texture2D _timerTexture;
        private Texture2D _whiteTexture;
        private readonly SpriteBatch _sb = sb;
        public event Action<GameTime> OnTimerExpired;
        public long GetEndTime()
        {
            return _endTime;
        }
        public void SetEndTime(long time)
        {
            _endTime = time;
        }

        public void Pause(GameTime gameTime)
        {
            if (_paused)
            {
                return;
            }
            _paused = true;
            _pausedTotalMS = (int)gameTime.TotalGameTime.TotalMilliseconds;
            _pausedRemainingMs = GetRemainingTime(gameTime);
        }

        public void UnPause(GameTime gameTime)
        {
            if (!_paused)
            {
                return;
            }
            _paused = false;
            if (_pausedTotalMS == 0)
            {
                throw new InvalidOperationException("aaaaaaaaaa");
            }
            if (_endTime == -1)
            {
                return;
            }


            _endTime += (int)gameTime.TotalGameTime.TotalMilliseconds - _pausedTotalMS;



        }

        public void Draw(GameTime gameTime)
        {
            throw new NotImplementedException();
        }

        public long GetRemainingTime(GameTime gameTime)
        {
            if (_endTime == -1)
            {
                return STARTING_TIME_MILLISECONDS;
            }
            long res = _endTime - (long)gameTime.TotalGameTime.TotalMilliseconds;
            return res;

        }

        private int GetBarWidth(GameTime gameTime)
        {
            long remainingMs = _paused ? _pausedRemainingMs : GetRemainingTime(gameTime);
            int barWidth = (int)(GRAPHIC_WIDTH * ((float)remainingMs / STARTING_TIME_MILLISECONDS));
            return barWidth;

        }


        public void Draw(GameTime gameTime, float xOffset, float yOffset)
        {
            double factor = (double)GameLabGame.W / GRAPHIC_WIDTH;

            //modulo is for blinking effect
            if (_whiteUntil >= gameTime.TotalGameTime.TotalMilliseconds)
            {
                _sb.Draw(_whiteTexture, new Rectangle(10 - (int)xOffset, 10 - (int)yOffset, (int)(_whiteBarWidth * factor), (int)(GRAPHIC_HEIGHT * factor)), new Rectangle(0, 0, _whiteBarWidth, GRAPHIC_HEIGHT), Color.White);
            }
            int barWidth = GetBarWidth(gameTime);
            _sb.Draw(_timerTexture, new Rectangle(10 - (int)xOffset, 10 - (int)yOffset, (int)(barWidth * factor), (int)(GRAPHIC_HEIGHT * factor)), new Rectangle(0, 0, barWidth, GRAPHIC_HEIGHT), Color.White);
        }


        public void LoadContent(ContentManager content)
        {
            _timerTexture = content.Load<Texture2D>("img/timerbarHQ");
            _whiteTexture = content.Load<Texture2D>("img/timerbarwhiteHQ");

        }

        public void StartTimerIfNotStarted(GameTime gameTime)
        {
            if (_endTime != -1)
            {
                return;
            }
            _endTime = (long)gameTime.TotalGameTime.TotalMilliseconds + STARTING_TIME_MILLISECONDS;

        }

        public void Update(GameTime gameTime)
        {
            if (_endTime == -1)//not started
            { return; }
            if (GetRemainingTime(gameTime) <= 0 && !_paused)
            {
                OnTimerExpired?.Invoke(gameTime);
            }
        }

        public void AddTimeMs(int timeToAdd, GameTime gameTime)
        {
            _endTime += timeToAdd;
            if (GetRemainingTime(gameTime) > STARTING_TIME_MILLISECONDS)
            {
                _endTime = (int)gameTime.TotalGameTime.TotalMilliseconds + STARTING_TIME_MILLISECONDS;
            }
        }
        public void RemoveTimeMs(int timeToRemove, GameTime gameTime)
        {
            _whiteBarWidth = GetBarWidth(gameTime);
            _whiteUntil = (long)gameTime.TotalGameTime.TotalMilliseconds + BLINKING_TIME_MS;
            _endTime -= timeToRemove;
        }
        public void ResetTimer()
        {
            _endTime = -1;
            _paused = false;
            _pausedRemainingMs = STARTING_TIME_MILLISECONDS;
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _timerTexture.Dispose();
            GC.SuppressFinalize(this);
        }

        public bool IsVisible()
        {
            return true;
        }

    }
}