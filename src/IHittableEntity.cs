using Microsoft.Xna.Framework;

namespace GameLab
{
    public interface IHittableEntity : IGameEntity
    {
        public void GetHit(GameTime gameTime);
    }
}