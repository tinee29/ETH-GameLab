using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System;

namespace GameLab
{
    public class ObstacleContainer
    {
        public List<ObstacleData> Obstacles { get; set; }
        public static ObstacleContainer LoadObstacleData(string jsonFile)
        {
            string bin_path = Path.GetDirectoryName(Environment.ProcessPath);

            string path = Path.Combine(bin_path, "Content", jsonFile);
            using StreamReader r = new(path);
            string json = r.ReadToEnd();
            ObstacleContainer obstacleData = JsonSerializer.Deserialize<ObstacleContainer>(json);
            return obstacleData;
        }

    }
}