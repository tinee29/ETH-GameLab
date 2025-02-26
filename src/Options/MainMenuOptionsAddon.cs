using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameLab
{
    public class MainMenuOptionsAddon : IGameEntityStaticPos
    {
        private readonly Slider _volumeSlider;
        private readonly Slider _effectSlider;
        private readonly CheckBox _spoilerCheck;
        private readonly Action<GameTime> _exitCallback;
        private readonly SpriteBatch _sb;

        private KeyboardState _lastKs;
        private GamePadState _lastGs;

        private Texture2D _arrowTexture;

        private Selection _current_selected;

        public MainMenuOptionsAddon(Action<GameTime> exitCallback, OptionsState ostate, SpriteBatch sb)
        {
            _sb = sb;
            _exitCallback = exitCallback;
            _current_selected = Selection.Spoilers;

            CheckBox showSpoilersMain = new(sb, 394, 672, 30, 26, ostate.GetShowSpoilers(), ostate.SetShowSpoilers, () => _current_selected = Selection.Spoilers);
            ostate.SubscribeSpoilerChanges(showSpoilersMain.SetCurrent);
            _spoilerCheck = showSpoilersMain;

            Slider volSlider = new(sb, 405, 800, 368, 40, ostate.GetMusicVolume(), ostate.SetMusicVolume, () => _current_selected = Selection.Music);
            ostate.SubscribeMusicChanges(volSlider.SetCurrent);
            _volumeSlider = volSlider;

            Slider effSlider = new(sb, 405, 920, 368, 40, ostate.GetEffectsVolume(), ostate.SetEffectsVolume, () => _current_selected = Selection.Effects);
            ostate.SubscribeEffectChanges(effSlider.SetCurrent);
            _effectSlider = effSlider;
        }

        public void Draw(GameTime gameTime, float xOffset, float yOffset)
        {
            // _sb.Draw(_texture, new Rectangle(-(int)xOffset, -(int)yOffset, (int)(_spriteWidth * _scale), (int)(_scale * _spriteHeight)), new Rectangle(_currentIdx * _spriteWidth, 0, _spriteWidth, _spriteHeight), Color.White);
            // _sb.Draw(_texture, new Rectangle((int)-xOffset, (int)-yOffset, 1920, 1080), new Rectangle(1920 * 3, 0, 1920, 1080), Color.Black);
            _volumeSlider.Draw(gameTime, xOffset, yOffset);
            _effectSlider.Draw(gameTime, xOffset, yOffset);
            _spoilerCheck.Draw(gameTime, xOffset, yOffset);

            int x_for_arrow = 375;
            int arrow_w = 10;
            int arrow_h = 35;
            switch (_current_selected)
            {
                case Selection.Spoilers:
                    _sb.Draw(_arrowTexture, new Rectangle((int)-xOffset + x_for_arrow, (int)-yOffset + 668, arrow_w, arrow_h), Color.Black);
                    break;
                case Selection.Music:
                    _sb.Draw(_arrowTexture, new Rectangle((int)-xOffset + x_for_arrow, (int)-yOffset + 800, arrow_w, arrow_h), Color.Black);
                    break;
                case Selection.Effects:
                    _sb.Draw(_arrowTexture, new Rectangle((int)-xOffset + x_for_arrow, (int)-yOffset + 920, arrow_w, arrow_h), Color.Black);
                    break;
                case Selection.Back:
                    _sb.Draw(_arrowTexture, new Rectangle((int)-xOffset + x_for_arrow, (int)-yOffset + 980, arrow_w, arrow_h), Color.Black);
                    break;

                default:
                    break;

            }
        }

        public void Draw(GameTime gameTime)
        {
            throw new NotSupportedException();
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public bool IsVisible()
        {
            return true;
        }

        public void LoadContent(ContentManager content)
        {
            //TODO: load options sheet
            _arrowTexture = content.Load<Texture2D>("img/menuarrow");
            _volumeSlider.LoadContent(content);
            _effectSlider.LoadContent(content);
            _spoilerCheck.LoadContent(content);
        }

        private void CloseOptions(GameTime gameTime)
        {
            _current_selected = Selection.Spoilers;
            _exitCallback(gameTime);
        }
        private void HandleInput(GameTime gameTime, KeyboardState newks, KeyboardState oldks, GamePadState newgs, GamePadState oldgs)
        {

            if (Globals.AnyKeyOrButtonPressed([Keys.Escape, Keys.Back], [Buttons.B, Buttons.Back], newks, newgs))
            {
                _current_selected = Selection.Spoilers;
                CloseOptions(gameTime);

            }

            if (Globals.AnyKeyOrButtonNewlyPressed([Keys.Up], [Buttons.DPadUp, Buttons.LeftThumbstickUp, Buttons.RightThumbstickUp], newks, oldks, newgs, oldgs))
            {
                _current_selected = _current_selected.Prev();
            }
            else if (Globals.AnyKeyOrButtonNewlyPressed([Keys.Down], [Buttons.DPadDown, Buttons.LeftThumbstickDown, Buttons.RightThumbstickDown], newks, oldks, newgs, oldgs))
            {
                _current_selected = _current_selected.Next();
            }

            switch (_current_selected)
            {
                case Selection.Spoilers:
                    if (Globals.AnyKeyOrButtonNewlyPressed([Keys.Enter, Keys.Space], [Buttons.A], newks, oldks, newgs, oldgs))
                    {
                        _spoilerCheck.Click();
                    }
                    break;
                case Selection.Music:
                case Selection.Effects:
                    Slider inUse = _current_selected == Selection.Music ? _volumeSlider : _effectSlider;
                    if (Globals.AnyKeyOrButtonPressed([Keys.Left], [Buttons.DPadLeft, Buttons.RightThumbstickLeft, Buttons.LeftThumbstickLeft], newks, newgs))
                    {
                        inUse.StepDown();
                    }
                    else if (Globals.AnyKeyOrButtonPressed([Keys.Right], [Buttons.DPadRight, Buttons.RightThumbstickRight, Buttons.LeftThumbstickRight], newks, newgs))
                    {
                        inUse.StepUp();
                    }
                    break;
                case Selection.Back:
                    if (Globals.AnyKeyOrButtonNewlyPressed([Keys.Enter, Keys.Space], [Buttons.A], newks, oldks, newgs, oldgs))
                    {
                        CloseOptions(gameTime);
                    }
                    break;

                default:
                    break;
            }


        }

        public void Update(GameTime gameTime, KeyboardState newks, KeyboardState oldks, GamePadState newgs, GamePadState oldgs)
        {

            HandleInput(gameTime, newks, oldks, newgs, oldgs);

            _volumeSlider.Update(gameTime);
            _effectSlider.Update(gameTime);
            _spoilerCheck.Update(gameTime);
        }


        public void Update(GameTime gameTime)
        {


            KeyboardState newks = Keyboard.GetState();
            GamePadState newgs = GamePad.GetState(PlayerIndex.One);
            Update(gameTime, newks, _lastKs, newgs, _lastGs);
            _lastKs = newks;
            _lastGs = newgs;
        }
    }
}