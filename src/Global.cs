
using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GameLab
{

    public class Globals
    {
        public static float GetDistance(Vector2 pos, Vector2 target)
        {
            return (float)Math.Sqrt(Math.Pow(pos.X - target.X, 2) + Math.Pow(pos.Y - target.Y, 2));
        }

        public static Vector2 RadialMovement(Vector2 focus, Vector2 pos, float speed)
        {
            float dist = GetDistance(pos, focus);

            return dist <= speed ? focus - pos : (focus - pos) * speed / dist;
        }
        public static Rectangle NormalizeRectangle(Rectangle rect)
        {
            // If the width is negative, shift the X position by the width and take the absolute value of the width.
            if (rect.Width < 0)
            {
                rect.X += rect.Width;
                rect.Width = -rect.Width;
            }

            // If the height is negative, shift the Y position by the height and take the absolute value of the height.
            if (rect.Height < 0)
            {
                rect.Y += rect.Height;
                rect.Height = -rect.Height;
            }

            return rect;
        }
        public static float DistanceBetweenRectangles(Rectangle rectA, Rectangle rectB)
        {
            if (rectA.Intersects(rectB))
            {
                return 0;
            }

            // Horizontal distance
            int horizontalDistance = 0;
            if (rectA.Right < rectB.Left)
            {
                horizontalDistance = rectB.Left - rectA.Right;
            }
            else if (rectB.Right < rectA.Left)
            {
                horizontalDistance = rectA.Left - rectB.Right;
            }

            // Vertical distance
            int verticalDistance = 0;
            if (rectA.Bottom < rectB.Top)
            {
                verticalDistance = rectB.Top - rectA.Bottom;
            }
            else if (rectB.Bottom < rectA.Top)
            {
                verticalDistance = rectA.Top - rectB.Bottom;
            }
            return (float)Math.Sqrt((horizontalDistance * horizontalDistance) + (verticalDistance * verticalDistance));
        }
        public static Vector2 CalculateGlobalPosition(int roomIndex, int mapsize)
        {
            int mapSize = mapsize; // Assuming Map.MAPSIZE is accessible here
            int roomWidth = Map.BLOCKSIZE; // Assuming rooms are square and Map.BLOCKSIZE defines the size
            int rowIndex = roomIndex / mapSize;
            int columnIndex = roomIndex % mapSize;

            // Calculate center of the room as starting position
            float startX = (columnIndex * roomWidth) + (roomWidth / 2f);
            float startY = (rowIndex * roomWidth) + (roomWidth / 2f);

            return new Vector2(startX, startY);
        }

        public static bool AnyKeyOrButtonPressed(List<Keys> ks, List<Buttons> bs, KeyboardState newks, GamePadState newgs)
        {
            foreach (Keys k in ks)
            {
                if (newks.IsKeyDown(k))
                {
                    return true;
                }
            }
            foreach (Buttons b in bs)
            {
                if (newgs.IsButtonDown(b))
                {
                    return true;
                }
            }
            return false;

        }

        public static bool AnyKeyOrButtonNewlyPressed(List<Keys> ks, List<Buttons> bs, KeyboardState newks, KeyboardState oldks, GamePadState newgs, GamePadState oldgs)
        {
            foreach (Keys k in ks)
            {
                if (KeyNewlyPressed(k, newks, oldks))
                {
                    return true;
                }
            }
            foreach (Buttons b in bs)
            {
                if (GamePadNewlyPressed(b, newgs, oldgs))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool KeyNewlyPressed(Keys k, KeyboardState newks, KeyboardState oldks)
        {
            return newks.IsKeyDown(k) && oldks.IsKeyUp(k);

        }
        public static bool GamePadNewlyPressed(Buttons b, GamePadState newgs, GamePadState oldgs)
        {
            return newgs.IsButtonDown(b) && oldgs.IsButtonUp(b);
        }
        public static float GetDistanceBetweenRectangles(Rectangle rect1, Rectangle rect2)
        {
            int x1 = Math.Max(rect1.Left, rect2.Left);
            int x2 = Math.Min(rect1.Right, rect2.Right);
            int y1 = Math.Max(rect1.Top, rect2.Top);
            int y2 = Math.Min(rect1.Bottom, rect2.Bottom);

            int dx = (x2 > x1) ? 0 : Math.Min(rect1.Right - rect2.Left, rect2.Right - rect1.Left);
            int dy = (y2 > y1) ? 0 : Math.Min(rect1.Bottom - rect2.Top, rect2.Bottom - rect1.Top);

            return (float)Math.Sqrt((dx * dx) + (dy * dy));
        }
    }
}