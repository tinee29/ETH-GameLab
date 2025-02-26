using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameLab
{

    public class Block
    {
        public Texture2D CorridorTexture;
        public Rectangle SourceRectangle { get; set; }
        public Rectangle TopRectangle { get; set; }
        public Rectangle BottomRectangle { get; set; }
        public Rectangle LeftRectangle { get; set; }
        public Rectangle RightRectangle { get; set; }
        public Room Room { get; set; }
        public bool IsMirrored { get; set; }
        public bool IsStart { get; set; }
        public bool IsTarget { get; set; }
        public int Probability { get; set; }
        // Additional properties like doors and location can be added here
    }
}
