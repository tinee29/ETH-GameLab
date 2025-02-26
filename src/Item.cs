using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLab
{


    public abstract class Item(SpriteBatch sb, Vector2 position) : IGameEntity
    {

        protected Vector2 _pos = position;
        public static readonly int ITEM_DIM = 32;

        protected SpriteBatch _sb = sb;
        public bool Dead;

        private int _hoverState = 0;
        private Vector2 _hoverDist = new(0, 0.7f);
        public Rectangle GetHitBox()
        {
            return new Rectangle((int)_pos.X, (int)_pos.Y, ITEM_DIM, ITEM_DIM);
        }
        public virtual void Draw(GameTime gameTime)
        {
        }

        public virtual void LoadContent(ContentManager content)
        {
        }

        public virtual void Update(GameTime gameTime, Player player)
        {
            Rectangle playerHitBox = player.GetHitBox(player.GetPos().X, player.GetPos().Y);
            float hitBoxDistance = Globals.GetDistanceBetweenRectangles(playerHitBox, GetHitBox());
            if (hitBoxDistance < 1)
            {
                PickUp(player, gameTime);
                Dead = true;
            }
            switch (_hoverState / 20)
            {
                case 0:
                    _pos += _hoverDist;
                    _hoverState++;
                    break;
                case 1:
                    _pos -= _hoverDist;
                    _hoverState++;
                    break;
                default:
                    _hoverState = 0;
                    break;
            }

        }
        public void Update(GameTime gameTime)
        {

        }
        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public abstract void PickUp(Player player, GameTime gameTime);

        public bool IsVisible()
        {
            return true;
        }

    }
}
