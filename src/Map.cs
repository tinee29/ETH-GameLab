using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Vector2 = Microsoft.Xna.Framework.Vector2;




namespace GameLab
{

    public class Map(SpriteBatch sb, ContentManager content, Timer timer, Player player, Exit exit, SoundManager soundManager) : IGameEntity
    {
        private Texture2D _topSheet;
        private Texture2D _bottomSheet;
        private Texture2D _leftSheet;
        private Texture2D _rightSheet;
        private Texture2D _corridorSheet1;
        private Texture2D _corridorSheet2;
        private Texture2D _obstacleSheet;
        private Texture2D _floorSheet;

        private Texture2D _topWallSheet;
        private Texture2D _leftWallSheet;
        private Texture2D _rightWallSheet;
        private Texture2D _topLeftCorner;
        private Texture2D _bottomLeftCorner;
        private Texture2D _topRightCorner;
        private Texture2D _bottomRightCorner;

        private RoomData _roomData;
        private RoomData _hallData;
        private RoomData _corridorData1;
        private RoomData _corridorData2;
        private ObstacleContainer _obstacleData;
        private Dictionary<int, List<Obstacle>> _obstacles;
        private Dictionary<int, List<Vector2>> _spawnPoints;
        private Dictionary<int, List<Item>> _itemsDict = [];
        public List<EnemySpawns> SpawnPoints = [];
        public List<Enemy> Enemies = [];
        public List<Projectile> Projectiles = [];
        public List<Block> Blocks;
        public List<Block> AllBlocks;
        public List<Rectangle> Walls;
        public List<Obstacle> Obstacles;
        public const int BLOCKSIZE = 1000;
        public int MapSize = 3;
        public int MinSpawns = 1;
        public int MaxSpawns = 3;
        public int ItemsCount = 1;





        private readonly SpriteBatch _sb = sb;
        private readonly Timer _timer = timer;
        private readonly Player _player = player;
        private readonly Exit _exit = exit;
        private readonly SoundManager _soundManager = soundManager;


        public void Draw(GameTime gameTime)
        {
            _sb.Draw(_topLeftCorner, MapHelpers.CalculateBlockPosition(0, MapSize) - new Vector2(100, 100), Color.White);
            _sb.Draw(_topRightCorner, MapHelpers.CalculateBlockPosition(MapSize - 1, MapSize) - new Vector2(0, 100), Color.White);

            for (int i = 0; i < MapSize; i++)
            {
                _sb.Draw(_topWallSheet, MapHelpers.CalculateBlockPosition(i, MapSize) - new Vector2(0, 100), Color.White);
            }

            for (int i = 0; i < Blocks.Count; i++)
            {
                Block block = Blocks[i];
                // Calculate grid position
                Vector2 position = MapHelpers.CalculateBlockPosition(i, MapSize);

                Rectangle floorSource = new(i % MapSize % 2 * BLOCKSIZE / 2, i / MapSize % 2 * BLOCKSIZE / 2, BLOCKSIZE / 2, BLOCKSIZE / 2);
                Rectangle floorTarget = new((int)position.X, (int)position.Y, BLOCKSIZE, BLOCKSIZE);
                _sb.Draw(_floorSheet, floorTarget, floorSource, Color.White);

                if (block.Room.IsCorridor())
                {
                    Vector2 origin = new(block.SourceRectangle.Width / 2f, block.SourceRectangle.Height / 2f);
                    //Since we changed the origin to rotate the block, we need to correct the position
                    Vector2 correctedPosition = new(position.X + 500, position.Y + 500);
                    SpriteEffects effect = block.IsMirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                    _sb.Draw(
                        texture: block.CorridorTexture,
                        position: correctedPosition,
                        sourceRectangle: block.SourceRectangle,
                        color: Color.White,
                        rotation: 0,
                        origin: origin,
                        scale: new Vector2(1, 1),
                        effects: effect,
                        layerDepth: 0f
                    );
                }
                else
                {
                    Vector2 topPosition = position + new Vector2(0, -20);
                    Vector2 bottomPosition = position + new Vector2(0, 900);
                    Vector2 leftPosition = position + new Vector2(0, 80);
                    Vector2 rightPosition = position + new Vector2(900, 80);
                    _sb.Draw(
                        texture: _topSheet,
                        position: topPosition,
                        sourceRectangle: block.TopRectangle,
                        color: Color.White
                    );
                    _sb.Draw(
                        texture: _leftSheet,
                        position: leftPosition,
                        sourceRectangle: block.LeftRectangle,
                        color: Color.White
                    );
                    _sb.Draw(
                        texture: _rightSheet,
                        position: rightPosition,
                        sourceRectangle: block.RightRectangle,
                        color: Color.White
                    );
                    _sb.Draw(
                        texture: _bottomSheet,
                        position: bottomPosition,
                        sourceRectangle: block.BottomRectangle,
                        color: Color.White
                    );
                }


                if (_obstacles.TryGetValue(i, out List<Obstacle> value))
                {
                    foreach (Obstacle currentObstacle in value)
                    {
                        Vector2 obstaclePos = new(currentObstacle.PositionBottom.X, currentObstacle.PositionBottom.Y);
                        _sb.Draw(
                            texture: _obstacleSheet,
                            position: obstaclePos,
                            sourceRectangle: currentObstacle.SourceRectangleBottom,
                            color: Color.White
                        );
                    }
                }

            }

            for (int i = 0; i < MapSize; i++)
            {
                _sb.Draw(_leftWallSheet, MapHelpers.CalculateBlockPosition(i * MapSize, MapSize) - new Vector2(100, 0), Color.White);
                _sb.Draw(_rightWallSheet, MapHelpers.CalculateBlockPosition((i * MapSize) + MapSize - 1, MapSize) + new Vector2(1000, 0), Color.White);
            }

            for (int i = MapSize * (MapSize - 1); i < (MapSize * MapSize) - 1; i++)
            {
                _sb.Draw(_topWallSheet, MapHelpers.CalculateBlockPosition(i, MapSize) + new Vector2(0, 1000), Color.White);
            }

            _sb.Draw(_bottomLeftCorner, MapHelpers.CalculateBlockPosition(MapSize * (MapSize - 1), MapSize) - new Vector2(100, -1000), Color.White);
            _sb.Draw(_bottomRightCorner, MapHelpers.CalculateBlockPosition((MapSize * MapSize) - 1, MapSize) + new Vector2(0, 1000), Color.White);


            if (_itemsDict != null)
            {
                foreach (KeyValuePair<int, List<Item>> itemPair in _itemsDict)
                {
                    foreach (Item item in itemPair.Value)
                    {
                        item.Draw(gameTime);
                    }
                }
            }

            // //DRAW WALLS FOR DEBUGGING
            // Color wallColor = Color.Red;

            // // Draw each wall
            // foreach (Rectangle wall in Walls)
            // {
            //     _sb.Draw(_pixel, wall, wallColor);
            // }
            //DrawSingleRoom(gameTime, 296);
        }

        public void DrawContentOverPlayer(GameTime gameTime)
        {
            foreach (Enemy enemy in Enemies)
            {
                enemy.Draw(gameTime);
            }
            foreach (Projectile projectile in Projectiles)
            {
                projectile.Draw(gameTime);
            }
            for (int i = 0; i < Blocks.Count; i++)
            {
                if (_obstacles.TryGetValue(i, out List<Obstacle> value))
                {
                    foreach (Obstacle currentObstacle in value)
                    {
                        Vector2 obstaclePos = new(currentObstacle.PositionTop.X, currentObstacle.PositionTop.Y);
                        _sb.Draw(
                            texture: _obstacleSheet,
                            position: obstaclePos,
                            sourceRectangle: currentObstacle.SourceRectangleTop,
                            color: Color.White
                        );
                    }
                }
            }
        }

        public void UpdateNonMapStuff(GameTime gameTime)
        {
            foreach (EnemySpawns spaawn in SpawnPoints)
            {
                spaawn.Update(gameTime);
            }
            for (int i = 0; i < Projectiles.Count; i++)
            {
                Projectile proj = Projectiles[i];
                proj.Update(gameTime, _player, Walls);
                if (proj.Dead)
                {
                    _ = Projectiles.Remove(proj);
                    i--;
                }
            }
            for (int i = 0; i < Enemies.Count; i++)
            {
                Enemy enemy = Enemies[i];
                enemy.Update(gameTime, _player, Walls);
                if (enemy.Dead)
                {
                    _ = Enemies.Remove(enemy);
                    i--;
                }
            }
        }
        public void LoadContent()
        {
            // Load textures for rooms and corridors
            LoadTextures(content);
            // Load data for rooms and corridors
            LoadData();
            // Load blocks for rooms and corridors, applying rotations as needed
            (AllBlocks, Obstacles) = LoadBlocksAndWalls();

            //_pixel = new Texture2D(graphicsDevice, 1, 1);
            //_pixel.SetData(new[] { Color.White }); // Set the pixel to white
        }
        public void Update(GameTime gameTime)
        {
            if (_itemsDict != null)
            {
                foreach (KeyValuePair<int, List<Item>> blockItemsPair in _itemsDict)
                {
                    List<Item> itemsInBlock = blockItemsPair.Value;
                    _ = itemsInBlock.RemoveAll(item =>
                    {
                        item.Update(gameTime, _player); // Update first, then check status
                        return item.Dead; // Remove if dead
                    });
                }
            }
        }
        public void LoadTextures(ContentManager content)
        {
            _floorSheet = content.Load<Texture2D>("img/floor");
            _topSheet = content.Load<Texture2D>("img/top-walls");
            _bottomSheet = content.Load<Texture2D>("img/bottom-walls");
            _leftSheet = content.Load<Texture2D>("img/left-walls");
            _rightSheet = content.Load<Texture2D>("img/right-walls");
            _corridorSheet1 = content.Load<Texture2D>("img/corridor-sheet1");
            _corridorSheet2 = content.Load<Texture2D>("img/corridor-sheet2");
            _obstacleSheet = content.Load<Texture2D>("img/obstacle-sheet");

            _topWallSheet = content.Load<Texture2D>("img/top-ext-wall");
            _leftWallSheet = content.Load<Texture2D>("img/left-ext-wall");
            _rightWallSheet = content.Load<Texture2D>("img/right-ext-wall");
            _topLeftCorner = content.Load<Texture2D>("img/topleft-ext-wall");
            _topRightCorner = content.Load<Texture2D>("img/topright-ext-wall");
            _bottomRightCorner = content.Load<Texture2D>("img/bottomright-ext-wall");
            _bottomLeftCorner = content.Load<Texture2D>("img/bottomleft-ext-wall");
        }
        public void LoadData()
        {
            _roomData = RoomData.LoadRoomData("rooms.json");
            _hallData = RoomData.LoadRoomData("halls.json");
            _corridorData1 = RoomData.LoadRoomData("corridors1.json");
            _corridorData2 = RoomData.LoadRoomData("corridors2.json");
            _obstacleData = ObstacleContainer.LoadObstacleData("obstacles.json");
        }
        public (List<Block>, List<Obstacle>) LoadBlocksAndWalls()
        {
            List<Block> roomBlocks = MapHelpers.LoadRoomBlocks(_roomData);
            List<Block> hallBlocks = MapHelpers.LoadRoomBlocks(_hallData);
            List<Block> corridorBlocks1 = MapHelpers.LoadCorridorBlocks(_corridorSheet1, _corridorData1);
            List<Block> corridorBlocks2 = MapHelpers.LoadCorridorBlocks(_corridorSheet2, _corridorData2);
            List<Obstacle> obstacles = MapHelpers.LoadObstacles(_obstacleSheet, _obstacleData);

            List<Block> allRooms = new(roomBlocks);
            allRooms.AddRange(hallBlocks);

            List<Block> allCorridors = new(corridorBlocks1);
            allCorridors.AddRange(corridorBlocks2);

            MapHelpers.ComputeRoomBordersAndDoors(allRooms);
            MapHelpers.ComputeCorridorBordersAndDoors(allCorridors);
            allRooms.AddRange(allCorridors);
            return (allRooms, obstacles);
        }
        public void GenerateMap()
        {
            Blocks = MapGenerator.GenerateMap(AllBlocks, MapSize, MapSize);
            _player.SetPlayerStartPosition(Blocks, MapSize);
            _exit.SetTargetPosition(Blocks, MapSize);
            GenerateItems(Blocks, MapSize, _sb);
            (_obstacles, _spawnPoints) = MapHelpers.GenerateObstaclesAndSpawners(Blocks, Obstacles, 1, 3, MinSpawns, MaxSpawns, new Random(), _player.GetStartingRoomIndex(), _exit.GetTargetRoomIndex(), _itemsDict);
            CreateEnemySpawners(content);
            Walls = MapHelpers.GetCollisionRectangles(Blocks, _obstacles, MapSize);
            foreach (Enemy enemy in Enemies)
            {
                enemy.LoadContent(content);
            }
            foreach (Projectile projectile in Projectiles)
            {
                projectile.LoadContent(content);
            }
        }
        public void GenerateItems(List<Block> map, int mapSize, SpriteBatch sb)
        {
            _itemsDict = MapHelpers.PopulateItems(map, mapSize, sb, _player.GetStartingRoomIndex(), _exit.GetTargetRoomIndex());
            //check if not null since in first level we have no items
            if (_itemsDict != null)
            {
                foreach (KeyValuePair<int, List<Item>> blockItems in _itemsDict)
                {
                    foreach (Item item in blockItems.Value)
                    {
                        item.LoadContent(content);
                    }
                }
            }
        }


        public void AddEnemy(Vector2 position, int enemyType, EnemySpawns enSpawn, ContentManager content)
        {
            Enemy en = new BananaEnemy(_sb, position, _timer.AddTimeMs, enSpawn, _soundManager);
            switch (enemyType)
            {
                case 1:
                    en = new MushroomEnemy(_sb, position, _timer.AddTimeMs, enSpawn, _soundManager);
                    break;
                case 2:
                    en = new WatermelonEnemy(_sb, position, _timer.AddTimeMs, enSpawn, _soundManager);
                    break;
                case 3:
                    en = new BananaEnemy(_sb, position, _timer.AddTimeMs, enSpawn, _soundManager);
                    break;
                case 4:
                    en = new TomatoEnemy(_sb, position, _timer.AddTimeMs, enSpawn, this, _soundManager);
                    break;
                default:
                    break;
            }
            en.LoadContent(content);
            Enemies.Add(en);
        }

        public void AddProjectile(Vector2 position, Vector2 direction)
        {
            Projectile proj = new(_sb, position, direction);
            proj.LoadContent(content);
            Projectiles.Add(proj);
        }
        private void CreateEnemySpawners(ContentManager content)
        {
            foreach (KeyValuePair<int, List<Vector2>> kvp in _spawnPoints)
            {
                int blockIndex = kvp.Key;
                foreach (Vector2 spawnPoint in kvp.Value)
                {
                    // Create enemy spawner which automatically spawns enemy
                    EnemySpawns spawn = new(spawnPoint, this, content, _player);
                    SpawnPoints.Add(spawn);
                }
            }
        }
        public void ConfigureNewLevel(int mapSize, int minSpawns, int maxSpawns, int itemsCount)
        {
            MapSize = mapSize;
            MinSpawns = minSpawns;
            MaxSpawns = maxSpawns;
            ItemsCount = itemsCount;
            Enemies?.Clear();
        }
        public void Reset(int mapSize)
        {
            MapSize = mapSize;
            MinSpawns = 1;
            MaxSpawns = 3;
            ItemsCount = MapSize;
            _spawnPoints?.Clear();
            _obstacles?.Clear();
            Enemies?.Clear();
        }
        /*
        private void DrawSingleRoom(GameTime gameTime, int roomIndex)

        {
            Block block = AllBlocks[roomIndex];
            Vector2 position = new(0, 0);
            float rotationRadians = MathHelper.ToRadians(block.RotationIndex * 90);
            Vector2 origin = new(block.SourceRectangle.Width / 2f, block.SourceRectangle.Height / 2f);
            Vector2 correctedPosition = new(position.X + (BLOCKSIZE / 2), position.Y + (BLOCKSIZE / 2));
            // Draw the selected room
            _sb.Draw(
                texture: block.Texture,
                position: correctedPosition,
                sourceRectangle: block.SourceRectangle,
                color: Color.White,
                rotation: rotationRadians,
                origin: origin,
                scale: new Vector2(2, 2),
                effects: SpriteEffects.None,
                layerDepth: 0f
            );

            // // Optionally, draw the walls of the selected room for debugging
            // foreach (Rectangle wall in block.Room.WallBorders)
            // {
            //     Rectangle screenRect = new(
            //         wall.X + (int)position.X,
            //         wall.Y + (int)position.Y,
            //         wall.Width,
            //         wall.Height);

            //     _sb.Draw(_pixel, screenRect, Color.Red); // Using Magenta for clarity
            // }
        }
        */

        public bool IsVisible()
        {
            return true;
        }
        public void Initialize()
        {
            throw new NotImplementedException();

        }
        public void LoadContent(ContentManager content)
        {
            throw new NotImplementedException();
        }



    }
}
