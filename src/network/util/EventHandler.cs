namespace network.util;

using network.model;

using System;

using db.util;
using network.model.util;
using network.protocol;
using network;

public static partial class EventHandler
{
    public static void OnDisconnect(ClientState c)
    {
        Console.WriteLine("Close");

        // 当前玩家正处于战斗中
        if (c.battlePlayer != null)
        {
            // 查找所在房间，并让其离开战场
            int roomId = c.battlePlayer.roomId;
            if (roomId >= 0)
            {
                Room? room = RoomManager.GetRoom(roomId);
                if (room != null)
                    room.RemovePlayer(c.battlePlayer.id);
            }

            // 保存数据
            DbManager.UpdatePlayerData(c.battlePlayer.id, c.battlePlayer.playerData);

            // 移除
            BattlePlayerManager.RemovePlayer(c.battlePlayer.id);
        }
    }

    // 定时任务，由 NetManager 至多每秒调用一次
    public static void OnTimer()
    {
        CheckPing();
        RoomManager.Update(); // 调用 RoomManager 的更新
    }

    // Ping检查
    public static void CheckPing()
    {
        long timeNow = NetManager.GetTimeStamp();

        // 删除掉线玩家 TODO: BAD: 每次调用最多删除一个，修改成 .filter()
        foreach (ClientState s in NetManager.clients.Values)
        {
            if (timeNow - s.lastPingTime > NetManager.pingInterval)
            {
                Console.WriteLine("Ping Close " + s.socket.RemoteEndPoint!.ToString());

                // 发送特殊的踢下线协议
                MsgKick msgKick = new MsgKick();
                msgKick.reason = 3;
                NetManager.Send(s, msgKick);

                NetManager.Close(s);
                return;
            }
        }
    }
}

