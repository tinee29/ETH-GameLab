using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameLab
{

    public enum Fonts
    {
        Default,
        Title,
        TextBox,


    }
    public class Ui(SpriteBatch sb) : IGameEntityStaticPos, IKeyBoardNavigable
    {

        private readonly List<IGameEntity> _elements = [];
        private readonly SpriteBatch _sb = sb;
        private bool _isVisible = true;
        private readonly Dictionary<string, IGameEntity> _namedElements = [];

        public void AddElement(IGameEntity element)
        {
            _elements.Add(element);
        }

        public void AddNamedElement(string name, IGameEntity element)
        {
            AddElement(element);
            _namedElements[name] = element;
        }
        public Type GetNamedElement<Type>(string name)
        {
            if (!_namedElements.ContainsKey(name))
            {
                Console.WriteLine("There is no named element for the name {0}", name);
            }
            return (Type)_namedElements[name];
        }





        public void Draw(GameTime gameTime, float xOffset, float yOffset)
        {

            void Call(IGameEntity e)
            {
                if (!e.IsVisible())
                {
                    return;
                }

                if (e is IGameEntityStaticPos staticElement)
                {
                    staticElement.Draw(gameTime, xOffset, yOffset);
                }
                else
                {
                    e.Draw(gameTime);
                }
            }
            CallForEachElement(Call);
        }

        public void Draw(GameTime gameTime)
        {
            throw new NotSupportedException();
        }


        public void Initialize()
        {
            static void Call(IGameEntity e)
            {
                e.Initialize();
            }
            CallForEachElement(Call);
        }

        public void LoadContent(ContentManager content)
        {
            void Call(IGameEntity e)
            {
                e.LoadContent(content);
            }
            CallForEachElement(Call);
        }

        public void Update(GameTime gameTime, KeyboardState newks, KeyboardState oldks, GamePadState newgs, GamePadState oldgs)
        {

            if (!_isVisible)
            {
                return;
            }
            void Call(IGameEntity e)
            {
                if (e is IKeyBoardNavigable kn)
                {
                    kn.Update(gameTime, newks, oldks, newgs, oldgs);
                }
                else
                {
                    e.Update(gameTime);
                }
            }


            CallForEachElement(Call);

        }

        public void AddText(string text, int x, int y, Fonts font, Color color)
        {
            UiText newText = new(x, y, font, _sb, color, text);
            AddElement(newText);
        }

        public void AddTextWithGenerator(Func<string> generator, int x, int y, Fonts font, Color color)
        {
            UiText newText = new(x, y, font, _sb, color, "", generator);
            AddElement(newText);
        }


        private void CallForEachElement(Action<IGameEntity> method)
        {
            foreach (IGameEntity element in _elements)
            {
                method(element);
            }

        }

        public bool IsVisible()
        {
            return _isVisible;
        }
        public void Hide()
        {
            _isVisible = false;
        }
        public void Show()
        {
            _isVisible = true;
        }
        public void SetVisible(bool b)
        {
            _isVisible = b;
        }

        public void ToggleVisibility()
        {
            _isVisible = !_isVisible;
        }
        public void Update(GameTime gameTime)
        {
            throw new NotSupportedException();
        }
    }

}

