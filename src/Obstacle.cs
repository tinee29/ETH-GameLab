using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Vector2 = Microsoft.Xna.Framework.Vector2;
namespace GameLab
{
    public class Obstacle
    {
        public Texture2D Texture { get; set; }
        public Rectangle SourceRectangleBottom { get; set; }
        public Rectangle SourceRectangleTop { get; set; }
        public Vector2 ShapeBottom { get; set; }
        public Vector2 ShapeTop { get; set; }
        public Vector2 PositionBottom { get; set; }
        public Vector2 PositionTop { get; set; }
    }
}