using System;

namespace GameLab
{
    public static class Program
    {
        [STAThread]
        private static void Main()
        {
            using GameLabGame game = new();
            game.Run();
        }
    }
}
