using System;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameLab
{
    public class OptionsState(int music, int effect, bool showSpoilers, int highScore)
    {
        private int _musicVolume = music;
        private int _effectVolume = effect;
        private bool _showSpoilers = showSpoilers;

        private int _highScore = highScore;

        private event Action<bool> SpoilersUpdate;
        private event Action<int> MusicUpdate;
        private event Action<int> EffectUpdate;


        public static OptionsState ReadConfig(string fpath)
        {
            int music = 50;
            int fx = 50;
            bool spoilers = true;
            int highScore = 0;

            try
            {

                using StreamReader sr = new(fpath);
                music = int.Parse(sr.ReadLine());
                fx = int.Parse(sr.ReadLine());
                spoilers = bool.Parse(sr.ReadLine());
                try
                {

                    highScore = int.Parse(sr.ReadLine());
                }
                catch (ArgumentNullException)
                {
                    highScore = 0;
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("couldnt read :/");
                Console.WriteLine(e.Message);
            }


            return new OptionsState(music, fx, spoilers, highScore);

        }


        public void WriteConfig(string fpath)
        {
            try
            {
                using StreamWriter sr = new(fpath);
                sr.WriteLine(_musicVolume);
                sr.WriteLine(_effectVolume);
                sr.WriteLine(_showSpoilers);
                sr.WriteLine(_highScore);
            }
            catch (IOException e)
            {
                Console.WriteLine("couldnt write :/");
                Console.WriteLine(e.Message);
            }
        }

        public int GetMusicVolume()
        {
            return _musicVolume;
        }
        public void SetMusicVolume(int vol)
        {
            _musicVolume = vol;
            MusicUpdate?.Invoke(vol);
        }

        public int GetEffectsVolume()
        {
            return _effectVolume;
        }
        public void SetEffectsVolume(int vol)
        {
            _effectVolume = vol;
            EffectUpdate?.Invoke(vol);
        }

        public bool GetShowSpoilers()
        {
            return _showSpoilers;
        }

        public void SubscribeSpoilerChanges(Action<bool> f)
        {
            SpoilersUpdate += f;
        }

        public void SubscribeMusicChanges(Action<int> f)
        {
            MusicUpdate += f;
        }

        public void SubscribeEffectChanges(Action<int> f)
        {
            EffectUpdate += f;
        }

        public void SetShowSpoilers(bool showSpoilers)
        {
            _showSpoilers = showSpoilers;
            SpoilersUpdate?.Invoke(showSpoilers);
        }

        public int GetHighScore()
        {
            return _highScore;
        }

        public void SetHighScore(int newscore)
        {
            _highScore = newscore / 1000;
        }


    }
    public class Options
    {

        private readonly OptionsScreen _display;
        private readonly OptionsState _state;
        private readonly string _configPath;

        private readonly GameStateManager _gsm;
        private readonly SoundManager _sm;

        public OptionsState GetState()
        {
            return _state;
        }

        private void SoundMusicWrapper(int val)
        {
            float actualVal = val / 100f;
            // Console.WriteLine("trying to set volume for main-loop to {0}", actualVal);
            _sm.SetVolume("main-loop", actualVal);
            _sm.SetVolume("menu-loop", actualVal);
            _sm.SetVolume("game-over", actualVal);
        }
        private void SoundEffectWrapper(int val)
        {
            float actualVal = val / 100f;

            _sm.SetVolume("banana-death", actualVal);
            _sm.SetVolume("player-step", actualVal);
            _sm.SetVolume("player-kick", actualVal);
            _sm.SetVolume("player-hit", actualVal);
            _sm.SetVolume("player-dodge", actualVal);
            _sm.SetVolume("mushroom-attack", actualVal);
            _sm.SetVolume("banana-attack", actualVal);
            _sm.SetVolume("player-slide", actualVal);
        }

        public Options(SoundManager sm, GameStateManager gsm, OptionsState state, SpriteBatch sb, int x, int y, string configPath) : this(sm, gsm, state, sb, x, y, OptionsScreen.ORIGINAL_W, OptionsScreen.ORIGINAL_H, configPath) { }


        public Options(SoundManager sm, GameStateManager gsm, OptionsState state, SpriteBatch sb, int x, int y, int w, int h, string configPath)
        {
            _state = state;
            _configPath = configPath;
            _display = new(sb, _state, x, y, w, h, CloseOptions);
            _gsm = gsm;
            _sm = sm;
            _state.SubscribeEffectChanges(SoundEffectWrapper);
            _state.SubscribeMusicChanges(SoundMusicWrapper);
        }

        public void SaveState()
        {
            _state.WriteConfig(_configPath);
        }

        public void Draw(GameTime gameTime, float xOffset, float yOffset)
        {

            _display.Draw(gameTime, xOffset, yOffset);

        }
        public void LoadContent(ContentManager content)
        {
            _display.LoadContent(content);
        }

        private void CloseOptions()
        {
            _gsm.ChangeState(GameState.Pause);
        }

        public void Update(GameTime gameTime, KeyboardState newks, KeyboardState oldks, GamePadState newgs, GamePadState oldgs)
        {

            if (Globals.AnyKeyOrButtonNewlyPressed([Keys.Escape, Keys.Back], [Buttons.B, Buttons.Back], newks, oldks, newgs, oldgs))
            {
                CloseOptions();
            }
            _display.Update(gameTime, newks, oldks, newgs, oldgs);
        }
    }
}