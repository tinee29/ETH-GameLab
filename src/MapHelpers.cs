using System;
using System.Collections.Generic;

using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Vector2 = Microsoft.Xna.Framework.Vector2;


namespace GameLab
{

    public class MapHelpers
    {
        private const int ROOMSIZE = 1000;
        private const int DOORSIZE = 200;
        private const int WALLSIZE = 100;
        private const int LARGEWALL = 120;
        private const int SCALING = 1;
        private const int SCALEDROOM = ROOMSIZE / SCALING;
        private const int SCALEDWALL = WALLSIZE / SCALING;
        private const int SCALEDLARGEWALL = LARGEWALL / SCALING;

        private static int GetSheetIndex(Room room, string s)
        {
            bool[] doors = room.Location[s];
            return s == "north"
                ? doors.All(x => x) ? 4 : doors[0] ? 2 : doors[1] ? 1 : doors[2] ? 3 : 0
                : s == "west"
                    ? doors.All(x => x) ? 4 : doors[0] ? 3 : doors[1] ? 1 : doors[2] ? 2 : 0
                    : s == "east"
                                    ? doors.All(x => x) ? 4 : doors[0] ? 1 : doors[1] ? 2 : doors[2] ? 0 : 3
                                    : doors.All(x => x) ? 4 : doors[0] ? 3 : doors[1] ? 1 : doors[2] ? 2 : 0;

        }

        public static List<Block> LoadRoomBlocks(RoomData blockData)
        {

            List<Block> blocks = [];

            for (int i = 0; i < blockData.Rooms.Count; i++)
            {
                Room room = blockData.Rooms.ElementAt(i);
                if (!room.Active)
                {
                    continue;
                }

                for (int rotationIndex = 0; rotationIndex <= 3; rotationIndex++)
                {
                    Room rotatedRoom = room.GetRotatedVersion(rotationIndex);
                    Rectangle topRectangle = new(0, (GetSheetIndex(rotatedRoom, "north") * SCALEDLARGEWALL) + 1, SCALEDROOM, SCALEDLARGEWALL - 2);
                    Rectangle leftRectangle = new((GetSheetIndex(rotatedRoom, "west") * SCALEDWALL) + 1, 0, SCALEDWALL - 2, SCALEDROOM - 180);
                    Rectangle rightRectangle = new((GetSheetIndex(rotatedRoom, "east") * SCALEDWALL) + 1, 0, SCALEDWALL - 2, SCALEDROOM - 180);
                    Rectangle bottomRectangle = new(0, (GetSheetIndex(rotatedRoom, "south") * SCALEDWALL) + 1, SCALEDROOM, SCALEDWALL - 2);

                    blocks.Add(new Block
                    {
                        TopRectangle = topRectangle,
                        LeftRectangle = leftRectangle,
                        RightRectangle = rightRectangle,
                        BottomRectangle = bottomRectangle,
                        Room = rotatedRoom,
                        IsMirrored = false
                    });
                }
                if (room.Mirror)
                {
                    Room mirroredRoom = room.GetMirroredVersion();

                    for (int rotationIndex = 0; rotationIndex <= 3; rotationIndex++)
                    {
                        Room transformedRoom = mirroredRoom.GetRotatedVersion(rotationIndex);
                        Rectangle topRectangle = new(0, (GetSheetIndex(transformedRoom, "north") * SCALEDLARGEWALL) + 1, SCALEDROOM - 1, SCALEDLARGEWALL - 2);
                        Rectangle leftRectangle = new((GetSheetIndex(transformedRoom, "west") * SCALEDWALL) + 1, 0, SCALEDWALL - 2, SCALEDROOM - 180);
                        Rectangle rightRectangle = new((GetSheetIndex(transformedRoom, "east") * SCALEDWALL) + 1, 0, SCALEDWALL - 2, SCALEDROOM - 180);
                        Rectangle bottomRectangle = new(0, (GetSheetIndex(transformedRoom, "south") * SCALEDWALL) + 1, SCALEDROOM - 1, SCALEDWALL - 2);

                        blocks.Add(new Block
                        {
                            TopRectangle = topRectangle,
                            LeftRectangle = leftRectangle,
                            RightRectangle = rightRectangle,
                            BottomRectangle = bottomRectangle,
                            Room = transformedRoom,
                            IsMirrored = true,
                        });
                    }
                }

            }
            return blocks;
        }

        public static List<Block> LoadCorridorBlocks(Texture2D sheet, RoomData blockData)
        {

            List<Block> blocks = [];

            for (int i = 0; i < blockData.Rooms.Count; i++)
            {
                Room room = blockData.Rooms.ElementAt(i);
                if (!room.Active)
                {
                    continue;
                }

                Rectangle sourceRectangle = new(0, (i * SCALEDROOM) + 1, SCALEDROOM, SCALEDROOM - 2);

                Room rotatedRoom = room.GetRotatedVersion(0);
                blocks.Add(new Block
                {
                    CorridorTexture = sheet,
                    SourceRectangle = sourceRectangle,
                    Room = rotatedRoom,
                    IsMirrored = false
                });

                if (room.Mirror)
                {
                    Room mirroredRoom = room.GetMirroredVersion();
                    Room transformedRoom = mirroredRoom.GetRotatedVersion(0);
                    blocks.Add(new Block
                    {
                        CorridorTexture = sheet,
                        SourceRectangle = sourceRectangle,
                        Room = transformedRoom,
                        IsMirrored = true
                    });

                }

            }
            return blocks;
        }
        public static List<Obstacle> LoadObstacles(Texture2D sheet, ObstacleContainer obstacleData)
        {

            List<Obstacle> obstacles = [];
            int slack = Player.SLACK;

            foreach (ObstacleData obstacle in obstacleData.Obstacles)
            {
                if (!obstacle.Active)
                {
                    continue;
                }
                // Assuming room IDs are like "room1", "room2", ..., "room16".
                // int index = int.Parse(System.Text.RegularExpressions.Regex.Match(obstacle.Id, @"\d+").Value) - 1; // Converts ID to an index
                // Calculate the source rectangle based on row and column.
                Rectangle sourceRectangleBottom = new(obstacle.X, obstacle.Y + slack, obstacle.Width, obstacle.Height - slack);
                Rectangle sourceRectangleTop = new(obstacle.X, obstacle.Y, obstacle.Width, slack);
                obstacles.Add(new Obstacle
                {
                    Texture = sheet,
                    SourceRectangleBottom = sourceRectangleBottom,
                    SourceRectangleTop = sourceRectangleTop,
                    ShapeBottom = new Vector2(obstacle.Width, obstacle.Height - slack),
                    ShapeTop = new Vector2(obstacle.Width, slack),
                    PositionBottom = new(0, slack),
                    PositionTop = new(0, 0)
                }
                    );
            }
            return obstacles;
        }

        public static (Dictionary<int, List<Obstacle>>, Dictionary<int, List<Vector2>>) GenerateObstaclesAndSpawners(List<Block> blocks, List<Obstacle> obstacleTemplates, int minObstacles, int maxObstacles, int minSpawns, int maxSpawns, Random rng, int startingRoomIndex, int targetRoomIndex, Dictionary<int, List<Item>> items)
        {
            Dictionary<int, List<Obstacle>> blockObstacles = [];
            Dictionary<int, List<Vector2>> spawnPoints = [];
            int idx = 0;
            int mapSize = (int)Math.Sqrt(blocks.Count());
            foreach (Block block in blocks)
            {
                List<Obstacle> obstaclesForCurrentBlock = [];
                List<Vector2> spawnsForCurrentBlock = [];
                Vector2 blockPos = CalculateBlockPosition(idx, mapSize);

                if (block.Room.IsCorridor() || startingRoomIndex == idx || targetRoomIndex == idx || items[idx].Count > 0)
                {
                    //blockObstacles.Add(idx, obstaclesForCurrentBlock);
                    //spawnPoints.Add(idx, spawnsForCurrentBlock);
                    idx++;
                    continue;
                }
                int obstacleCount = rng.Next(minObstacles, maxObstacles + 1);

                for (int i = 0; i < obstacleCount; i++)
                {
                    Obstacle template = obstacleTemplates[rng.Next(obstacleTemplates.Count)];
                    Obstacle obstacle = CloneObstacle(template);

                    bool isValid = false;
                    int attempt = 0;
                    const int maxAttempts = 10; // To prevent infinite loops

                    while (!isValid && attempt < maxAttempts)
                    {
                        Vector2 position = CalculateRandomObstaclePosition(block, obstacle, rng);
                        Rectangle obstacleRect = new(
                            (int)position.X + (int)blockPos.X,
                            (int)position.Y + (int)blockPos.Y,
                            (int)obstacle.ShapeBottom.X,
                            (int)obstacle.ShapeBottom.Y
                        );

                        // Check if the new obstacle intersects with any existing obstacle
                        bool intersects = obstaclesForCurrentBlock.Any(existingObstacle => obstacleRect.Intersects(new Rectangle((int)existingObstacle.PositionBottom.X, (int)existingObstacle.PositionBottom.Y, (int)existingObstacle.ShapeBottom.X, (int)existingObstacle.ShapeBottom.Y)));
                        if (!intersects)
                        {
                            obstacle.PositionTop = new Vector2(obstacleRect.X, obstacleRect.Y) - obstacle.PositionBottom;
                            obstacle.PositionBottom = new Vector2(obstacleRect.X, obstacleRect.Y);
                            obstaclesForCurrentBlock.Add(obstacle);
                            isValid = true;
                        }
                        attempt++;
                    }
                }

                int enemiesCount = rng.Next(minSpawns, maxSpawns);
                for (int i = 0; i < enemiesCount; i++)
                {
                    bool isValid = false;
                    int attempt = 0;
                    const int maxAttempts = 10; // To prevent infinite loops

                    while (!isValid && attempt < maxAttempts)
                    {
                        int spawnX = rng.Next(100, 900 - Enemy.ENEM_DIM);
                        int spawnY = rng.Next(100, 900 - Enemy.ENEM_DIM);
                        Vector2 spawnPointPosition = new(spawnX + blockPos.X, spawnY + blockPos.Y);
                        bool intersects = obstaclesForCurrentBlock.Any(existingObstacle => new Rectangle((int)spawnPointPosition.X, (int)spawnPointPosition.Y, Enemy.ENEM_DIM, Enemy.ENEM_DIM).Intersects(new Rectangle((int)existingObstacle.PositionBottom.X, (int)existingObstacle.PositionBottom.Y, (int)existingObstacle.ShapeBottom.X, (int)existingObstacle.ShapeBottom.Y)));
                        if (!intersects)
                        {
                            spawnsForCurrentBlock.Add(spawnPointPosition);
                            isValid = true;
                        }
                        attempt++;
                    }
                }


                // Only add the list to the dictionary if obstacles were generated for the block
                if (obstaclesForCurrentBlock.Count > 0)
                {
                    blockObstacles.Add(idx, obstaclesForCurrentBlock);
                }
                if (spawnsForCurrentBlock.Count > 0)
                {
                    spawnPoints.Add(idx, spawnsForCurrentBlock);
                }

                idx++;
            }
            return (blockObstacles, spawnPoints);
        }
        public static Obstacle CloneObstacle(Obstacle originalObstacle)
        {
            return new Obstacle
            {
                Texture = originalObstacle.Texture,
                SourceRectangleBottom = originalObstacle.SourceRectangleBottom,
                SourceRectangleTop = originalObstacle.SourceRectangleTop,
                ShapeBottom = originalObstacle.ShapeBottom,
                ShapeTop = originalObstacle.ShapeTop,
                PositionBottom = originalObstacle.PositionBottom,
                PositionTop = originalObstacle.PositionTop,
            };
        }
        public static Vector2 CalculateRandomObstaclePosition(Block block, Obstacle obstacle, Random rng)
        {
            // Example calculation, needs adjustment based on actual room/corridor dimensions
            int width = (int)obstacle.ShapeBottom.X;
            int height = (int)obstacle.ShapeBottom.Y;
            int minX = 100; // Assuming 50 pixels from the edge and doors
            int maxX = 900 - width; // Assuming room width is 1000
            int minY = 100;
            int maxY = 900 - height; // Assuming room height is 1000
            int x;
            int y;
            bool isValidPosition;
            const float threshold = 10f;
            Dictionary<string, Rectangle> wallsDict = new()

            {
                { "south", new Rectangle(0, 999, 1000, 1) },
                { "west", new Rectangle(0, 0, 1, 1000) },
                { "north", new Rectangle(0, 0, 1000, 1) },
                { "east", new Rectangle(999, 0, 1, 1000) }
            };

            do
            {
                x = rng.Next(minX, maxX);
                y = rng.Next(minY, maxY);

                Rectangle proposedObstacle = new(x, y, width, height);
                isValidPosition = true;

                foreach (Rectangle doorArea in block.Room.DoorAreas)
                {
                    if (Globals.DistanceBetweenRectangles(proposedObstacle, doorArea) < 100 + (2 * Player.PLAYER_DIM))
                    {
                        isValidPosition = false;
                        break;
                    }
                }
                if (isValidPosition)
                {
                    foreach (KeyValuePair<string, Rectangle> wall in wallsDict)
                    {
                        Rectangle wallRect = wall.Value;
                        string wallSide = wall.Key;
                        if (Globals.DistanceBetweenRectangles(proposedObstacle, wallRect) < threshold + Player.PLAYER_DIM + 100)
                        {
                            Vector2 adjustedPosition = MoveToWall(x, y, width, height, wallSide);
                            x = (int)adjustedPosition.X;
                            y = (int)adjustedPosition.Y;
                        }
                    }
                }
            }
            while (!isValidPosition);

            return new Vector2(x, y);
        }

        public static Vector2 MoveToWall(int xPos, int yPos, int width, int height, string wall)
        {
            int x = xPos;
            int y = yPos;

            switch (wall)
            {
                case "south": y = 900 - height; break;
                case "west": x = 100; break;
                case "north": y = 100; break;
                case "east": x = 900 - width; break;
                default:
                    break;
            }
            return new Vector2(x, y);
        }



        public static List<Rectangle> ComputeRoomWallSegments(List<Rectangle> doors, bool isVertical, bool toreverse)
        {
            int start = 0;
            int end = 1000; // Assuming each side is 1000 pixels long
            List<Rectangle> wallSegments = [];
            if (toreverse)
            {
                doors.Reverse();
            }
            // Start of the first wall segment
            int wallStart = start;
            foreach (Rectangle door in doors)
            {
                // Door start position
                int doorStart = isVertical ? door.Y : door.X;
                // Door end position
                int doorEnd = doorStart + (isVertical ? door.Height : door.Width);

                // Check if there's space between the current wallStart and the start of the door
                if (doorStart > wallStart)
                {
                    // Add a wall segment before the door
                    wallSegments.Add(Globals.NormalizeRectangle(isVertical
                        ? new Rectangle(toreverse ? 0 : 1000, wallStart, toreverse ? 100 : -100, doorStart - wallStart)
                        : new Rectangle(wallStart, toreverse ? 1000 : 0, doorStart - wallStart, toreverse ? -100 : 100)));
                }
                // Move the start of the next wall segment to the end of the current door
                wallStart = doorEnd;
            }

            // Check if there's space between the last door and the end of the side
            if (wallStart < end)
            {
                // Add the final wall segment
                wallSegments.Add(Globals.NormalizeRectangle(isVertical
                    ? new Rectangle(toreverse ? 0 : 1000, wallStart, toreverse ? 100 : -100, end - wallStart)
                    : new Rectangle(wallStart, toreverse ? 1000 : 0, end - wallStart, toreverse ? -100 : 100)));
            }

            return wallSegments;
        }
        public static void ComputeRoomBordersAndDoors(List<Block> blocks)
        {

            foreach (Block block in blocks)
            {
                Room room = block.Room;
                room.WallBorders.Clear();
                room.DoorAreas.Clear();

                foreach (string side in room.Location.Keys)
                {

                    if (room.Doors[side] == false)
                    {
                        Rectangle wall = side switch
                        {
                            "north" => new Rectangle(0, 0, ROOMSIZE, 100),
                            "south" => new Rectangle(0, ROOMSIZE, ROOMSIZE, -100),
                            "east" => new Rectangle(ROOMSIZE, 0, -100, ROOMSIZE),
                            "west" => new Rectangle(0, 0, 100, ROOMSIZE),
                            _ => Rectangle.Empty
                        };
                        room.WallBorders.Add(Globals.NormalizeRectangle(wall));
                    }
                    else if (room.Location[side].All(x => x))
                    {
                        List<Rectangle> sideDoors = [];
                        Rectangle door = side switch
                        {
                            "north" => new Rectangle(WALLSIZE, 0, ROOMSIZE - (2 * WALLSIZE), 1),
                            "south" => new Rectangle(WALLSIZE, ROOMSIZE, ROOMSIZE - (2 * WALLSIZE), 1),
                            "east" => new Rectangle(ROOMSIZE, WALLSIZE, 1, ROOMSIZE - (2 * WALLSIZE)),
                            "west" => new Rectangle(0, WALLSIZE, 1, ROOMSIZE - (2 * WALLSIZE)),
                            _ => Rectangle.Empty
                        };
                        sideDoors.Add(Globals.NormalizeRectangle(door));

                        bool isVertical = false;
                        bool toreverse = false;
                        switch (side)
                        {
                            case "north": break;
                            case "south": toreverse = true; break;
                            case "east": isVertical = true; break;
                            case "west": isVertical = true; toreverse = true; break;
                            default:
                                break;

                        }
                        List<Rectangle> walls = ComputeRoomWallSegments(sideDoors, isVertical, toreverse);
                        room.WallBorders.AddRange(walls);
                        //room.DoorAreas.AddRange(sideDoors);
                    }
                    else
                    {
                        List<Rectangle> sideDoors = [];
                        for (int i = 0; i < 3; i++)
                        {
                            int startPixel = 0;

                            switch (i)
                            {
                                case 0: startPixel = (side is "north" or "east") ? 100 : 700; break;
                                case 1: startPixel = 400; break;
                                case 2: startPixel = (side is "north" or "east") ? 700 : 100; break;
                                default:
                                    break;

                            }
                            if (room.Location[side][i])
                            {
                                Rectangle door = side switch
                                {
                                    "north" => new Rectangle(startPixel, 0, DOORSIZE, 1),
                                    "south" => new Rectangle(startPixel, ROOMSIZE, DOORSIZE, 1),
                                    "east" => new Rectangle(ROOMSIZE, startPixel, 1, DOORSIZE),
                                    "west" => new Rectangle(0, startPixel, 1, DOORSIZE),
                                    _ => Rectangle.Empty
                                };
                                sideDoors.Add(Globals.NormalizeRectangle(door));
                            }
                        }
                        bool isVertical = false;
                        bool toreverse = false;
                        switch (side)
                        {
                            case "north": break;
                            case "south": toreverse = true; break;
                            case "east": isVertical = true; break;
                            case "west": isVertical = true; toreverse = true; break;
                            default:
                                break;

                        }
                        List<Rectangle> walls = ComputeRoomWallSegments(sideDoors, isVertical, toreverse);
                        room.WallBorders.AddRange(walls);
                        room.DoorAreas.AddRange(sideDoors);

                    }
                }
            }
        }

        public static (Vector2 leftPos, Vector2 rigthPos, string opp, string nx) GetDoorSidesPos(Room room, string side)
        {
            Vector2 leftDoorSidePos = new(0);
            Vector2 rightDoorSidePos = new(0);
            string opposite = "";
            string next = "";
            switch (side)
            {
                case "south":
                    if (room.Location[side][0])
                    {
                        leftDoorSidePos = new Vector2(700, 1000);
                        rightDoorSidePos = new Vector2(900, 1000);
                    }
                    else if (room.Location[side][2])
                    {
                        leftDoorSidePos = new Vector2(100, 1000);
                        rightDoorSidePos = new Vector2(300, 1000);
                    }
                    else
                    {
                        leftDoorSidePos = new Vector2(400f, 1000f);
                        rightDoorSidePos = new Vector2(600f, 1000f);
                    }
                    next = "west";
                    opposite = "north";
                    return (leftDoorSidePos, rightDoorSidePos, opposite, next);
                case "west":
                    if (room.Location[side][0])
                    {
                        leftDoorSidePos.X = 0; leftDoorSidePos.Y = 700;
                        rightDoorSidePos.X = 0; rightDoorSidePos.Y = 900;
                    }
                    else if (room.Location[side][2])
                    {
                        leftDoorSidePos.X = 0; leftDoorSidePos.Y = 100;
                        rightDoorSidePos.X = 0; rightDoorSidePos.Y = 300;
                    }
                    else
                    {
                        leftDoorSidePos.X = 0; leftDoorSidePos.Y = 400;
                        rightDoorSidePos.X = 0; rightDoorSidePos.Y = 600;
                    }
                    next = "north";
                    opposite = "east";
                    return (leftDoorSidePos, rightDoorSidePos, opposite, next);
                case "north":
                    if (room.Location[side][0])
                    {
                        leftDoorSidePos.X = 300; leftDoorSidePos.Y = 0;
                        rightDoorSidePos.X = 100; rightDoorSidePos.Y = 0;
                    }
                    else if (room.Location[side][2])
                    {
                        leftDoorSidePos.X = 900; leftDoorSidePos.Y = 0;
                        rightDoorSidePos.X = 700; rightDoorSidePos.Y = 0;
                    }
                    else
                    {
                        leftDoorSidePos.X = 600; leftDoorSidePos.Y = 0;
                        rightDoorSidePos.X = 400; rightDoorSidePos.Y = 0;
                    }
                    next = "east";
                    opposite = "south";
                    return (leftDoorSidePos, rightDoorSidePos, opposite, next);
                case "east":
                    if (room.Location[side][0])
                    {
                        leftDoorSidePos.X = 1000; leftDoorSidePos.Y = 300;
                        rightDoorSidePos.X = 1000; rightDoorSidePos.Y = 100;
                    }
                    else if (room.Location[side][2])
                    {
                        leftDoorSidePos.X = 1000; leftDoorSidePos.Y = 900;
                        rightDoorSidePos.X = 1000; rightDoorSidePos.Y = 700;
                    }
                    else
                    {
                        leftDoorSidePos.X = 1000; leftDoorSidePos.Y = 600;
                        rightDoorSidePos.X = 1000; rightDoorSidePos.Y = 400;
                    }
                    next = "south";
                    opposite = "west";
                    return (leftDoorSidePos, rightDoorSidePos, opposite, next);
                default:
                    break;
            }
            return (leftDoorSidePos, rightDoorSidePos, opposite, next);
        }

        public static void ComputeCorridorBordersAndDoors(List<Block> blocks)
        {
            foreach (Block block in blocks)
            {
                Room room = block.Room;
                room.WallBorders.Clear();
                room.DoorAreas.Clear();
                foreach (string side in room.Location.Keys)
                {
                    Vector2 currDoorPos;
                    Vector2 nextDoorPos;

                    Vector2 doorLeftPos;
                    Vector2 doorRightPos;

                    Rectangle wall;
                    Rectangle normalized_wall;
                    string currDoor = side;
                    string nextDoor;
                    string oppositeDoor;
                    int wallWidth;
                    int wallHeight;
                    (doorLeftPos, _, oppositeDoor, nextDoor) = GetDoorSidesPos(room, side);
                    currDoorPos = doorLeftPos;
                    if (room.Doors[nextDoor])
                    {
                        (_, doorRightPos, _, _) = GetDoorSidesPos(room, nextDoor);
                        nextDoorPos = doorRightPos;
                    }
                    else
                    {
                        if (room.Doors[oppositeDoor] && room.Location[oppositeDoor][0] && room.Location[currDoor][0] == false)
                        {
                            (_, doorRightPos, _, _) = GetDoorSidesPos(room, nextDoor);
                            nextDoorPos = doorRightPos;

                            switch (nextDoor)
                            {
                                case "east":
                                    wallWidth = -100;
                                    wallHeight = 200;
                                    break;
                                case "south":
                                    wallWidth = -200;
                                    wallHeight = -100;
                                    break;
                                case "west":
                                    wallWidth = 100;
                                    wallHeight = -200;
                                    break;
                                case "north":
                                    wallWidth = 200;
                                    wallHeight = 100;
                                    break;
                                default:
                                    wallHeight = 0; wallWidth = 0; break;
                            }
                            wall = new Rectangle((int)nextDoorPos.X, (int)nextDoorPos.Y, wallWidth, wallHeight);
                            normalized_wall = Globals.NormalizeRectangle(wall);
                            room.WallBorders.Add(normalized_wall);
                        }
                        else if (room.Location[currDoor][0])
                        {
                            (Vector2 _, Vector2 rightPos, string _, string _) = GetDoorSidesPos(room, nextDoor);
                            nextDoorPos = rightPos;
                            (Vector2 _, Vector2 right, string _, string _) = GetDoorSidesPos(room, oppositeDoor);
                            Vector2 oppositeDoorPos = right;
                            switch (nextDoor)
                            {
                                case "east":
                                    wallWidth = (int)(oppositeDoorPos.X - nextDoorPos.X);
                                    wallHeight = 200;
                                    break;
                                case "south":
                                    wallWidth = -200;
                                    wallHeight = (int)(oppositeDoorPos.Y - nextDoorPos.Y);
                                    break;
                                case "west":
                                    wallWidth = (int)(oppositeDoorPos.X - nextDoorPos.X);
                                    wallHeight = -200;
                                    break;
                                case "north":
                                    wallWidth = 200;
                                    wallHeight = (int)(oppositeDoorPos.Y - nextDoorPos.Y);
                                    break;
                                default:
                                    wallHeight = 0; wallWidth = 0; break;
                            }
                            wall = new Rectangle((int)nextDoorPos.X, (int)nextDoorPos.Y, wallWidth, wallHeight);
                            normalized_wall = Globals.NormalizeRectangle(wall);
                            room.WallBorders.Add(normalized_wall);
                        }
                        else
                        {
                            (Vector2 leftPos, Vector2 _, string _, string _) = GetDoorSidesPos(room, nextDoor);
                            nextDoorPos = leftPos;
                        }
                    }
                    wallWidth = (int)(nextDoorPos.X - currDoorPos.X);
                    wallHeight = (int)(nextDoorPos.Y - currDoorPos.Y);
                    wall = new Rectangle((int)currDoorPos.X, (int)currDoorPos.Y, wallWidth, wallHeight);
                    normalized_wall = Globals.NormalizeRectangle(wall);
                    room.WallBorders.Add(normalized_wall);
                }

            }
        }
        public static Vector2 CalculateBlockPosition(int idx, int mapSize)
        {
            int gridX = idx % mapSize;
            int gridY = idx / mapSize;
            Vector2 position = new(gridX * Map.BLOCKSIZE, gridY * Map.BLOCKSIZE);
            return position;
        }
        public static List<Rectangle> GetCollisionRectangles(List<Block> blocks, Dictionary<int, List<Obstacle>> blockObstacles, int mapSize)
        {
            List<Rectangle> collidables = [];
            int idx = 0;
            foreach (Block block in blocks)
            {
                Vector2 blockPosition = CalculateBlockPosition(idx, mapSize); // Now pass mapSize if needed
                foreach (Rectangle wall in block.Room.WallBorders)
                {
                    Rectangle adjustedWall = new(
                        wall.X + (int)blockPosition.X,
                        wall.Y + (int)blockPosition.Y,
                        wall.Width,
                        wall.Height);

                    collidables.Add(adjustedWall);
                }
                if (blockObstacles.ContainsKey(idx))
                {
                    foreach (Obstacle obstacle in blockObstacles[idx])
                    {
                        Rectangle adjustedObstacle = new(
                            (int)obstacle.PositionBottom.X,
                            (int)obstacle.PositionBottom.Y,
                            (int)obstacle.ShapeBottom.X,
                            (int)obstacle.ShapeBottom.Y);

                        collidables.Add(adjustedObstacle);
                    }
                }

                idx++;
            }

            return collidables;
        }
        public static Dictionary<int, List<Item>> PopulateItems(List<Block> map, int mapSize, SpriteBatch sb, int startingRoomIndex, int targetRoomIndex)
        {
            Random rng = new();
            Dictionary<int, List<Item>> itemsDict = [];

            // Initialize all blocks with an empty list first
            for (int i = 0; i < map.Count; i++)
            {
                itemsDict[i] = [];
            }
            List<int> validIndices = Enumerable.Range(0, map.Count)
                                       .Where(index => index != startingRoomIndex && index != targetRoomIndex)
                                       .ToList();

            for (int i = 0; i < Math.Ceiling(mapSize * mapSize / 10f); i++)
            {
                int selectedBlockIdx = 0;
                do
                {
                    selectedBlockIdx = validIndices[rng.Next(validIndices.Count)];
                }
                while (map[selectedBlockIdx].Room.Id.Contains("corr"));


                int itemType = rng.Next(1, 3);
                Vector2 position = new(500, 500);

                // Add item only to the randomly selected block
                Vector2 blockPosition = CalculateBlockPosition(selectedBlockIdx, mapSize);
                Vector2 spawnPosition = blockPosition + position;
                Item item = null;
                switch (itemType)
                {
                    case 1:
                        item = new CoffeeItem(sb, spawnPosition);
                        break;
                    case 2:
                        item = new IceItem(sb, spawnPosition);
                        break;
                    default:
                        break;
                }
                if (item != null)
                {
                    if (!itemsDict.ContainsKey(selectedBlockIdx))
                    {
                        itemsDict[selectedBlockIdx] = [];
                    }
                    itemsDict[selectedBlockIdx].Add(item);
                }
                validIndices.Remove(selectedBlockIdx);
            }
            return itemsDict;

        }
    }

}
