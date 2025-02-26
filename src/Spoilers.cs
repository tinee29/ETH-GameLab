using System;

namespace GameLab
{

    public class Spoilers
    {
        private static readonly string[] SpoilerTexts = ["Dumbledore dies", "Dan is Gossip Girl", "Darth Vader is Lukes' Father", "Thanos wipes out half the Universe", "Tyler Durden in Fight Club isn't real", "Mufasa dies", "Tony Stark dies in Endgame", "Snape kills Dumbledore", "Atreus is Loki", "Sheik is actually Zelda", "The Princess is in another castle", "The cake is a lie", "Joel dies"];
        private static readonly int NumSpoilers = SpoilerTexts.Length;
        private static readonly Random Random = new();

        private static int s_currentSpoiler = Random.Next() % NumSpoilers;

        public static string GetSpoiler()
        {
            return SpoilerTexts[s_currentSpoiler] + "!";
        }
        public static void NewSpoiler()
        {
            s_currentSpoiler = Random.Next() % NumSpoilers;

        }
    }
}