using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
namespace GameLab
{
    public class EnemySpawns(Vector2 position, Map map, ContentManager content, Player player) : IGameEntity
    {
        private Vector2 _position = position;
        private readonly Map _map = map;
        private bool _spawning = true;
        private static readonly Random Random = new();
        private readonly int _spawnCD = 5000;
        private readonly ContentManager _content = content;
        private int _enemyType = (Random.Next() % 4) + 1;

        private int _spawn_timer = -5001; //HACK: for instant spawn at start of the game...
        //slightly larger than viewing window
        private const int DistHorizontal = 1100;
        public const int DistVertical = 700;

        private bool _canSpawn = true;

        public void AllowNewSpawns(GameTime gameTime)
        {
            _spawning = true;
            _spawn_timer = (int)gameTime.TotalGameTime.TotalMilliseconds;

        }


        public void Update(GameTime gameTime)
        {
            _canSpawn = _spawnCD - ((int)gameTime.TotalGameTime.TotalMilliseconds - _spawn_timer) < 0
                && (Math.Abs(player.GetPos().X - _position.X) > DistHorizontal || Math.Abs(player.GetPos().Y - _position.Y) > DistVertical);
            if (_spawning & _canSpawn)
            {
                SpawnEnemy();
                _spawning = false;
                _spawn_timer = (int)gameTime.TotalGameTime.TotalMilliseconds;
            }
        }
        public void Draw(GameTime gameTime)
        {
            throw new NotImplementedException();
        }
        private void SpawnEnemy()
        {
            _map.AddEnemy(_position, _enemyType, this, _content);
            _enemyType = (Random.Next() % 4) + 1;
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public void LoadContent(ContentManager content)
        {
            throw new NotImplementedException();
        }

        public bool IsVisible()
        {
            return false; //XXX right?
        }
    }
}