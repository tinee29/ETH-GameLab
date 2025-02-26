using Microsoft.Xna.Framework;

namespace GameLab
{
    public interface IGameEntityStaticPos : IGameEntity
    {

        public void Draw(GameTime gameTime, float xOffset, float yOffset);


    }
}