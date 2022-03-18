namespace network.util;

using network.model;
using network.model.util;
using network.protocol;

public static partial class MsgHandler
{
    // 查询战绩
    public static void MsgGetAchieve(ClientState c, BaseMsg msgBase)
    {
        MsgGetAchieve msg = (MsgGetAchieve)msgBase;
        BattlePlayer? player = c.battlePlayer;
        if (player == null)
        {
            return;
        }

        msg.win = player.playerData.win;
        msg.lose = player.playerData.lose;

        player.SendToSocket(msg);
    }

    // 获取房间信息
    public static void MsgGetRoomInfo(ClientState c, BaseMsg msgBase)
    {
        MsgGetRoomInfo msg = (MsgGetRoomInfo)msgBase;
        BattlePlayer? player = c.battlePlayer;
        if (player == null)
        {
            return;
        }

        Room? room = RoomManager.GetRoom(player.roomId);
        if (room == null)
        {
            return;
        }

        player.SendToSocket(room.GenerateGetRoomInfoMsg());
    }

    // 进入房间
    public static void MsgEnterRoom(ClientState c, BaseMsg msgBase)
    {
        MsgEnterRoom msg = (MsgEnterRoom)msgBase;
        BattlePlayer? player = c.battlePlayer;
        if (player == null)
        {
            return;
        }

        // 已经在房间里
        if (player.roomId >= 0)
        {
            msg.result = 1; // 进入失败
            player.SendToSocket(msg);
            return;
        }
        // 获取房间
        Room? room = RoomManager.GetRoom(msg.id);
        if (room == null)
        {
            msg.result = 1; // 找不到这个房间
            player.SendToSocket(msg);
            return;
        }
        // 进入
        bool flag = room.AddPlayer(player.id); // room.AddPlayer 会自动广播
        if (flag == false) // 某种原因导致加入失败
        {
            msg.result = 1;
            player.SendToSocket(msg);
            return;
        }
        msg.result = 0;
        player.SendToSocket(msg);
    }

    // 请求开始战斗
    public static void MsgStartBattle(ClientState c, BaseMsg msgBase)
    {
        MsgStartBattle msg = (MsgStartBattle)msgBase;
        BattlePlayer? player = c.battlePlayer;
        if (player == null)
        {
            return;
        }

        Room? room = RoomManager.GetRoom(player.roomId);
        if (room == null)
        {
            msg.result = 1;
            player.SendToSocket(msg);
            return;
        }

        if (!room.isOwner(player)) // 是否是房主
        {
            msg.result = 1;
            player.SendToSocket(msg);
            return;
        }

        bool flag = room.StartBattle();
        if (flag == false) // 开战失败
        {
            msg.result = 1;
            player.SendToSocket(msg);
            return;
        }

        msg.result = 0;
        player.SendToSocket(msg);
    }
}

