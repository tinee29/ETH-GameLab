using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

//using Penumbra;


namespace GameLab
{

    public class GameLabGame : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SoundManager _soundManager;
        private GameStateManager _stateManager;
        private GameState _previousState;
        private Player _player;
        private Exit _exit;
        private Ui _gameUi;
        private Ui _menuUi;
        private Ui _pauseUi;
        private Ui _gameOverUi;
        private Ui _controlsUi;

        private Ui _optionsUi;
        private Map _map;
        //private PenumbraComponent _penumbra;
        //private LightManager _lightManager;
        private Matrix _translation;
        private KeyboardState _lastKeyboardState;
        private UiText _startText;
        private Texture2D _textBubbleTexture;
        private TextBubble _startTextBubble;
        private bool _gameOverInitialized;
        private GamePadState _lastGamePadState;

        private Options _options;
        public const int W = 1920;
        public const int H = 1080;

        public const string OPTIONS_PATH = "options.txt";
        // private readonly List<int> _mapSizes = [3, 5, 10, 15, 20];
        //private readonly List<int> _mapSizes = [3, 4, 5, 6, 7];
        private int _currentLevelIndex = 3;
        private bool _menuMusicPlaying = false;
        private static readonly int FRIDGE_OPEN_CLOSE_FRAME_DURATION = 70;
        private static readonly int POSTIT_ZOOM_FRAME_DURATION = 90;
        private int _run = 0;

        private int _score = 0;
        private int _scoreMultiplier = 1;
        private int _lastTimeScoreCalculated = 0;
        private bool _newHighScore = false;
        private const int LEVEL_COMPLETE_REWARD_MS = 8000;
        private FramesAnimator _transitionAnimation;
        public GameLabGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            if (!_graphics.IsFullScreen)
            {

                _graphics.ToggleFullScreen();
            }
            Window.Title = "No Spoilers!";
        }

        private void StartGameCallback()
        {
            ResetGame();
            _stateManager.ChangeState(GameState.Game);
            PlayGameMusic();
        }
        public void ResetGame()
        {
            Console.WriteLine($"Run: {_run}");
            _soundManager.StopAllSounds();
            _map.Reset(3);
            _map.GenerateMap();

            Timer timer = _gameUi.GetNamedElement<Timer>("timer");
            timer.ResetTimer();

            _player.Reset();
            _gameOverInitialized = false;
            if (_startTextBubble == null)
            {
                _startTextBubble = new TextBubble(_spriteBatch, _textBubbleTexture, _player.GetPos(), false);
                _startTextBubble.SetText(_startText);
            }
            _startTextBubble.SetPosition(_player.GetPos());
            _startTextBubble.SetVisible(_run == 0);

            _run += 1;
            _currentLevelIndex = 3;
            _score = 0;
            _scoreMultiplier = 1;
            _lastTimeScoreCalculated = 0;
            _newHighScore = false;
        }
        private void NextLevelCallback(GameTime gameTime)
        {
            _currentLevelIndex++;
            int mapSize = _currentLevelIndex;
            int minSpawns = _currentLevelIndex - 2;
            int maxSpawns = _currentLevelIndex;
            int itemsCount = mapSize;
            _score += LEVEL_COMPLETE_REWARD_MS * _scoreMultiplier;
            _scoreMultiplier++;
            //Update map
            _map.ConfigureNewLevel(mapSize, minSpawns, maxSpawns, itemsCount);
            _map.GenerateMap();
            //reset timer + bonus time
            Timer timer = _gameUi.GetNamedElement<Timer>("timer");
            long endTime = timer.GetEndTime();
            timer.SetEndTime(endTime);
            timer.AddTimeMs(LEVEL_COMPLETE_REWARD_MS, gameTime);

            //hide start game bubble
            _startTextBubble.SetVisible(false);
        }

        private void CloseFridge(Action next)
        {

            _transitionAnimation = new("img/fridge-close", 0, 0, 0, 5, 1920, 1080, FRIDGE_OPEN_CLOSE_FRAME_DURATION, _spriteBatch, next, 1);
            _transitionAnimation.LoadContent(Content);

            _stateManager.ChangeState(GameState.Transitioning);

        }

        private void OpenFridge()
        {

            _transitionAnimation = new("img/fridge-close", 0, 0, 5, 0, 1920, 1080, FRIDGE_OPEN_CLOSE_FRAME_DURATION, _spriteBatch, () => { _transitionAnimation = null; _stateManager.ChangeState(GameState.Menu); }, 1);
            _transitionAnimation.LoadContent(Content);

            _stateManager.ChangeState(GameState.Transitioning);

        }


        private void ZoomOptions()
        {
            _transitionAnimation = new("img/options-zoom", 0, 0, 0, 3, 1920, 1080, POSTIT_ZOOM_FRAME_DURATION, _spriteBatch, () => { _transitionAnimation = null; _stateManager.ChangeState(GameState.Options); }, 1);
            _transitionAnimation.LoadContent(Content);

            _stateManager.ChangeState(GameState.Transitioning);

        }
        private void ZoomControls()
        {
            _transitionAnimation = new("img/controls-zoom", 0, 0, 0, 3, 1920, 1080, POSTIT_ZOOM_FRAME_DURATION, _spriteBatch, () => { _transitionAnimation = null; _stateManager.ChangeState(GameState.Controls); }, 1);
            _transitionAnimation.LoadContent(Content);

            _stateManager.ChangeState(GameState.Transitioning);

        }

        private void ControlsToMain()
        {
            _transitionAnimation = new("img/controls-zoom", 0, 0, 3, 0, 1920, 1080, POSTIT_ZOOM_FRAME_DURATION, _spriteBatch, OpenFridge, 1);
            _transitionAnimation.LoadContent(Content);

            _stateManager.ChangeState(GameState.Transitioning);
        }

        private void OptionsToMain()
        {
            _transitionAnimation = new("img/options-zoom", 0, 0, 3, 0, 1920, 1080, POSTIT_ZOOM_FRAME_DURATION, _spriteBatch, OpenFridge, 1);
            _transitionAnimation.LoadContent(Content);
            _stateManager.ChangeState(GameState.Transitioning);

        }
        private void MainToOptions()
        {

            CloseFridge(ZoomOptions);

        }

        private void MainToControls()
        {
            CloseFridge(ZoomControls);

        }

        private void Pause(GameTime gameTime)
        {
            _stateManager.ChangeState(GameState.Pause);
            _gameUi.GetNamedElement<Timer>("timer").Pause(gameTime);
        }

        private void UnPause(GameTime gameTime)
        {
            _lastTimeScoreCalculated = (int)gameTime.TotalGameTime.TotalMilliseconds; //so we dont get score for being in the pause menu...
            _stateManager.ChangeState(GameState.Game);
            _gameUi.GetNamedElement<Timer>("timer").UnPause(gameTime);
        }

        private Action<GameTime> ButtonCallDiscardGameTime(Action toCall)
        {
            return _ => toCall();
        }
        private void HandlePlayerStateChange(GameState newState)
        {
            _stateManager.ChangeState(newState);
        }
        private void ConfigureGameUI()
        {
            Timer timer = new(_spriteBatch);
            _gameUi.AddNamedElement("timer", timer);
            // Button pauseButton = new(_spriteBatch, Color.Black, Color.White, W - 130, 25, 120, 50, "Pause", Pause);
            // _gameUi.AddNamedElement("pauseButton", pauseButton);
            UiText scoreText = new(50, 100, Fonts.Default, _spriteBatch, Color.White, generator: GetScoreString);
            _gameUi.AddNamedElement("score", scoreText);
            _startText = new UiText(0, 0, Fonts.TextBox, _spriteBatch, Color.Black, "I need to find the exit, time is running out!");

        }

        private void ConfigureMenuUI()
        {
            GraphicsMenu mainMenu = new(0, 0, 1920, 1080, 4, "img/main-menu", _spriteBatch, spriteStartIdx: 1);
            mainMenu.SetAction(1, _ => StartGameCallback());
            mainMenu.SetAction(2, _ => MainToOptions());
            mainMenu.SetAction(3, _ => MainToControls());
            mainMenu.SetAction(4, _ => ExitGame());
            _menuUi.AddElement(mainMenu);
        }
        private void ConfigurePauseUI()
        {
            GraphicsMenu pauseMenu = new((W / 2) - 128, (H / 2) - 128, 256, 256, 4, "img/pause-menu", _spriteBatch, 1, 1);
            pauseMenu.SetAction(1, UnPause);
            pauseMenu.SetAction(2, _ => _stateManager.ChangeState(GameState.Menu));
            pauseMenu.SetAction(3, _ => _stateManager.ChangeState(GameState.PauseOptions));
            pauseMenu.SetAction(4, _ => ExitGame());
            _pauseUi.AddElement(pauseMenu);
        }
        private void ConfigureGameOverUI()
        {

            Vector2 parentDims = new(W, H);
            GraphicsMenu gameOverMenu = new((W / 2) - 192, (H / 2) + 70, 256, 32, 2, "img/death-menu", _spriteBatch, 1, 1.5, Direction.Horizontal);
            gameOverMenu.SetAction(1, _ => _stateManager.ChangeState(GameState.Menu));
            gameOverMenu.SetAction(2, ButtonCallDiscardGameTime(StartGameCallback)); // Restart the game
            _gameOverUi.AddElement(gameOverMenu);
            UiText gameOverText = new(0, 300, Fonts.Title, _spriteBatch, Color.DarkRed, generator: Spoilers.GetSpoiler, center: true, parentDims: parentDims, doShow: _options.GetState().GetShowSpoilers);
            _gameOverUi.AddElement(gameOverText);
            UiText scoreTextGameOver = new(0, 360, Fonts.Title, _spriteBatch, Color.DarkGoldenrod, generator: GetScoreString, center: true, parentDims: parentDims);
            _gameOverUi.AddElement(scoreTextGameOver);
            UiText newHighScore = new(0, 420, Fonts.Title, _spriteBatch, Color.LimeGreen, "New Highscore!", center: true, parentDims: parentDims, doShow: () => _newHighScore);
            _gameOverUi.AddElement(newHighScore);
            UiText highScoreText = new(0, 420, Fonts.Title, _spriteBatch, Color.SlateGray, generator: GetHighScoreString, center: true, parentDims: parentDims, doShow: () => !_newHighScore);
            _gameOverUi.AddElement(highScoreText);
        }

        private void ConfigureControlsUi()
        {
            GraphicsMenu controlsMenu = new(0, 0, 1920, 1080, 1, "img/controls-zoom", _spriteBatch, 3, 1);
            // controlsMenu.SetAction(3, _ => ControlsToMain());
            _controlsUi.AddElement(controlsMenu);
        }

        private void ConfigureOptionsUi()
        {
            GraphicsMenu optionsMenu = new(0, 0, 1920, 1080, 1, "img/options-zoom", _spriteBatch, 3, 1);
            // optionsMenu.SetAction(3, _ => OptionsToMain());
            _optionsUi.AddElement(optionsMenu);


            //ui elements for options below


            MainMenuOptionsAddon beby = new(_ => OptionsToMain(), _options.GetState(), _spriteBatch);
            _optionsUi.AddElement(beby);
        }
        private void SetupStaticUI()
        {
            _menuUi = new Ui(_spriteBatch);
            _pauseUi = new Ui(_spriteBatch);
            _gameOverUi = new Ui(_spriteBatch);
            _gameUi = new Ui(_spriteBatch);  // Initialize the game UI container
            _controlsUi = new(_spriteBatch);
            _optionsUi = new(_spriteBatch);

            ConfigureMenuUI();
            ConfigurePauseUI();
            ConfigureGameOverUI();
            ConfigureGameUI();
            ConfigureControlsUi();
            ConfigureOptionsUi();

        }

        private void ExitGame()
        {
            _options.SaveState();
            Exit();
        }

        protected override void Initialize()
        {


            _stateManager = new GameStateManager(GameState.None);
            _graphics.PreferredBackBufferWidth = W;
            _graphics.PreferredBackBufferHeight = H;
            //_graphics.ToggleFullScreen();
            _graphics.ApplyChanges();
            _spriteBatch = new(GraphicsDevice);


            OptionsState state = OptionsState.ReadConfig(OPTIONS_PATH);
            _soundManager = new(state);
            _options = new(_soundManager, _stateManager, state, _spriteBatch, 700, 400, OPTIONS_PATH);
            SetupStaticUI();
            Timer timer = _gameUi.GetNamedElement<Timer>("timer");

            _player = new Player(_spriteBatch, timer.RemoveTimeMs, _soundManager);
            _player.OnPlayerStateChanged += HandlePlayerStateChange;

            timer.OnTimerExpired += _player.Die;


            _exit = new Exit(_spriteBatch, _player, NextLevelCallback);
            _map = new Map(_spriteBatch, Content, timer, _player, _exit, _soundManager);

            _previousState = _stateManager.CurrentState;
            _soundManager.PlaySound("menu-loop", true);

            base.Initialize();

            // ==================== LIGHTING ====================
            /*
            _penumbra = new PenumbraComponent(this);
            Components.Add(_penumbra);
            _lightManager = new LightManager(_penumbra);
            */
            // ==================== DEBUG UI ====================

            // _debugUi = new(_spriteBatch);
            // _debugMenu = new(100, 5, Color.HotPink, Color.BlueViolet, Direction.Vertical, _spriteBatch);

            // void Kill()
            // {
            //     timer.SetEndTime(1);
            // }
            // void InfiniteTime()
            // {
            //     timer.SetEndTime(int.MaxValue);
            // }

            // void Subtract5(GameTime gameTime)
            // {
            //     timer.RemoveTimeMs(5000);
            // }

            // _debugMenu.AddButton("Kill self", 400, 40, ButtonCallDiscardGameTime(Kill));
            // _debugMenu.AddButton("Infinite Time", 400, 40, ButtonCallDiscardGameTime(InfiniteTime));
            // _debugMenu.AddButton("Go to Main Menu", 400, 40, GenerateSetCurentStateMethod(CurrentDisplay.Menu));
            // _debugMenu.AddButton("Go to Game", 400, 40, GenerateSetCurentStateMethod(CurrentDisplay.Game));
            // _debugMenu.AddButton("Go to Pause Menu", 400, 40, Pause);
            // _debugMenu.AddButton("Go to Game Over Screen", 400, 40, GenerateSetCurentStateMethod(CurrentDisplay.GameOver));
            // _debugMenu.AddButton("Finish this level", 400, 40, NextLevelCallback);
            // _debugMenu.AddButton("-5s", 400, 40, Subtract5);
            // _debugUi.AddElement(_debugMenu);
            // _debugUi.Hide();
        }


        protected override void LoadContent()
        {
            _soundManager.LoadContent(Content);
            _player.LoadContent(Content);
            _gameUi.LoadContent(Content);
            _pauseUi.LoadContent(Content);
            _gameOverUi.LoadContent(Content);
            _menuUi.LoadContent(Content);
            _controlsUi.LoadContent(Content);
            _options.LoadContent(Content);
            _optionsUi.LoadContent(Content);
            _exit.LoadContent(Content);
            _map.LoadContent();
            _startText.LoadContent(Content);
            _textBubbleTexture = Content.Load<Texture2D>("img/box-bubble");
            // TODO: use this.Content to load your game content here
        }

        private void UpdateCameraPosition()
        {
            float dx = (W / 2) - _player.GetPos().X - (Player.PLAYER_DIM / 2);
            float dy = (H / 2) - _player.GetPos().Y - (Player.PLAYER_DIM / 2);
            _translation = Matrix.CreateTranslation(dx, dy, 0f);
        }
        public void HandleInput()
        {


            //=============DEBUG==================

            // if (Keyboard.GetState().IsKeyDown(Keys.F2) && _lastKeyboardState.IsKeyUp(Keys.F2))//XXX this is for debugging
            // {
            //     _debugUi.ToggleVisibility();
            // }
            // _debugUi.Update(gameTime, ks, _lastKeyboardState, gs, _lastGamePadState);
            //====================================
        }
        private void HandleSoundChange(GameState newState)
        {
            if (newState is GameState.Menu or GameState.GameOver or GameState.Controls or GameState.Options or GameState.Transitioning)
            {
                if (!_menuMusicPlaying)
                {
                    _soundManager.StopAllSounds();
                    _soundManager.PlaySound("menu-loop", true);
                    _menuMusicPlaying = true;
                }
            }
            else
            {
                _menuMusicPlaying = false;
                _soundManager.StopSound("menu-loop");
            }
        }

        private string GetDisplayScore()
        {
            return (_score / 1000).ToString();
        }

        private string GetScoreString()
        {
            return "Score: " + GetDisplayScore();
        }
        private string GetHighScoreString()
        {
            return "Your Highscore: " + _options.GetState().GetHighScore().ToString();
        }

        private void UpdateScore(GameTime gameTime)
        {
            if (_lastTimeScoreCalculated == 0)
            {
                _lastTimeScoreCalculated = (int)gameTime.TotalGameTime.TotalMilliseconds;
                return;
            }
            _score += (int)((gameTime.TotalGameTime.TotalMilliseconds - _lastTimeScoreCalculated) / 1) * _scoreMultiplier;
            _lastTimeScoreCalculated = (int)gameTime.TotalGameTime.TotalMilliseconds;

        }
        protected override void Update(GameTime gameTime)
        {
            HandleInput();
            KeyboardState ks = Keyboard.GetState();
            GamePadState gs = GamePad.GetState(PlayerIndex.One);
            if (_previousState != _stateManager.CurrentState)
            {
                HandleSoundChange(_stateManager.CurrentState);
                _previousState = _stateManager.CurrentState;
            }
            switch (_stateManager.CurrentState)
            {
                case GameState.None:
                    _stateManager.CurrentState = GameState.Menu;
                    break;
                case GameState.Game:
                    if (Globals.AnyKeyOrButtonNewlyPressed([Keys.P], [Buttons.Start], ks, _lastKeyboardState, gs, _lastGamePadState))
                    {
                        Pause(gameTime);
                        break;
                    }
                    if (_player.HasMoved())
                    {
                        UpdateScore(gameTime);
                        _gameUi.GetNamedElement<Timer>("timer").StartTimerIfNotStarted(gameTime);
                        _startTextBubble.SetVisible(false);
                    }
                    _map.Update(gameTime);
                    _map.UpdateNonMapStuff(gameTime);
                    _player.Update(gameTime, _map.Enemies, _map.Walls, ks, _lastKeyboardState, gs, _lastGamePadState);
                    _exit.Update(gameTime);
                    _gameUi.Update(gameTime, ks, _lastKeyboardState, gs, _lastGamePadState);

                    break;
                case GameState.Pause:
                    if (Globals.AnyKeyOrButtonNewlyPressed([Keys.P, Keys.Escape], [Buttons.Start, Buttons.Back, Buttons.B], ks, _lastKeyboardState, gs, _lastGamePadState))
                    {
                        UnPause(gameTime);
                    }
                    else
                    {
                        _pauseUi.Update(gameTime, ks, _lastKeyboardState, gs, _lastGamePadState);
                    }
                    break;
                case GameState.Menu:
                    _menuUi.Update(gameTime, ks, _lastKeyboardState, gs, _lastGamePadState);
                    break;
                case GameState.GameOver:
                    if (!_gameOverInitialized)
                    {
                        GameOver();
                        _gameOverInitialized = true;
                    }
                    _gameOverUi.Update(gameTime, ks, _lastKeyboardState, gs, _lastGamePadState);
                    break;
                case GameState.Dieing:
                    _map.Update(gameTime);
                    _player.Update(gameTime, _map.Enemies, _map.Walls, ks, _lastKeyboardState, gs, _lastGamePadState);
                    break;
                case GameState.Transitioning:
                    _transitionAnimation.Update(gameTime);
                    break;
                case GameState.Controls:
                    if (Globals.AnyKeyOrButtonNewlyPressed([Keys.Escape, Keys.Back, Keys.Enter, Keys.Space], [Buttons.B, Buttons.Back, Buttons.A], ks, _lastKeyboardState, gs, _lastGamePadState))
                    {
                        ControlsToMain();
                    };
                    _controlsUi.Update(gameTime, ks, _lastKeyboardState, gs, _lastGamePadState);
                    break;
                case GameState.Options:
                    _optionsUi.Update(gameTime, ks, _lastKeyboardState, gs, _lastGamePadState);
                    break;

                case GameState.PauseOptions:
                    if (Globals.AnyKeyOrButtonNewlyPressed([Keys.P], [Buttons.Start], ks, _lastKeyboardState, gs, _lastGamePadState))
                    {
                        UnPause(gameTime);
                    }
                    else
                    {
                        _options.Update(gameTime, ks, _lastKeyboardState, gs, _lastGamePadState);
                    }
                    break;
                default:
                    throw new NotSupportedException("wtf kinda state is this?");
            }

            base.Update(gameTime);
            UpdateCameraPosition();

            _lastKeyboardState = Keyboard.GetState();
            _lastGamePadState = GamePad.GetState(PlayerIndex.One);
        }

        protected override void Draw(GameTime gameTime)
        {
            //_penumbra.BeginDraw();

            base.Draw(gameTime);
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin(transformMatrix: _translation);
            switch (_stateManager.CurrentState)
            {
                case GameState.None:
                case GameState.Game:
                case GameState.Pause:
                    _map.Draw(gameTime);
                    _exit.Draw(gameTime);
                    _player.Draw(gameTime);
                    _map.DrawContentOverPlayer(gameTime);
                    _gameUi.Draw(gameTime, _translation.M41, _translation.M42);
                    if (_startTextBubble.IsVisible())
                    {
                        _startTextBubble.Draw(gameTime);
                    }

                    if (_stateManager.CurrentState == GameState.Pause)
                    {
                        // Additional drawing for the Pause state
                        _pauseUi.Draw(gameTime, _translation.M41, _translation.M42);
                    }
                    break;
                case GameState.PauseOptions:
                    _map.Draw(gameTime);
                    _exit.Draw(gameTime);
                    _player.Draw(gameTime);
                    if (_startTextBubble.IsVisible())
                    {
                        _startTextBubble.Draw(gameTime);
                    }
                    _map.DrawContentOverPlayer(gameTime);
                    _gameUi.Draw(gameTime, _translation.M41, _translation.M42);
                    _pauseUi.Draw(gameTime, _translation.M41, _translation.M42);
                    _options.Draw(gameTime, _translation.M41, _translation.M42);

                    break;
                case GameState.Menu:
                    _menuUi.Draw(gameTime, _translation.M41, _translation.M42);
                    break;
                case GameState.GameOver:
                    _gameOverUi.Draw(gameTime, _translation.M41, _translation.M42);
                    _player.Draw(gameTime);
                    break;
                case GameState.Dieing:
                    _player.Draw(gameTime);
                    break;
                case GameState.Transitioning:
                    _transitionAnimation.Draw(gameTime, _translation.M41, _translation.M42);
                    break;
                case GameState.Controls:
                    _controlsUi.Draw(gameTime, _translation.M41, _translation.M42);
                    break;
                case GameState.Options:
                    _optionsUi.Draw(gameTime, _translation.M41, _translation.M42);
                    break;
                default:
                    Console.WriteLine(_stateManager.CurrentState);
                    break;
            }
            // if (_debugUi.IsVisible())
            // {
            //     _debugUi.Draw(gameTime, _translation.M41, _translation.M42);
            // }
            _spriteBatch.End();

        }
        private void GameOver()
        {
            if (_score / 1000 > _options.GetState().GetHighScore())
            {
                _newHighScore = true;
                _options.GetState().SetHighScore(_score);
            }

            Spoilers.NewSpoiler();
            _run = 0;
        }
        private void PlayGameMusic()
        {
            _soundManager.StopAllSounds();
            _soundManager.PlaySound("main-loop", loop: true);
        }
    }
}