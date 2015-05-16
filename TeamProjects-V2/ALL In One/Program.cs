using System;

using LeagueSharp.Common;

namespace ALL_In_One
{
    class Program
    {
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            Initializer.initialize();
        }
    }
}