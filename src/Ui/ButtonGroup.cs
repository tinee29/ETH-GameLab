using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameLab
{
    public enum Direction
    {
        Horizontal,
        Vertical,
        Diagonal,
    }
    public class ButtonGroup(int x, int y, Color foreground, Color background, Direction direction, SpriteBatch sb) : IGameEntityStaticPos, IKeyBoardNavigable
    {

        public readonly int SPACING = 5;

        private int _nextX = x;
        private int _nextY = y;
        private readonly Color _fg = foreground;
        private readonly Color _bg = background;
        private readonly Direction _dir = direction;

        private readonly List<Button> _buttons = [];
        private int _selectedButtonIdx = 0;

        private readonly SpriteBatch _sb = sb;

        private readonly Ui _ui = new(sb);


        public void AddButton(string text, int w, int h, Action<GameTime> callback)
        {
            Button newButton = new(_sb, _fg, _bg, _nextX, _nextY, w, h, text, callback);
            if (_dir == Direction.Horizontal)
            {
                _nextX += w + SPACING;
            }
            else if (_dir == Direction.Vertical)
            {
                _nextY += h + SPACING;
            }
            else if (_dir == Direction.Diagonal)
            {
                _nextY += h + SPACING;
                _nextX += (int)(w * 0.75) + SPACING;

            }
            _buttons.Add(newButton);
            _ui.AddElement(newButton);

        }







        public void Draw(GameTime gameTime, float xOffset, float yOffset)
        {
            _ui.Draw(gameTime, xOffset, yOffset);
        }

        public void Draw(GameTime gameTime)
        {
            throw new NotSupportedException();
        }

        public void Initialize()
        {
            _ui.Initialize();
        }

        public void LoadContent(ContentManager content)
        {
            _ui.LoadContent(content);
        }

        public void Update(GameTime gameTime)
        {
            _ui.Update(gameTime);
        }

        public bool IsVisible()
        {
            return true;
        }

        public void HandleInput(GameTime gameTime, KeyboardState newks, KeyboardState oldks, GamePadState newgs, GamePadState oldgs)
        {


            bool enterKb = newks.IsKeyDown(Keys.Enter) && oldks.IsKeyUp(Keys.Enter);
            bool enterGp = newgs.IsButtonDown(Buttons.A) && oldks.IsKeyUp(Keys.A);
            if (enterKb || enterGp)
            {

                if (_buttons.Count > 0)
                {
                    _buttons[_selectedButtonIdx].Click(gameTime);
                }
            }



            bool goDownKb = newks.IsKeyDown(Keys.S) && oldks.IsKeyUp(Keys.S);
            bool goUpKb = newks.IsKeyDown(Keys.W) && oldks.IsKeyUp(Keys.W);
            bool goDownGp = newgs.IsButtonDown(Buttons.DPadDown) && oldgs.IsButtonUp(Buttons.DPadDown);
            bool goUpGp = newgs.IsButtonDown(Buttons.DPadUp) && oldgs.IsButtonUp(Buttons.DPadUp);

            int indx_mod = 0;
            if (goDownGp || goDownKb)
            {
                indx_mod++;
            }
            if (goUpGp || goUpKb)
            {
                indx_mod--;
            }

            if (_buttons.Count > 1)
            {
                if (indx_mod != 0)
                {

                    _buttons[_selectedButtonIdx].SetSelected(false);


                    _selectedButtonIdx += indx_mod;
                    _selectedButtonIdx %= _buttons.Count;
                    if (_selectedButtonIdx < 0)
                    {
                        _selectedButtonIdx += _buttons.Count;
                    }
                }
                _buttons[_selectedButtonIdx].SetSelected(true);
            }
            else
            {
                _selectedButtonIdx = 0;
            }

        }
        public void Update(GameTime gameTime, KeyboardState newks, KeyboardState oldks, GamePadState newgs, GamePadState oldgs)
        {
            HandleInput(gameTime, newks, oldks, newgs, oldgs);
            _ui.Update(gameTime, newks, oldks, newgs, oldgs);
        }

    }
}