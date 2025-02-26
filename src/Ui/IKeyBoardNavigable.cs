using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GameLab
{


    public interface IKeyBoardNavigable
    {
        public void Update(GameTime gameTime, KeyboardState newks, KeyboardState oldks, GamePadState newgs, GamePadState oldgs);
    }
}