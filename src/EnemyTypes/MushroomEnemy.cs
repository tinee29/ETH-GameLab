using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLab
{
    public class MushroomEnemy(SpriteBatch sb, Vector2 position, Action<int, GameTime> killCallback, EnemySpawns spawn, SoundManager soundManager) : Enemy(sb, position, killCallback, spawn, soundManager)
    {
        private Texture2D _enemyTexture;
        private static new readonly int ENEM_DIM = 80;

        private static readonly int IDLE_FRAMES_START_IDX = 0;
        private static readonly int NUMBER_OF_ENEMY_IDLE_FRAMES = 5;
        private static readonly int ENEMY_IDLE_FRAME_DURATION = 150;

        private static readonly int ATTACK_FRAMES_START_IDX = 5;
        private static readonly int NUMBER_OF_ENEMY_ATTACK_FRAMES = 10;
        private static readonly int ENEMY_ATTACK_FRAME_DURATION = 100;

        private static readonly int DEATH_FRAMES_START_IDX = 15;
        private static readonly int NUMBER_OF_DEATH_FRAMES = 6;
        private static readonly int DEATH_FRAME_DURATION = 200;

        private static readonly int[] DEATH_FRAMES_EXPLODED = [12, 13, 14, 18, 19, 20];

        private bool _canAttack = true;
        private int _hitPoints = 1;


        private readonly int _attackCD = 3000;
        private int _attackStart = 0;
        private bool _attacking = false;
        private bool _hasHit = false;
        private bool _exploded;

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

        public override void LoadContent(ContentManager content)
        {
            _enemyTexture = content.Load<Texture2D>("img/mushboom-Sheet");
        }

        public override void Update(GameTime gameTime, Player player, List<Rectangle> mapCollidables)
        {

            int gameTimeMilli = (int)gameTime.TotalGameTime.TotalMilliseconds;
            LookLeft = player.GetPos().X < _pos.X;

            if (_isDieing)
            {
                int deathprogress = (gameTimeMilli - _deathStart) / DEATH_FRAME_DURATION;
                if (deathprogress > NUMBER_OF_DEATH_FRAMES - 1)
                {
                    Dead = true;
                }
                else
                {
                    _animIdx = _exploded ? DEATH_FRAMES_EXPLODED[deathprogress] : deathprogress + DEATH_FRAMES_START_IDX;
                }
                return;
            }
            float hitBoxDistance = Globals.GetDistanceBetweenRectangles(player.GetHitBox(player.GetPos().X, player.GetPos().Y), GetHitBox(_pos.X, _pos.Y));

            if (hitBoxDistance < 1)
            { //atatck player if in range
                if (!_attacking)
                {
                    if (_canAttack)
                    {
                        _soundManager.PlaySound("mushroom-attack");
                        _attackStart = gameTimeMilli;
                        _attacking = true;
                        _canAttack = false;
                        _hasHit = false;
                    }
                }
                else
                {
                    //something with the attackCD and stuff doesnt work
                    int attack_progress = (gameTimeMilli - _attackStart) / ENEMY_ATTACK_FRAME_DURATION;
                    if (attack_progress == NUMBER_OF_ENEMY_ATTACK_FRAMES - 3)
                    {
                        if (hitBoxDistance < 1 && !_hasHit)
                        {
                            player.GetHit(gameTime);
                            _hasHit = true;
                        }
                        _isDieing = true;
                        _deathStart = gameTimeMilli;
                        _exploded = true;
                        CanBeHit = false;
                    }
                    _animIdx = ATTACK_FRAMES_START_IDX + attack_progress;

                }
            }
            else
            {
                _attacking = false;
                _animIdx = IDLE_FRAMES_START_IDX + (gameTimeMilli / ENEMY_IDLE_FRAME_DURATION % NUMBER_OF_ENEMY_IDLE_FRAMES);
            }
            _canAttack = _attackCD - ((int)gameTime.TotalGameTime.TotalMilliseconds - _attackStart) < 0;
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