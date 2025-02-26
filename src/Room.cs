using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
namespace GameLab
{

    public class Room
    {
        public string Id { get; set; }
        public bool Active { get; set; }
        public bool Mirror { get; set; }

        public Dictionary<string, bool> Doors { get; set; }
        public Dictionary<string, bool[]> Location { get; set; }
        public List<Rectangle> WallBorders { get; set; } = [];
        public List<Rectangle> DoorAreas { get; set; } = [];
        public bool IsRoom()
        {
            return Id.StartsWith("room");
        }
        public bool IsCorridor()
        {
            return Id.StartsWith("corr");
        }
        public bool IsOpen()
        {
            return Id.StartsWith("hall");
        }
        private string GenerateMirroredId()
        {
            return Id + "_mirrored";
        }

        // Generate a new ID for rotated versions
        private string GenerateRotatedId(int rotationDegrees)
        {
            return Id + "_rot" + rotationDegrees;
        }
        public Room GetMirroredVersion()
        {
            Room mirroredRoom = new()
            {
                Id = GenerateMirroredId(),
                Active = Active,
                Doors = [],
                Location = []
            };

            foreach (KeyValuePair<string, bool> door in Doors)
            {
                string newKey = door.Key switch
                {
                    "west" => "east",
                    "east" => "west",
                    _ => door.Key
                };
                mirroredRoom.Doors[newKey] = door.Value;
                bool[] reversedLocations = (bool[])Location[door.Key].Clone();
                Array.Reverse(reversedLocations);
                mirroredRoom.Location[newKey] = reversedLocations;
            }
            return mirroredRoom;
        }
        public Room GetRotatedVersion(int rotationIndex)
        {
            Room rotatedRoom = new()
            {
                Id = GenerateRotatedId(rotationIndex * 90),
                Active = Active,
                Doors = [],
                Location = []
            };

            foreach (string side in Doors.Keys)
            {
                // Calculate the new direction based on the rotation
                string newSide = RotateDirection(side, rotationIndex * 90);

                // Apply the rotated direction to the new room
                rotatedRoom.Doors[newSide] = Doors[side];
                rotatedRoom.Location[newSide] = Location[side];
            }
            return rotatedRoom;
        }

        private static string RotateDirection(string currentDirection, int rotationDegrees)
        {
            List<string> directions = ["north", "east", "south", "west"];
            int currentIndex = directions.IndexOf(currentDirection);
            if (currentIndex == -1)
            {
                throw new ArgumentException("Invalid direction");
            }

            // Calculate the number of steps to rotate based on 90-degree increments

            int steps = rotationDegrees / 90;
            int newIndex = (currentIndex + steps) % directions.Count;
            return directions[newIndex];
        }

    }

}
