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
        msg.lost = player.playerData.lose;

        player.SendToSocket(msg);
    }

    // 请求房间列表
    public static void MsgGetRoomList(ClientState c, BaseMsg msgBase)
    {
        MsgGetRoomList msg = (MsgGetRoomList)msgBase;
        BattlePlayer? player = c.battlePlayer;
        if (player == null)
        {
            return;
        }

        player.SendToSocket(RoomManager.GenerateGetRoomListMsg());
    }

    // 创建房间
    public static void MsgCreateRoom(ClientState c, BaseMsg msgBase)
    {
        MsgCreateRoom msg = (MsgCreateRoom)msgBase;
        BattlePlayer? player = c.battlePlayer;
        if (player == null)
        {
            return;
        }

        // 已经在房间里
        if (player.roomId >= 0)
        {
            msg.result = 1; // 创建失败
            player.SendToSocket(msg);
            return;
        }
        Room room = RoomManager.AddRoom();
        room.AddPlayer(player.id);

        msg.result = 0;
        player.SendToSocket(msg);
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
}

