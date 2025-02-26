using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameLab
{
    public class Player(SpriteBatch sb, Action<int, GameTime> damageCallback, SoundManager soundManager) : IHittableEntity, IDisposable
    {
        private Vector2 _playerSpeed = new(7, 7);
        private Vector2 _initial_playerSpeed = new(7, 7);
        private readonly SpriteBatch _sb = sb;
        private Texture2D _playerTextureStandard;
        private Texture2D _playerTextureRainbow;
        private Texture2D _dodgeCDTexture;
        private Texture2D _shieldTexture;
        private readonly SoundManager _soundManager = soundManager;

        public static readonly int SLACK = 30;

        public static readonly int PLAYER_DIM = 85;
        public static readonly int PLAYER_DIM_SOURCE = 64;

        private static readonly int PLAYER_RUN_FRAME_DURATION = 70;
        private static readonly int NUMBER_OF_PLAYER_RUN_FRAMES = 8;

        private static readonly int PLAYER_KICK_FRAME_DURATION = 80;
        private static readonly int KICK_FRAMES_START_IDX = 10;
        private static readonly int NUMBER_OF_PLAYER_KICK_FRAMES = 3;

        private static readonly int PLAYER_IDLE_FRAME_DURATION = 500;
        private static readonly int NUMBER_OF_PLAYER_IDLE_FRAMES = 2;
        private static readonly int IDLE_FRAMES_START_IDX = 8;

        private static readonly int PLAYER_DIE_FRAME_DURATION = 400;
        private static readonly int PLAYER_DIE_FRAME_START_IDX = 19;
        private static readonly int NUMBER_OF_PLAYER_DIE_FRAMES = 4;

        private static readonly int PLAYER_DODGE_FRAME_DURATION = 80;
        private static readonly int PLAYER_DODGE_FRAME_START_IDX = 23;
        private static readonly int NUMBER_OF_PLAYER_DODGE_FRAMES = 8;

        private static readonly int PLAYER_HURT_FRAME_DURATION = 100;
        private static readonly int PLAYER_HURT_FRAME_START_IDX = 14;
        private static readonly int NUMBER_OF_PLAYER_HURT_FRAMES = 3;

        private static readonly int PLAYER_SLIDE_FRAME_DURATION = 100;
        private static readonly int PLAYER_SLIDE_FRAME_START_IDX = 30;
        private static readonly int NUMBER_OF_PLAYER_SLIDE_FRAMES = 7;

        private static readonly int DAMAGE_PENALTY_MS = 5000;
        private bool _moving = false;
        private bool _lookLeft = false;
        private bool _kicking = false;
        private int _kick_start_milli;
        public bool IsIntersecting = false;
        private Vector2 _pos;
        private Vector2 _startingPos;
        private int _startingRoomIndex;
        private bool _dodging = false;
        private bool _sliding = false;
        private readonly int _dodgeCost = 1;
        private readonly int _slideCost = 3;
        private readonly int _maxEnergy = 5;
        private int _energy_recharge_start = 0;
        private readonly int _energyCD = 3000;
        private int _energy_move_start = 0;
        private int _energy = 5;
        private bool _hurting = false;
        private int _hurt_start_mili;
        private int _faster_start;


        private readonly double _blinkStartTime = 2000; // Start blinking 2000 ms before the effect ends
        private readonly double _blinkInterval = 300; // Blink every 200 ms
        private bool _invulnerable = false;
        private int _invulnerable_start;
        private readonly int _fastCD = 6000;
        private readonly int _invulnerableCD = 8000;
        private Vector2 _posMod;

        private bool _soundPlayedThisFrame;
        private bool _alreadyHit;
        private bool _dead = false;
        private bool _dieing = false;
        private int _dead_time = 0;
        private int _dead_start_milli = 0;
        private readonly Action<int, GameTime> _damageCallback = damageCallback;
        public event Action<GameState> OnPlayerStateChanged;
        public int GetStartingRoomIndex()
        {
            return _startingRoomIndex;
        }

        public Vector2 GetPos()
        {
            return new Vector2(_pos.X, _pos.Y);
        }
        public Rectangle GetHitBox(float x, float y)
        {
            int hitboxWidth = (int)(PLAYER_DIM * 0.6);  // 60% of the full player dimension for the width
            int widthOffset = (PLAYER_DIM - hitboxWidth) / 2;  // Offset to center the hitbox horizontally within the player's full width
            return new Rectangle((int)x + widthOffset, (int)y, hitboxWidth, PLAYER_DIM);
        }

        public Rectangle GetCollisionBox(float x, float y)
        {
            //int hitboxWidth = (int)(PLAYER_DIM * 0.5);  // 60% of the full player dimension for the width
            //int widthOffset = (PLAYER_DIM - hitboxWidth) / 2;  // Offset to center the hitbox horizontally within the player's full width
            return new Rectangle((int)x + SLACK, (int)y + SLACK, PLAYER_DIM - SLACK * 2, PLAYER_DIM - SLACK);
        }
        public bool HasMoved()
        {
            return _pos != _startingPos;
        }


        public void SetPlayerStartPosition(List<Block> map, int mapSize)
        {
            int i = 0;
            foreach (Block block in map)
            {
                if (block.IsStart)
                {
                    _startingRoomIndex = i;
                    break;
                }
                i++;
            }
            _startingPos = Globals.CalculateGlobalPosition(_startingRoomIndex, mapSize);
            _pos = _startingPos;
        }
        /* FOR DEBUGGING
        public void SetPlayerStartPosition(List<Block> map)
        {

            StartingRoomIndex = 0;
            Pos = Globals.CalculatePlayerStartPosition(StartingRoomIndex);
        }
        */


        /* FOR DEBUGGING
        public void SetPlayerStartPosition(List<Block> map)
        {

            StartingRoomIndex = 0;
            Pos = Globals.CalculatePlayerStartPosition(StartingRoomIndex);
        }
        */
        public void Draw(GameTime gameTime)
        {
            int animIdx;
            int gameTimeMilli = (int)gameTime.TotalGameTime.TotalMilliseconds;
            if (_dieing)
            {
                int dead_progress = (gameTimeMilli - _dead_start_milli) / PLAYER_DIE_FRAME_DURATION;
                animIdx = dead_progress + PLAYER_DIE_FRAME_START_IDX;
            }
            else if (_kicking)
            {
                int kick_progress = (gameTimeMilli - _kick_start_milli) / PLAYER_KICK_FRAME_DURATION;
                animIdx = kick_progress >= NUMBER_OF_PLAYER_KICK_FRAMES
                    ? NUMBER_OF_PLAYER_KICK_FRAMES - (kick_progress % NUMBER_OF_PLAYER_KICK_FRAMES)
                    : kick_progress;
                animIdx += KICK_FRAMES_START_IDX;
            }
            else if (_dodging)
            {
                int dodge_progress = (gameTimeMilli - _energy_move_start) / PLAYER_DODGE_FRAME_DURATION;
                animIdx = dodge_progress + PLAYER_DODGE_FRAME_START_IDX;
            }
            else if (_hurting)
            {
                int hurt_progress = (gameTimeMilli - _hurt_start_mili) / PLAYER_HURT_FRAME_DURATION;
                animIdx = hurt_progress >= NUMBER_OF_PLAYER_HURT_FRAMES
                    ? NUMBER_OF_PLAYER_HURT_FRAMES - (hurt_progress % NUMBER_OF_PLAYER_KICK_FRAMES)
                    : hurt_progress;
                animIdx += PLAYER_HURT_FRAME_START_IDX;
            }
            else if (_sliding)
            {
                int slide_progress = (gameTimeMilli - _energy_move_start) / PLAYER_SLIDE_FRAME_DURATION;
                animIdx = slide_progress + PLAYER_SLIDE_FRAME_START_IDX;
            }
            else
            {
                //Determine the current frame based on the game time (in milliseconds)
                animIdx = _moving
                ? gameTimeMilli / PLAYER_RUN_FRAME_DURATION % NUMBER_OF_PLAYER_RUN_FRAMES
                : IDLE_FRAMES_START_IDX + (gameTimeMilli / PLAYER_IDLE_FRAME_DURATION % NUMBER_OF_PLAYER_IDLE_FRAMES);
            }
            if (_dieing || _dead)
            {
                int dead_progress = Math.Min(NUMBER_OF_PLAYER_DIE_FRAMES - 1,
                    (gameTimeMilli - _dead_start_milli) / PLAYER_DIE_FRAME_DURATION);
                animIdx = dead_progress + PLAYER_DIE_FRAME_START_IDX;
            }

            int remaining_time = _fastCD - (gameTimeMilli - _faster_start);
            Texture2D playerTexture = (_playerSpeed.X > _initial_playerSpeed.X && (remaining_time > _blinkStartTime || remaining_time % (2 * _blinkInterval) < _blinkInterval)) ? _playerTextureRainbow : _playerTextureStandard;
            _sb.Draw(
                playerTexture,
                new Rectangle((int)_pos.X, (int)_pos.Y, PLAYER_DIM, PLAYER_DIM),
                new Rectangle(animIdx * PLAYER_DIM_SOURCE, 0, PLAYER_DIM_SOURCE, PLAYER_DIM_SOURCE),
                Color.White, 0F, Vector2.Zero, _lookLeft ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0
                );

            int barSize = (PLAYER_DIM - 30) / _maxEnergy;
            if (!_dead && !_dieing)
            {
                for (int i = 0; i < _energy; i++)
                {
                    _sb.Draw(_dodgeCDTexture, new Rectangle((int)_pos.X + 5 + (i * 5) + (i * barSize), (int)_pos.Y + PLAYER_DIM + 5, barSize, 5), Color.White);
                }
            }

            remaining_time = _invulnerableCD - (gameTimeMilli - _invulnerable_start);
            if (_invulnerable && (remaining_time > _blinkStartTime || remaining_time % (2 * _blinkInterval) < _blinkInterval))
            {
                int temp = (int)_pos.X - 5;
                if (_lookLeft)
                {
                    temp = (int)_pos.X + 5;
                }
                _sb.Draw(_shieldTexture,
                new Rectangle(temp, (int)_pos.Y, PLAYER_DIM, PLAYER_DIM),
                new Rectangle(0, 0, PLAYER_DIM_SOURCE, PLAYER_DIM_SOURCE),
                Color.White, 0F, Vector2.Zero, SpriteEffects.None, 0
                );
            }
        }

        public void LoadContent(ContentManager content)
        {
            _playerTextureStandard = content.Load<Texture2D>("img/karate-Sheet");
            _playerTextureRainbow = content.Load<Texture2D>("img/karate-fast");
            _shieldTexture = content.Load<Texture2D>("img/shield");
            _dodgeCDTexture = new Texture2D(_sb.GraphicsDevice, 1, 1);
            _dodgeCDTexture.SetData(new Color[] { Color.PaleGoldenrod });
        }


        public void Update(GameTime gameTime, List<Enemy> enemies, List<Rectangle> mapCollidables, KeyboardState newks, KeyboardState oldks, GamePadState newgs, GamePadState oldgs)
        {
            _soundPlayedThisFrame = false;
            int time = (int)gameTime.TotalGameTime.TotalMilliseconds;

            if (_dieing && time >= _dead_time)
            {
                _dead = true;
                OnPlayerStateChanged?.Invoke(GameState.GameOver);
            }
            if (time - _faster_start > _fastCD)
            {
                _playerSpeed = _initial_playerSpeed;
            }
            if (time - _invulnerable_start > _invulnerableCD)
            {
                _invulnerable = false;
            }
            if (_kicking)
            {
                _soundPlayedThisFrame = false;
            }
            if (_energy < _maxEnergy)
            {
                if (time - _energy_recharge_start >= _energyCD)
                {
                    _energy++;
                    _energy_recharge_start = time;
                }
            }

            if (_kicking && !_soundPlayedThisFrame)
            {
                _soundManager.PlaySound("player-kick");
                _soundPlayedThisFrame = true;
                int gameTimeMilli = time;
                int kick_progress = (gameTimeMilli - _kick_start_milli) / PLAYER_KICK_FRAME_DURATION;

                if (!_alreadyHit && kick_progress == NUMBER_OF_PLAYER_KICK_FRAMES)
                {
                    foreach (Enemy en in enemies)
                    {
                        if (!en.CanBeHit)
                        {
                            continue;
                        }
                        if (Globals.GetDistanceBetweenRectangles(GetHitBox(_pos.X, _pos.Y), en.GetHitBox(en.GetPos().X, en.GetPos().Y)) < 1)
                        {
                            en.GetHit(gameTime);
                            _alreadyHit = true;
                        }
                    }
                }

                if (kick_progress >= 2 * NUMBER_OF_PLAYER_KICK_FRAMES)
                {
                    _kicking = false;
                    _alreadyHit = false;
                }
                return;

            }
            if (_dodging)
            {
                int dodge_progress = (time - _energy_move_start) / PLAYER_DODGE_FRAME_DURATION;
                if (dodge_progress == 0 && !_soundPlayedThisFrame)
                {
                    _soundManager.PlaySound("player-dodge");
                    _soundPlayedThisFrame = true;
                }
                if (dodge_progress >= NUMBER_OF_PLAYER_DODGE_FRAMES)
                {
                    _dodging = false;
                }

                AttemptMovement(mapCollidables, 1.2f);

                return;
            }
            if (_sliding)
            {
                int slide_progress = (time - _energy_move_start) / PLAYER_SLIDE_FRAME_DURATION;
                if (slide_progress == 0 && !_soundPlayedThisFrame)
                {
                    _soundManager.PlaySound("player-slide");
                    _soundPlayedThisFrame = true;
                }

                foreach (Enemy en in enemies)
                {
                    if (!en.CanBeHit)
                    {
                        continue;
                    }
                    if (!_alreadyHit)
                    {
                        if (Globals.GetDistanceBetweenRectangles(GetHitBox(_pos.X, _pos.Y), en.GetHitBox(en.GetPos().X, en.GetPos().Y)) < 1)
                        {
                            en.GetHit(gameTime);
                            //_alreadyHit = true;
                        }
                    }
                }

                if (slide_progress >= NUMBER_OF_PLAYER_SLIDE_FRAMES)
                {
                    _sliding = false;
                    //_alreadyHit = false;
                }

                AttemptMovement(mapCollidables, 2f);

                return;
            }
            if (_hurting)
            {
                int hurt_progress = (time - _hurt_start_mili) / PLAYER_HURT_FRAME_DURATION;
                if (hurt_progress >= 2 * NUMBER_OF_PLAYER_HURT_FRAMES)
                {
                    _hurting = false;
                }
                return;
            }
            //movement

            //TODO!!!

            Vector2 leftStickState = newgs.ThumbSticks.Left;
            _posMod = new(0);

            if (leftStickState.LengthSquared() < float.Epsilon * 2) //XXX: magic number
            { //use keyboard
                if (Globals.AnyKeyOrButtonPressed([Keys.W, Keys.Up], [], newks, newgs))
                {
                    _posMod.Y -= 1;
                }
                if (Globals.AnyKeyOrButtonPressed([Keys.S, Keys.Down], [], newks, newgs))
                {
                    _posMod.Y += 1;
                }
                if (Globals.AnyKeyOrButtonPressed([Keys.A, Keys.Left], [], newks, newgs))
                {
                    _posMod.X -= 1;
                }
                if (Globals.AnyKeyOrButtonPressed([Keys.D, Keys.Right], [], newks, newgs))
                {
                    _posMod.X += 1;
                }
                // Normalize the movement vector to prevent faster diagonal movement
                if (_posMod.LengthSquared() > 1)
                {
                    _posMod.Normalize();
                }

            }
            else
            {
                _posMod.X = leftStickState.X;
                _posMod.Y = -leftStickState.Y; //the y state is +1 for straight up, but coordinates need -1 to go up
            }
            _moving = !_posMod.Equals(Vector2.Zero);


            //controls

            if (Globals.AnyKeyOrButtonNewlyPressed([Keys.Space, Keys.K], [Buttons.RightTrigger, Buttons.A], newks, oldks, newgs, oldgs))
            {
                _kicking = true;
                _kick_start_milli = time;
                return;
            }

            if (Globals.AnyKeyOrButtonNewlyPressed([Keys.J], [Buttons.X], newks, oldks, newgs, oldgs) && _energy >= _dodgeCost)
            {
                _dodging = true;
                _energy_move_start = time;
                if (_energy == _maxEnergy)
                {
                    _energy_recharge_start = time;
                }
                _energy -= _dodgeCost;
                return;
            }
            if (Globals.AnyKeyOrButtonNewlyPressed([Keys.L], [Buttons.Y], newks, oldks, newgs, oldgs) && (_posMod.LengthSquared() != 0) && (_energy >= _slideCost))
            {
                _sliding = true;
                _energy_move_start = time;
                if (_energy == _maxEnergy)
                {
                    _energy_recharge_start = time;
                }
                _energy -= _slideCost;
                return;
            }

            AttemptMovement(mapCollidables, 1);

            // Update animation and state based on movement
            if (_posMod.X != 0)
            {
                _lookLeft = _posMod.X < 0;
            }
            if (_moving)
            {
                // _soundManager.SetVolume("player-step", 0.07f);
                PlayStepSoundBasedOnAnimationFrame(gameTime);
            }

        }

        private void AttemptMovement(List<Rectangle> mapCollidables, float speedMod)
        {
            // Attempt horizontal movement
            float newX = _pos.X + (_playerSpeed.X * _posMod.X * speedMod);
            Rectangle newHorizontalHitBox = GetCollisionBox(newX, _pos.Y);
            if (!mapCollidables.Any(wall => wall.Intersects(newHorizontalHitBox)))
            {
                _pos.X = newX;
            }

            // Attempt vertical movement
            float newY = _pos.Y + (_playerSpeed.Y * _posMod.Y * speedMod);
            Rectangle newVerticalHitBox = GetCollisionBox(_pos.X, newY);
            if (!mapCollidables.Any(wall => wall.Intersects(newVerticalHitBox)))
            {
                _pos.Y = newY;
            }
        }

        private void PlayStepSoundBasedOnAnimationFrame(GameTime gameTime)
        {
            int gameTimeMilli = (int)gameTime.TotalGameTime.TotalMilliseconds;
            int currentFrame = gameTimeMilli / PLAYER_RUN_FRAME_DURATION % NUMBER_OF_PLAYER_RUN_FRAMES;

            if ((currentFrame == 1 || currentFrame == 5) && !_soundPlayedThisFrame)
            {
                _soundManager.PlaySound("player-step");
                _soundPlayedThisFrame = true;
            }
        }
        public void Update(GameTime gameTime)
        {
            throw new NotSupportedException("Player needs more arguments for Update!");
        }
        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public void GetHit(GameTime gameTime)
        {
            if (!(_invulnerable || _dodging || _dead))
            {
                _damageCallback(DAMAGE_PENALTY_MS, gameTime);
                _hurting = true;
                _sliding = false;
                _hurt_start_mili = (int)gameTime.TotalGameTime.TotalMilliseconds;
                _soundManager.PlaySound("player-hit");
            }
        }
        public void GetSpeed(GameTime gameTime)
        {
            _playerSpeed = new Vector2(10, 10);
            _faster_start = (int)gameTime.TotalGameTime.TotalMilliseconds;
        }
        public void GetInvulnerable(GameTime gameTime)
        {
            _invulnerable = true;
            _invulnerable_start = (int)gameTime.TotalGameTime.TotalMilliseconds;
        }

        public bool IsVisible()
        {
            return true;
        }

        public void Die(GameTime gameTime)
        {

            if (!(_dieing || _dead))
            {
                _dieing = true;
                _dead_time = (int)(gameTime.TotalGameTime.TotalMilliseconds + (PLAYER_DIE_FRAME_DURATION * NUMBER_OF_PLAYER_DIE_FRAMES));
                _dead_start_milli = (int)gameTime.TotalGameTime.TotalMilliseconds;
                _soundManager.StopSound("main-loop");
                _soundManager.PlaySound("game-over");
                OnPlayerStateChanged?.Invoke(GameState.Dieing);
            }
        }

        public bool IsDieingOrDead()
        {
            return _dieing || _dead;
        }
        public void Reset()
        {
            _dead = false;
            _dieing = false;
            _dodging = false;
            _hurting = false;
            _invulnerable = false;
            _kicking = false;
            _moving = false;
            _lookLeft = false;
            _playerSpeed = _initial_playerSpeed;
            _energy = _maxEnergy;
            _sliding = false;
        }


        public void Dispose()
        {
            _dodgeCDTexture.Dispose();
            GC.SuppressFinalize(this);
        }

    }
}
