using System;
using System.Collections.Generic;


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLab
{
    public class TomatoEnemy(SpriteBatch sb, Vector2 position, Action<int, GameTime> killCallback, EnemySpawns spawn, Map map, SoundManager soundManager) : Enemy(sb, position, killCallback, spawn, soundManager)
    {
        private Texture2D _enemyTexture;
        private readonly float _enemySpeed = 1.4f;
        private static new readonly int ENEM_DIM = 90;

        private static readonly int RUN_FRAMES_START_IDX = 2;
        private static readonly int NUMBER_OF_ENEMY_RUN_FRAMES = 4;
        private static readonly int ENEMY_RUN_FRAME_DURATION = 90;

        private static readonly int IDLE_FRAMES_START_IDX = 0;
        private static readonly int NUMBER_OF_ENEMY_IDLE_FRAMES = 2;
        private static readonly int ENEMY_IDLE_FRAME_DURATION = 500;

        private static readonly int ATTACK_FRAMES_START_IDX = 6;
        private static readonly int NUMBER_OF_ENEMY_ATTACK_FRAMES = 5;
        private static readonly int ENEMY_ATTACK_FRAME_DURATION = 100;

        private static readonly int DEATH_FRAMES_START_IDX = 15;
        private static readonly int NUMBER_OF_DEATH_FRAMES = 9;
        private static readonly int DEATH_FRAME_DURATION = 200;

        private static readonly int HURT_FRAMES_START_IDX = 11;
        private static readonly int NUMBER_OF_HURT_FRAMES = 4;
        private static readonly int HURT_FRAME_DURATION = 120;

        private readonly int _fleeRange = 150;
        private bool _canAttack = true;
        private readonly float _attackRange = 450;
        private int _hitPoints = 2;

        private readonly int _attackCD = 3000;
        private int _attackStart = 0;
        private bool _attacking = false;
        private bool _shot;
        private bool _hurting = false;
        private int _hurtStart;
        private readonly Map _map = map;

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
            LookLeft = player.GetPos().X < _pos.X;

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
            if (_hurting)
            {
                int hurt_progress = (gameTimeMilli - _hurtStart) / HURT_FRAME_DURATION;
                if (hurt_progress >= NUMBER_OF_HURT_FRAMES)
                {
                    _hurting = false;
                }
                else
                {
                    _animIdx = HURT_FRAMES_START_IDX + hurt_progress;
                }
                return;
            }
            else if (currDist < _fleeRange)
            {//run away from player
                _pos = FindPath(mapCollidables, _pos, _pos + (_pos - player.GetPos()), _enemySpeed);
                _animIdx = RUN_FRAMES_START_IDX + (gameTimeMilli / ENEMY_RUN_FRAME_DURATION % NUMBER_OF_ENEMY_RUN_FRAMES);
                LookLeft = !LookLeft;
            }
            else if (currDist < _attackRange)
            { //atatck player if in range
                if (!_attacking)
                {
                    if (_canAttack)
                    {
                        _attackStart = gameTimeMilli;
                        _shot = false;
                        _attacking = true;
                        _canAttack = false;
                    }
                    _animIdx = (gameTimeMilli / ENEMY_IDLE_FRAME_DURATION % NUMBER_OF_ENEMY_IDLE_FRAMES) + IDLE_FRAMES_START_IDX;
                }
                else
                {
                    //something with the attackCD and stuff doesnt work
                    int attack_progress = (gameTimeMilli - _attackStart) / ENEMY_ATTACK_FRAME_DURATION;
                    if (attack_progress > NUMBER_OF_ENEMY_ATTACK_FRAMES)
                    {
                        _attacking = false;
                    }
                    else
                    {
                        if (attack_progress == NUMBER_OF_ENEMY_ATTACK_FRAMES - 2)
                        {
                            if (!_shot)
                            {
                                Vector2 projSpawn = new(_pos.X + 40, _pos.Y + 30);
                                _map.AddProjectile(projSpawn, player.GetPos());
                                _shot = true;
                            }
                        }
                        _animIdx = attack_progress + ATTACK_FRAMES_START_IDX;
                    }
                }
            }
            else
            {//idle
                _animIdx = (gameTimeMilli / ENEMY_IDLE_FRAME_DURATION % NUMBER_OF_ENEMY_IDLE_FRAMES) + IDLE_FRAMES_START_IDX;
            }
            _canAttack = _attackCD - ((int)gameTime.TotalGameTime.TotalMilliseconds - _attackStart) < 0;
        }
        public override void LoadContent(ContentManager content)
        {
            _enemyTexture = content.Load<Texture2D>("img/rottentomato-sheet");
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
                _hurting = true;
                _hurtStart = (int)gameTime.TotalGameTime.TotalMilliseconds;
                if (_attacking)
                {
                    _attacking = false;
                    _canAttack = true;
                }
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