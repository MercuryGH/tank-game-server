using System;
using db.util;

namespace GameServer
{
    class GameServerEntry
    {
        public static void Main(string[] args)
        {
            if (!DbManager.Connect("tank_game", "127.0.0.1", 3306, "root", ""))
            {
                return;
            }
            Console.WriteLine("Test");

            // NetManager.StartLoop(8888);
        }
    }
}
