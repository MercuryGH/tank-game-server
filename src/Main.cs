using System;

using db.dao;
using db.util;
using network;
using network.protocol;

namespace GameServer
{
    public class GameServerEntry
    {
        private const int PORT = 6666; // bind port = 6666

        public static void Main(string[] args)
        {
            bool flag = DbManager.Connect("tank_game", "127.0.0.1", 3306, "root", "");
            if (flag == false)
            {
                return;
            }
            // bool status = DbManager.Register("testId", "1234");
            // TestDb();

            NetManager.StartLoop(PORT);
        }

        private static void TestDb()
        {
            // bool status = DbManager.RegisterPlayer("testId");
            // Player player = DbManager.GetPlayerInfo("testId");
            // Console.WriteLine(player.coin);
            // Console.WriteLine(status);
        }

        private static void TestMsgMove()
        {
            // MsgMove msgMove = (MsgMove)msgBase;
            // Console.WriteLine(msgMove.x);
            // msgMove.x++;
            // NetManager.Send(c, msgMove);
        }

        private static void TestSerialize()
        {
            // 测试JSON序列化反序列化
            // MsgSyncTank msgSyncTank = new MsgSyncTank();
            // msgSyncTank.x = 100;
            // msgSyncTank.y = -20;
            // byte[] bytes = BaseMsg.Encode(msgSyncTank);

            // MethodInfo decodeMethod = typeof(BaseMsg).GetMethod(nameof(BaseMsg.Decode))!;
            // MethodInfo generic = decodeMethod.MakeGenericMethod(Type.GetType("network.protocol.MsgSyncTank")!);
            // BaseMsg msg = (BaseMsg)generic.Invoke(null, new object[] { bytes, 0, bytes.Length })!;

            // // BaseMsg msg = BaseMsg.Decode<MsgSyncTank>(bytes, 0, bytes.Length);
            // byte[] secondBytes = BaseMsg.Encode(msg);

            // string s = System.Text.Encoding.UTF8.GetString(bytes);
            // Console.WriteLine(s);
            // string t = System.Text.Encoding.UTF8.GetString(secondBytes);
            // Console.WriteLine(t);
        }
    }
}
