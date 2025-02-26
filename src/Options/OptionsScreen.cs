using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameLab
{


    public enum Selection
    {
        Spoilers,
        Music,
        Effects,
        Back,

    }

    public static class SelectionMethods
    {

        private static readonly Dictionary<Selection, Selection> NextState = new() { {
            Selection.Spoilers, Selection.Music },
            { Selection.Music, Selection.Effects },
             { Selection.Effects, Selection.Back },
             {Selection.Back, Selection.Spoilers}};
        private static readonly Dictionary<Selection, Selection> PrevState = new() { {
            Selection.Spoilers, Selection.Back },
            { Selection.Music, Selection.Spoilers },
             { Selection.Effects, Selection.Music },
             {Selection.Back, Selection.Effects} };

        public static Selection Next(this Selection sel)
        {
            return NextState[sel];
        }

        public static Selection Prev(this Selection sel)
        {
            return PrevState[sel];
        }

    }

    public class OptionsScreen : IGameEntityStaticPos
    {

        public static readonly int ORIGINAL_W = 131;
        public static readonly int ORIGINAL_H = 132;
        private readonly SpriteBatch _sb;
        private readonly Slider _musicVolume;
        private readonly Slider _effectVolume;
        private readonly CheckBox _spoilersCheck;
        private Texture2D _texture;
        private Texture2D _arrowTexture;
        private readonly Action _closeOptions;

        private readonly int _x, _y, _w, _h;

        private Selection _current_selected = Selection.Spoilers;



        public OptionsScreen(SpriteBatch sb, OptionsState optionsState, int x, int y, int w, int h, Action closeOptions)
        {

            _sb = sb;

            double scaling_h = h / ORIGINAL_H;
            double scaling_w = w / ORIGINAL_W;
            if (Math.Abs(scaling_h - scaling_w) > 0.01)
            {
                throw new NotSupportedException("bad dimensions for options");
            }
            _x = x;
            _y = y;
            _w = w;
            _h = h;

            _closeOptions = closeOptions;


            _spoilersCheck = new(sb, x + 10, y + 35, 8, 7, optionsState.GetShowSpoilers(), optionsState.SetShowSpoilers, () => _current_selected = Selection.Spoilers);
            optionsState.SubscribeSpoilerChanges(_spoilersCheck.SetCurrent);
            _musicVolume = new(sb, x + 14, y + 69, 100, 11, optionsState.GetMusicVolume(), optionsState.SetMusicVolume, () => _current_selected = Selection.Music);
            optionsState.SubscribeMusicChanges(_musicVolume.SetCurrent);
            _effectVolume = new(sb, x + 14, y + 101, 100, 11, optionsState.GetEffectsVolume(), optionsState.SetEffectsVolume, () => _current_selected = Selection.Effects);
            optionsState.SubscribeEffectChanges(_effectVolume.SetCurrent);


        }
        public void Draw(GameTime gameTime, float xOffset, float yOffset)
        {
            //TODO: draw background
            _sb.Draw(_texture, new Rectangle((int)(_x - xOffset), (int)(_y - yOffset), _w, _h), Color.Beige);
            _musicVolume.Draw(gameTime, xOffset, yOffset);
            _effectVolume.Draw(gameTime, xOffset, yOffset);
            _spoilersCheck.Draw(gameTime, xOffset, yOffset);

            switch (_current_selected)
            {
                case Selection.Spoilers:
                    _sb.Draw(_arrowTexture, new Rectangle((int)(_x - xOffset) + 2, (int)(_y - yOffset) + 33, 3, 12), Color.Black);
                    break;
                case Selection.Music:
                    _sb.Draw(_arrowTexture, new Rectangle((int)(_x - xOffset) + 2, (int)(_y - yOffset) + 68, 3, 12), Color.Black);
                    break;
                case Selection.Effects:
                    _sb.Draw(_arrowTexture, new Rectangle((int)(_x - xOffset) + 2, (int)(_y - yOffset) + 100, 3, 12), Color.Black);
                    break;
                case Selection.Back:
                    _sb.Draw(_arrowTexture, new Rectangle((int)(_x - xOffset) + 2, (int)(_y - yOffset) + 115, 3, 12), Color.Black);
                    break;

                default:
                    break;

            }
        }

        public void Draw(GameTime gameTime)
        {
            throw new NotSupportedException("static ui element needs offset coordinates");
        }

        public void Initialize()
        {

        }

        public bool IsVisible()
        {
            return true;
        }

        public void LoadContent(ContentManager content)
        {

            //TODO: load options sheet
            _texture = content.Load<Texture2D>("img/options_menu");
            _arrowTexture = content.Load<Texture2D>("img/menuarrow");
            _musicVolume.LoadContent(content);
            _effectVolume.LoadContent(content);
            _spoilersCheck.LoadContent(content);
        }




        private void HandleInput(KeyboardState newks, KeyboardState oldks, GamePadState newgs, GamePadState oldgs)
        {
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
                        _spoilersCheck.Click();
                    }
                    break;
                case Selection.Music:
                case Selection.Effects:
                    Slider inUse = _current_selected == Selection.Music ? _musicVolume : _effectVolume;
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
                        _current_selected = Selection.Spoilers;
                        _closeOptions();
                    }
                    break;

                default:
                    break;
            }


        }

        public void Update(GameTime gameTime, KeyboardState newks, KeyboardState oldks, GamePadState newgs, GamePadState oldgs)
        {

            HandleInput(newks, oldks, newgs, oldgs);

            _musicVolume.Update(gameTime);
            _effectVolume.Update(gameTime);
            _spoilersCheck.Update(gameTime);
        }

        public void Update(GameTime gameTime)
        {
            throw new NotImplementedException();
        }
    }
}