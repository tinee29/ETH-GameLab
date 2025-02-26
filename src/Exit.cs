using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLab
{

    public class Exit(SpriteBatch sb, Player player, Action<GameTime> nextLevelCallback) : IGameEntity
    {
        private const int EXIT_TEXTURE_DIM = 250;
        private const int ARROW_DIM = 75;

        private readonly SpriteBatch _sb = sb;
        private Texture2D _exitTexture;
        private readonly Player _player = player;
        private Vector2 _holePos;
        private readonly Action<GameTime> _nextLevelCallback = nextLevelCallback;
        private Rectangle _holeRectangle;
        private int _targetIndex;

        private Vector2 _arrowPos;
        private Texture2D _arrowTexture;
        private int _hoverState = 0;
        private Vector2 _hoverDist = new(0, 0.7f);

        //public void SetPos(Vector2 target)
        //{
        //    _holePos = target;
        //    _rectangle = new((int)_holePos.X - 10, (int)_holePos.Y - 10, EXIT_DIM + 10, EXIT_DIM + 10);
        //}
        public void SetTargetPosition(List<Block> map, int mapSize)
        {
            int i = 0;
            foreach (Block block in map)
            {
                if (block.IsTarget)
                {
                    _targetIndex = i;
                    break;
                }
                i++;
            }
            Vector2 pos = Globals.CalculateGlobalPosition(_targetIndex, mapSize);
            _holePos = new(pos.X - (EXIT_TEXTURE_DIM / 2), pos.Y - (EXIT_TEXTURE_DIM / 2));
            _holeRectangle = new((int)_holePos.X + 40, (int)_holePos.Y + 80, 160, 90); //dim of actual hole
            _arrowPos = new(pos.X - (ARROW_DIM / 2), pos.Y - ARROW_DIM);
        }
        public int GetTargetRoomIndex()
        {
            return _targetIndex;
        }
        public void Draw(GameTime gameTime)
        {
            _sb.Draw(_exitTexture, _holePos, Color.White);
            _sb.Draw(_arrowTexture, _arrowPos, Color.White);

        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public void LoadContent(ContentManager content)
        {
            _exitTexture = content.Load<Texture2D>("img/Exit");
            _arrowTexture = content.Load<Texture2D>("img/arrow");
        }

        public void Update(GameTime gameTime)
        {
            if (_holeRectangle.Intersects(_player.GetHitBox(_player.GetPos().X, _player.GetPos().Y)))
            {
                _nextLevelCallback(gameTime);
            }

            switch (_hoverState / 20)
            {
                case 0:
                    _arrowPos += _hoverDist;
                    _hoverState++;
                    break;
                case 1:
                    _arrowPos -= _hoverDist;
                    _hoverState++;
                    break;
                default:
                    _hoverState = 0;
                    break;
            }
        }

        public bool IsVisible()
        {
            return true;
        }

    }
}