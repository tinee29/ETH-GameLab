using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLab
{
    public class Projectile(SpriteBatch sb, Vector2 position, Vector2 destination)
    {
        private Texture2D _enemyTexture;

        protected Vector2 _pos = position;
        protected SpriteBatch _sb = sb;

        protected Vector2 _dist = Globals.RadialMovement(destination, position, 4.5f);

        private const int PROJ_DIM = 32;
        public bool Dead = false;
        private bool _hashit = false;
        private int _diestart;
        private bool _dieing;
        private int _animIdx = 0;
        private bool _isHorizontal;
        private Vector2 _hitOffset;

#pragma warning disable IDE0060 // Remove unused parameter
        public Rectangle GetHitBox(float x, float y)
        {
            int hitboxWidth = (int)(PROJ_DIM * 0.6);
            int hitboxHeight = (int)(PROJ_DIM * 0.6);
            int widthOffset = (PROJ_DIM - hitboxWidth) / 2;
            int heightOffset = (PROJ_DIM - hitboxHeight) / 2;
            return new Rectangle((int)x + widthOffset, (int)y + heightOffset, hitboxWidth, hitboxHeight);
        }
        public void Draw(GameTime gameTime)
#pragma warning restore IDE0060 // Remove unused parameter

        {
            if (_hashit)
            {
                //just looks too weird imo
                return;
            }
            float rot = 0;
            if (_dieing)
            {
                rot = !_isHorizontal ? _dist.X > 0 ? 0 : 180 : _dist.Y > 0 ? 90 : 270;
            }
            rot = MathHelper.ToRadians(rot);
            Vector2 origin = new(8, 8);
            _sb.Draw(
                    _enemyTexture,
                    new Rectangle((int)_pos.X, (int)_pos.Y, 32, 32), //destr
                    new Rectangle(_animIdx * 16, 0, 16, 16), //source
                    Color.White, rot, origin, SpriteEffects.None, 0
                    );
        }

        public void LoadContent(ContentManager content)
        {
            _enemyTexture = content.Load<Texture2D>("img/projectile");
        }
        public Vector2 GetPos()
        {
            return new Vector2(_pos.X, _pos.Y);
        }

        public void Update(GameTime gameTime, Player player, List<Rectangle> mapCollidables)
        {
            // float currDist = Globals.GetDistance(_pos, player.GetPos());
            Rectangle enemRect = new((int)_pos.X, (int)_pos.Y, 32, 32);
            int gameTimeMilli = (int)gameTime.TotalGameTime.TotalMilliseconds;


            if (_dieing)
            {
                if (gameTimeMilli - _diestart > 2500)
                {
                    Dead = true;
                }
                if (_hashit)
                {
                    _pos += Globals.RadialMovement(player.GetPos() + _hitOffset, _pos, 10.0f);
                }
                return;
            }
            float hitBoxDistance = Globals.GetDistanceBetweenRectangles(player.GetHitBox(player.GetPos().X, player.GetPos().Y), GetHitBox(_pos.X, _pos.Y));

            if (hitBoxDistance < 1)
            {
                if (!_hashit)
                {
                    _hashit = true;
                    player.GetHit(gameTime);
                    _hitOffset = _pos - player.GetPos();

                }
                _diestart = gameTimeMilli;
                _dieing = true;
                _animIdx = 1;
            }
            foreach (Rectangle wall in mapCollidables)
            {
                if (wall.Intersects(enemRect))
                {
                    Rectangle intersection = Rectangle.Intersect(wall, enemRect);
                    _isHorizontal = intersection.Width > intersection.Height;
                    _diestart = gameTimeMilli;
                    _dieing = true;
                    _animIdx = 1;
                }
            }
            _pos += _dist;
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }
    }
}