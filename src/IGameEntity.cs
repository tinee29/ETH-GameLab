using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace GameLab
{
    public interface IGameEntity
    {


        public void LoadContent(ContentManager content);
        public void Draw(GameTime gameTime);
        public void Update(GameTime gameTime);
        public void Initialize();

        public bool IsVisible();


    }
}