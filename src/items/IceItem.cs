
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLab
{
    public class IceItem(SpriteBatch sb, Vector2 position) : Item(sb, position)
    {
        private Texture2D _enemyTexture;

        public override void Draw(GameTime gameTime)
        {

            _sb.Draw(
                    _enemyTexture,
                    new Rectangle((int)_pos.X, (int)_pos.Y, 64, 64), //destr
                    new Rectangle(0, 0, ITEM_DIM, ITEM_DIM), //source
                    Color.White, 0F, Vector2.Zero, SpriteEffects.None, 0
                    );
        }

        public override void LoadContent(ContentManager content)
        {
            _enemyTexture = content.Load<Texture2D>("img/Ice_cube_border");
        }

        public override void PickUp(Player player, GameTime gameTime)
        {
            player.GetInvulnerable(gameTime);
        }

    }
}