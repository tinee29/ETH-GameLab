using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLab
{
    public class WatermelonEnemy(SpriteBatch sb, Vector2 position, Action<int, GameTime> killCallback, EnemySpawns spawn, SoundManager soundManager) : Enemy(sb, position, killCallback, spawn, soundManager)
    {
        private Texture2D _enemyTexture;

        private static new readonly int ENEM_DIM = 90;

        private readonly float _enemySpeed = 5.5f;

        protected static readonly int NUMBER_OF_ENEMY_RUN_FRAMES = 6;
        protected static readonly int ENEMY_RUN_FRAME_DURATION = 90;

        protected static readonly int IDLE_FRAMES_START_IDX = 0;
        protected static readonly int NUMBER_OF_ENEMY_IDLE_FRAMES = 1;
        protected static readonly int ENEMY_IDLE_FRAME_DURATION = 500;

        protected static readonly int ATTACK_FRAMES_START_IDX = 0;
        protected static readonly int NUMBER_OF_ENEMY_ATTACK_FRAMES = 6;
        protected static readonly int ENEMY_ATTACK_FRAME_DURATION = 180;

        protected static readonly int DEATH_FRAMES_START_IDX = 11;
        protected static readonly int NUMBER_OF_DEATH_FRAMES = 8;
        protected static readonly int DEATH_FRAME_DURATION = 200;

        protected static readonly int STUNNED_FRAMES_START_IDX = 6;
        protected static readonly int NUMBER_OF_STUNNED_FRAMES = 2;
        protected static readonly int STUNNED_FRAME_DURATION = 400;

        private readonly float _attackRange = 500;
        private Vector2 _attackDest;
        private int _attackStart = 0;
        private bool _attacking = false;
        private bool _hasHit = false;
        private int _hitPoints = 1;
        private int _stunnedStart;
        private bool _stunned;
        private Vector2 _movement;

        public override Rectangle GetHitBox(float x, float y)
        {
            int hitboxWidth = (int)(ENEM_DIM * 0.6);  // 60% of the full player dimension for the width
            int widthOffset = (ENEM_DIM - hitboxWidth) / 2;  // Offset to center the hitbox horizontally within the player's full width
            return new Rectangle((int)x + widthOffset, (int)y, hitboxWidth, ENEM_DIM);
        }
        public override void Draw(GameTime gameTime)
        {

            _sb.Draw(
                    _enemyTexture,
                    new Rectangle((int)_pos.X, (int)_pos.Y, ENEM_DIM, ENEM_DIM), //destr
                    new Rectangle(_animIdx * ENEM_DIM_SOURCE, 0, ENEM_DIM_SOURCE, ENEM_DIM_SOURCE), //source
                    Color.White, 0F, Vector2.Zero, LookLeft ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0
                    );
        }

        public override void Update(GameTime gameTime, Player player, List<Rectangle> mapCollidables)
        {

            float currDist = Globals.GetDistance(_pos, player.GetPos());
            int gameTimeMilli = (int)gameTime.TotalGameTime.TotalMilliseconds;

            if (_isDieing)
            {
                int deathprogress = (gameTimeMilli - _deathStart) / DEATH_FRAME_DURATION;
                _animIdx = deathprogress + DEATH_FRAMES_START_IDX;
                if (deathprogress > NUMBER_OF_DEATH_FRAMES - 1)
                {
                    Dead = true;
                }
                else
                {
                    _animIdx = deathprogress + DEATH_FRAMES_START_IDX;
                }
                return;
            }
            if (_stunned)
            {
                if (gameTimeMilli - _stunnedStart > 3000)
                {
                    _stunned = false;
                }
                _animIdx = STUNNED_FRAMES_START_IDX + (gameTimeMilli / STUNNED_FRAME_DURATION % NUMBER_OF_STUNNED_FRAMES);
            }
            else
            {
                float hitBoxDistance = Globals.GetDistanceBetweenRectangles(player.GetHitBox(player.GetPos().X, player.GetPos().Y), GetHitBox(_pos.X, _pos.Y));

                if (currDist < _attackRange & !_attacking)
                {
                    _attacking = true;
                    _attackStart = (int)gameTime.TotalGameTime.TotalMilliseconds;
                    _attackDest = player.GetPos();
                    _hasHit = false;
                    LookLeft = player.GetPos().X < _pos.X;
                }
                if (_attacking)
                {
                    _movement = Globals.RadialMovement(_attackDest, _pos, _enemySpeed);

                    Rectangle enemRect = new((int)(_pos.X + _movement.X), (int)(_pos.Y + _movement.Y), ENEM_DIM, ENEM_DIM);
                    //if wall hit then stunned
                    if (mapCollidables.Any(wall => wall.Intersects(enemRect)))
                    {
                        _attacking = false;
                        _stunned = true;
                        _stunnedStart = (int)gameTime.TotalGameTime.TotalMilliseconds;
                    }
                    else
                    {
                        _pos += _movement;
                        _attackDest += _movement;
                        if (hitBoxDistance < 1 && !_hasHit)
                        {
                            player.GetHit(gameTime);
                            _hasHit = true;
                        }
                        int attack_progress = (gameTimeMilli - _attackStart) / ENEMY_ATTACK_FRAME_DURATION;
                        _animIdx = attack_progress % NUMBER_OF_ENEMY_ATTACK_FRAMES;
                        _animIdx += ATTACK_FRAMES_START_IDX;
                    }
                }
                else
                { //return to initial position 
                    if (_pos != _initialPos)
                    {
                        _pos = FindPath(mapCollidables, _pos, _initialPos, _enemySpeed);
                        _animIdx = gameTimeMilli / ENEMY_RUN_FRAME_DURATION % NUMBER_OF_ENEMY_RUN_FRAMES;
                    }
                    else
                    {
                        _animIdx = (gameTimeMilli / ENEMY_IDLE_FRAME_DURATION % NUMBER_OF_ENEMY_IDLE_FRAMES) + IDLE_FRAMES_START_IDX;
                    }
                }
            }
        }

        public override void LoadContent(ContentManager content)
        {
            _enemyTexture = content.Load<Texture2D>("img/watermelone-Sheet");
        }
        public override void Debug()
        {
            Dead = true;
        }
        public override void GetHit(GameTime gameTime)
        {

            if (_hitPoints > 1)
            {
                _hitPoints--;
            }
            else
            {
                _isDieing = true;
                CanBeHit = false;
                _deathStart = (int)gameTime.TotalGameTime.TotalMilliseconds;
                _soundManager.PlaySound("banana-death");
                _mySpawn.AllowNewSpawns(gameTime);
                _killCallback(KILL_REWARD_MS, gameTime);
            }
        }
    }
}
