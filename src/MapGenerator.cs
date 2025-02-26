using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameLab
{
    public class MapGenerator
    {
        private static void PreprocessBlocks(List<Block> blocks, Random rng)
        {
            int mode = rng.Next(4);
            int corrProb, closedProb, openProb;
            switch (mode)
            {
                case 0:
                    corrProb = 100;
                    closedProb = 1;
                    openProb = 1;
                    break;
                case 1:
                    corrProb = 10;
                    closedProb = 10;
                    openProb = 1;
                    break;
                case 2:
                    corrProb = 10;
                    closedProb = 1;
                    openProb = 10;
                    break;
                case 3:
                    corrProb = 10;
                    closedProb = 1;
                    openProb = 1;
                    break;
                default:
                    corrProb = 1;
                    closedProb = 1;
                    openProb = 1;
                    break;
            }

            foreach (Block block in blocks)
            {
                block.IsStart = false;
                block.IsTarget = false;
                block.Probability = block.Room.IsCorridor() ? corrProb : block.Room.IsRoom() ? closedProb : openProb;
            }
        }

        private static Block FindBlock(List<Block> blocks, Random rng)
        {
            double totalWeights = 0;
            foreach (Block block in blocks)
            {
                totalWeights += block.Probability;
            }

            double rand = rng.NextDouble();
            double curr = 0;
            int i = 0;
            for (; i < blocks.Count; i++)
            {
                curr += blocks[i].Probability / totalWeights;
                if (rand < curr)
                {
                    break;
                }

            }
            return blocks.ElementAt(i);
        }

        private static double CalculateEntropy(List<Block> blocks)
        {
            double logweights = 0;
            double weights = 0;
            foreach (Block block in blocks)
            {
                weights += block.Probability;
                logweights += block.Probability * Math.Log2(block.Probability);
            }
            return Math.Log2(weights) - (logweights / weights);
        }
        private static bool HasMatchingDoor(List<bool> doors1, List<bool> doors2)
        {
            List<bool> reversedDoors2 = new(doors2);
            reversedDoors2.Reverse();
            if (doors1.Count != 3 || doors2.Count != 3)
            {
                throw new ArgumentException("Both lists must have exactly 3 elements.");
            }

            for (int i = 0; i < 3; i++)
            {
                if (!(doors1[i] == reversedDoors2[i]))
                {
                    return false; // Found non-compatible door
                }
            }

            return true; // All doors are compatible
        }

        private static List<Block> FilterBlocks(List<Block> blocks, bool hasDoor, Block other, string dirCurr, string dirOther)
        {
            return hasDoor
                ? blocks.Where(b => HasMatchingDoor([.. b.Room.Location[dirCurr]], [.. other.Room.Location[dirOther]])
                && !(b.Room.Id.Contains("corr") && other.Room.Id.Contains("corr"))).ToList()
                : blocks.Where(b => !b.Room.Doors[dirCurr]).ToList();
        }

        private static List<Block> FilterBlocksReplace(List<Block> blocks, bool hasDoor, Block[] map, int index, string dirCurr, string dirOther)
        {
            return hasDoor
                ? blocks.Where(b => HasMatchingDoor([.. b.Room.Location[dirCurr]], [.. map[index].Room.Location[dirOther]])
                && !(b.Room.Id.Contains("corr") && map[index].Room.Id.Contains("corr"))).ToList()
                : blocks.Where(b => !b.Room.Doors[dirCurr]).ToList();
        }

        private static Block[] GenerateRooms(List<Block> blocks, int x, int y, Random rng)
        {
            Block[] finalBlocks = new Block[x * y];
            bool[] isFinal = new bool[x * y];
            List<Block>[] potentialBlocks = new List<Block>[x * y];

            for (int i = 0; i < x * y; i++)
            {
                potentialBlocks[i] = [.. blocks.GetRange(0, blocks.Count)];
                if (i % x == 0) //left side of map
                {
                    potentialBlocks[i] = potentialBlocks[i].Where(b => !b.Room.Doors["west"]).ToList();
                }
                else if (i % x == x - 1) //right side of map
                {
                    potentialBlocks[i] = potentialBlocks[i].Where(b => !b.Room.Doors["east"]).ToList(); ;
                }
                if (i / x == 0) //top of map
                {
                    potentialBlocks[i] = potentialBlocks[i].Where(b => !b.Room.Doors["north"]).ToList();
                }
                else if (i / x == x - 1) //bottom of map
                {
                    potentialBlocks[i] = potentialBlocks[i].Where(b => !b.Room.Doors["south"]).ToList();
                }
            }

            for (int i = 0; i < y * x; i++)
            {
                if (potentialBlocks[i].Count == 0)
                {
                    ArgumentException exception = new("List of rooms is impossible to arrange in the map", nameof(blocks));
                    throw exception;
                }
            }

            // we sort according to (#possibilities, tileNr), the value is just a dummy variable
            SortedList<(double, int), int> q = [];

            for (int i = 0; i < x * y; i++)
            {
                q.Add((CalculateEntropy(potentialBlocks[i]), i), 1);
            }

            while (q.Count > 0)
            {
                //remove the smallest element and pick a random block to assign
                (double, int) key = q.ElementAt(0).Key;
                q.RemoveAt(0);
                int tileNr = key.Item2;
                Block found = FindBlock(potentialBlocks[tileNr], rng);
                finalBlocks[tileNr] = found;
                isFinal[tileNr] = true;

                //update all neighbours

                //top
                int neighbourTile = tileNr - x;
                if (0 <= neighbourTile && !isFinal[neighbourTile])
                {
                    (double, int) oldKey = (CalculateEntropy(potentialBlocks[neighbourTile]), neighbourTile);

                    potentialBlocks[neighbourTile] = FilterBlocks(potentialBlocks[neighbourTile], found.Room.Doors["north"], found, "south", "north");

                    //no possibilities left, reset the whole thingy
                    if (potentialBlocks[neighbourTile].Count == 0)
                    {
                        return null;
                    }

                    (double, int) newKey = (CalculateEntropy(potentialBlocks[neighbourTile]), neighbourTile);
                    _ = q.Remove(oldKey);
                    q.Add(newKey, 1);
                }

                //left
                neighbourTile = tileNr - 1;
                if (0 <= neighbourTile && neighbourTile % x != x - 1 && !isFinal[neighbourTile])
                {
                    (double, int) oldKey = (CalculateEntropy(potentialBlocks[neighbourTile]), neighbourTile);

                    potentialBlocks[neighbourTile] = FilterBlocks(potentialBlocks[neighbourTile], found.Room.Doors["west"], found, "east", "west");


                    if (potentialBlocks[neighbourTile].Count == 0)
                    {
                        return null;
                    }

                    (double, int) newKey = (CalculateEntropy(potentialBlocks[neighbourTile]), neighbourTile);
                    _ = q.Remove(oldKey);
                    q.Add(newKey, 1);
                }

                //right
                neighbourTile = tileNr + 1;
                if (neighbourTile < x * y && neighbourTile % x != 0 && !isFinal[neighbourTile])
                {
                    (double, int) oldKey = (CalculateEntropy(potentialBlocks[neighbourTile]), neighbourTile);

                    potentialBlocks[neighbourTile] = FilterBlocks(potentialBlocks[neighbourTile], found.Room.Doors["east"], found, "west", "east");


                    if (potentialBlocks[neighbourTile].Count == 0)
                    {
                        return null;
                    }

                    (double, int) newKey = (CalculateEntropy(potentialBlocks[neighbourTile]), neighbourTile);
                    _ = q.Remove(oldKey);
                    q.Add(newKey, 1);
                }

                //bottom
                neighbourTile = tileNr + x;
                if (neighbourTile < x * y && !isFinal[neighbourTile])
                {
                    (double, int) oldKey = (CalculateEntropy(potentialBlocks[neighbourTile]), neighbourTile);

                    potentialBlocks[neighbourTile] = FilterBlocks(potentialBlocks[neighbourTile], found.Room.Doors["south"], found, "north", "south");

                    if (potentialBlocks[neighbourTile].Count == 0)
                    {
                        return null;
                    }

                    (double, int) newKey = (CalculateEntropy(potentialBlocks[neighbourTile]), neighbourTile);
                    _ = q.Remove(oldKey);
                    q.Add(newKey, 1);
                }
            }
            return finalBlocks;
        }

        private static int FindComponents(List<int>[] graph, int[] components, int n)
        {
            for (int i = 0; i < n; i++)
            {
                components[i] = -1;
            }

            int component = 0;
            for (int i = 0; i < n; i++)
            {
                if (components[i] != -1)
                {
                    continue;
                }


                Queue<int> q = new();
                components[i] = component;
                q.Enqueue(i);

                while (q.Count > 0)
                {
                    int u = q.Dequeue();
                    foreach (int v in graph[u])
                    {
                        if (components[v] == -1)
                        {
                            components[v] = component;
                            q.Enqueue(v);
                        }
                    }
                }
                component++;
            }

            return component;
        }

        private static void FindDistances(List<int>[] graph, int[] dist, int n, int source)
        {
            for (int i = 0; i < n; i++)
            {
                dist[i] = -1;
            }

            Queue<int> q = new();
            dist[source] = 0;
            q.Enqueue(source);

            while (q.Count > 0)
            {
                int u = q.Dequeue();
                foreach (int v in graph[u])
                {
                    if (dist[v] == -1)
                    {
                        dist[v] = dist[u] + 1;
                        q.Enqueue(v);
                    }
                }
            }
        }

        private static void ReplaceBlock(List<Block> blocks, Block[] map, int fst, int snd, int x, Random rng)
        {
            if (fst > snd)
            {
                (fst, snd) = (snd, fst);
            }


            int diff = snd - fst;

            Block currBlockFst = map[fst];
            Block currBlockSnd = map[snd];

            List<Block> potentialBlocksFst = [.. blocks.GetRange(0, blocks.Count)];
            List<Block> potentialBlocksSnd = [.. blocks.GetRange(0, blocks.Count)];

            //if (fst - x > 0)
            //{
            potentialBlocksFst = FilterBlocksReplace(potentialBlocksFst, currBlockFst.Room.Doors["north"], map, fst - x, "north", "south");
            //}
            //if (fst - 1 > 0)
            //{
            potentialBlocksFst = FilterBlocksReplace(potentialBlocksFst, currBlockFst.Room.Doors["west"], map, fst - 1, "west", "east");
            //}
            //if (snd + x < x * y)
            //{
            potentialBlocksSnd = FilterBlocksReplace(potentialBlocksSnd, currBlockSnd.Room.Doors["south"], map, snd + x, "south", "north");
            //}
            //if (snd + 1 < x * y)
            //{
            potentialBlocksSnd = FilterBlocksReplace(potentialBlocksSnd, currBlockSnd.Room.Doors["east"], map, snd + 1, "east", "west");
            //}

            //fst is left of snd
            if (diff == 1)
            {
                //if (fst + x < x * y)
                //{
                potentialBlocksFst = FilterBlocksReplace(potentialBlocksFst, currBlockFst.Room.Doors["south"], map, fst + x, "south", "north");
                //}
                //if (snd - x > 0)
                //{
                potentialBlocksSnd = FilterBlocksReplace(potentialBlocksSnd, currBlockSnd.Room.Doors["north"], map, snd - x, "north", "south");
                //}

                potentialBlocksFst = potentialBlocksFst.Where(b => b.Room.Doors["east"]).ToList();

                currBlockFst = potentialBlocksFst.ElementAt(rng.Next(potentialBlocksFst.Count));

                potentialBlocksSnd = potentialBlocksSnd.Where(
                    b => HasMatchingDoor([.. b.Room.Location["west"]], [.. currBlockFst.Room.Location["east"]])
                    ).ToList();

                currBlockSnd = potentialBlocksSnd.ElementAt(rng.Next(potentialBlocksSnd.Count));
            }
            //fst is north of snd
            else if (diff == x)
            {
                //if (fst + 1 < x * y)
                //{
                potentialBlocksFst = FilterBlocksReplace(potentialBlocksFst, currBlockFst.Room.Doors["east"], map, fst + 1, "east", "west");
                //}
                //if (snd - 1 > 0)
                //{
                potentialBlocksSnd = FilterBlocksReplace(potentialBlocksSnd, currBlockSnd.Room.Doors["west"], map, snd - 1, "west", "east");
                //}

                potentialBlocksFst = potentialBlocksFst.Where(b => b.Room.Doors["south"]).ToList();

                currBlockFst = potentialBlocksFst.ElementAt(rng.Next(potentialBlocksFst.Count));

                potentialBlocksSnd = potentialBlocksSnd.Where(
                    b => HasMatchingDoor([.. b.Room.Location["north"]], [.. currBlockFst.Room.Location["south"]])
                    ).ToList();

                currBlockSnd = potentialBlocksSnd.ElementAt(rng.Next(potentialBlocksSnd.Count));
            }

            map[fst] = currBlockFst;
            map[snd] = currBlockSnd;
        }

        private static Block[] ConnectRooms(List<Block> blocks, Block[] map, int x, int y, Random rng)
        {
            if (map == null)
            {
                return null;
            }
            // for any two neighbouring blocks, save them in the door or wall graph, depending on if the have a door between them or not

            List<int>[] doorGraph = new List<int>[x * y];
            List<int>[] wallGraph = new List<int>[x * y];

            for (int u = 0; u < x * y; u++)
            {
                doorGraph[u] = [];
                wallGraph[u] = [];

                int v = u - x;
                if (0 <= v && v < x * y)
                {
                    if (map[u].Room.Doors["north"])
                    {
                        doorGraph[u].Add(v);
                    }
                    else
                    {
                        wallGraph[u].Add(v);
                    }
                }

                v = u - 1;
                if (0 <= v && v < x * y && v % x != x - 1)
                {
                    if (map[u].Room.Doors["west"])
                    {
                        doorGraph[u].Add(v);
                    }
                    else
                    {
                        wallGraph[u].Add(v);
                    }
                }

                v = u + 1;
                if (0 <= v && v < x * y && v % x != 0)
                {
                    if (map[u].Room.Doors["east"])
                    {
                        doorGraph[u].Add(v);
                    }
                    else
                    {
                        wallGraph[u].Add(v);
                    }
                }

                v = u + x;
                if (0 <= v && v < x * y)
                {
                    if (map[u].Room.Doors["south"])
                    {
                        doorGraph[u].Add(v);
                    }
                    else
                    {
                        wallGraph[u].Add(v);
                    }
                }
            }

            int[] components = new int[x * y];
            int compCount = FindComponents(doorGraph, components, x * y);

            while (compCount-- > 1)
            {
                List<(int, int)> potentialBlocks = [];

                for (int i = 0; i < x * y; i++)
                {
                    foreach (int j in wallGraph[i])
                    {
                        if (components[i] != components[j])
                        {
                            potentialBlocks.Add((i, j));
                        }
                    }
                }

                (int, int) pair = potentialBlocks.ElementAt(rng.Next(potentialBlocks.Count));

                try
                {
                    ReplaceBlock(blocks, map, pair.Item1, pair.Item2, x, rng);
                }
                catch
                {
                    File.AppendAllText(Path.Combine(Environment.CurrentDirectory, "log.txt"), "Could not replace blocks\n");
                    return null;
                }

                int fstComp = components[pair.Item1];
                int sndComp = components[pair.Item2];
                for (int i = 0; i < x * y; i++)
                {
                    if (components[i] == sndComp)
                    {
                        components[i] = fstComp;
                    }

                }

                doorGraph[pair.Item1].Add(pair.Item2);
                doorGraph[pair.Item2].Add(pair.Item1);
                _ = wallGraph[pair.Item1].Remove(pair.Item2);
                _ = wallGraph[pair.Item2].Remove(pair.Item1);
            }

            Block startBlock;
            int startIndex;
            do
            {
                startIndex = rng.Next(map.Length);
                startBlock = map[startIndex];

            } while (startBlock.Room.IsCorridor());
            startBlock.IsStart = true;

            int[] distances = new int[x * y];
            FindDistances(doorGraph, distances, x * y, startIndex);

            List<Block> longDist = [];
            int maxDist = distances.Max();

            while (longDist.Count == 0)
            {
                for (int i = 0; i < x * y; i++)
                {
                    if (distances[i] == maxDist && !map[i].Room.IsCorridor())
                    {
                        longDist.Add(map[i]);
                    }
                }
                maxDist--;
            }

            int targetIndex = rng.Next(longDist.Count);
            Block targetBlock = longDist.ElementAt(targetIndex); ;
            targetBlock.IsTarget = true;

            return map;
        }

        /// <summary>
        /// Generates the Map using wavefunction collapse
        /// </summary>
        /// <param name="blocks"> The list of possible blocks</param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>A List of Blocks ordered row by row from top left of the map to the bottom right</returns>
        public static List<Block> GenerateMap(List<Block> blocks, int x, int y)
        {
            File.WriteAllText(Path.Combine(Environment.CurrentDirectory, "log.txt"), string.Empty);
            Random rng = new();
            PreprocessBlocks(blocks, rng);

            Block[] map = null;
            while (map == null)
            {
                map = GenerateRooms(blocks, x, y, rng);
                map = ConnectRooms(blocks, map, x, y, rng);
            }

            return [.. map];
        }
    }
}