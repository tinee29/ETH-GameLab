
using System.IO;
using System.Collections.Generic;
using System;
using System.Text.Json;


namespace GameLab
{

    public class RoomData
    {
        public List<Room> Rooms { get; set; }

        public static RoomData LoadRoomData(string jsonFile)
        {
            string bin_path = Path.GetDirectoryName(Environment.ProcessPath);
            string path = Path.Combine(bin_path, "Content", jsonFile);
            using StreamReader r = new(path);
            string json = r.ReadToEnd();
            RoomData roomData = JsonSerializer.Deserialize<RoomData>(json);
            return roomData;
        }
    }
}