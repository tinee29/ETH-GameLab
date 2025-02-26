using System;
using System.Collections.Generic;
//using System.IO;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLab
{


    public abstract class Enemy(SpriteBatch sb, Vector2 position, Action<int, GameTime> killCallback, EnemySpawns spawn, SoundManager soundManager) : IHittableEntity
    {
        private readonly float _enemySpeed = 2.0f;
        private readonly float _epsilon = 0.01f;
        protected static readonly int KILL_REWARD_MS = 5000;

        public static readonly int ENEM_DIM_SOURCE = 64;
        public static readonly int ENEM_DIM = 90;
        protected Vector2 _pos = position;
        protected Vector2 _initialPos = position;
        protected SpriteBatch _sb = sb;
        protected SoundManager _soundManager = soundManager;

        protected bool _isDieing = false;
        protected int _deathStart = 0;
        public bool Dead = false;
        public bool CanBeHit = true;
        public float HitDist = 30;

        protected int _animIdx = 0;
        protected readonly Action<int, GameTime> _killCallback = killCallback;


        public bool LookLeft = false;
#pragma warning disable IDE0052 // Remove unread private members

        protected readonly EnemySpawns _mySpawn = spawn;
#pragma warning restore IDE0052 // Remove unread private members


        public Vector2 GetPos()
        {
            return new Vector2(_pos.X, _pos.Y);
        }
        //public Rectangle GetHitBox(float x, float y)
        //{
        //    int hitboxWidth = (int)(ENEM_DIM * 0.6);  // 60% of the full player dimension for the width
        //    int widthOffset = (ENEM_DIM - hitboxWidth) / 2;  // Offset to center the hitbox horizontally within the player's full width
        //    return new Rectangle((int)x + widthOffset, (int)y, hitboxWidth, ENEM_DIM);
        //}
        public abstract Rectangle GetHitBox(float x, float y);


        public abstract void Draw(GameTime gameTime);

        public virtual void LoadContent(ContentManager content)
        {
        }
        public abstract void Debug();
        public void AI(Player player, List<Rectangle> mapCollidables)
        {
            if (!player.GetHitBox(player.GetPos().X, player.GetPos().Y).Intersects(GetHitBox(_pos.X, _pos.Y)))
            {
                _pos = FindPath(mapCollidables, _pos, player.GetPos(), _enemySpeed);
            }

        }

        public Vector2 FindPath(List<Rectangle> mapCollidables, Vector2 pos, Vector2 target, float speed)
        {
            Vector2 newPos = Globals.RadialMovement(target, pos, speed);
            bool xFree, yFree;
            xFree = yFree = false;

            // Attempt horizontal movement
            Rectangle enemRectX = GetHitBox(pos.X + newPos.X, pos.Y);
            if (!mapCollidables.Any(wall => wall.Intersects(enemRectX)))
            {
                xFree = true;
            }

            // Attempt vertical movement
            Rectangle enemRectY = GetHitBox(pos.X, pos.Y + newPos.Y);
            if (!mapCollidables.Any(wall => wall.Intersects(enemRectY)))
            {
                yFree = true;
            }

            if (xFree && yFree)
            {
                pos += newPos;
            }
            else if (xFree)
            {
                pos.X += (Math.Abs(newPos.X) < _epsilon) ? speed : Math.Sign(newPos.X) * speed;
            }
            else if (yFree)
            {
                pos.Y += (Math.Abs(newPos.Y) < _epsilon) ? speed : Math.Sign(newPos.Y) * speed;
            }
            else
            {
                //File.AppendAllText(Path.Combine(Environment.CurrentDirectory, "log.txt"), "Stuck\n");
            }

            return pos;
        }

        public abstract void Update(GameTime gameTime, Player player, List<Rectangle> mapCollidables);
        public void Update(GameTime gameTime)
        {

        }
        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public abstract void GetHit(GameTime gameTime);
        public bool IsVisible()
        {
            return true;
        }

    }
}
