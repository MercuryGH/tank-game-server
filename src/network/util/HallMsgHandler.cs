namespace network.util;

using network.model;
using network.model.util;
using network.protocol;

public static partial class MsgHandler
{
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
            player.SendToSocket(msg); // send null
            return;
        }

        player.SendToSocket(room.GenerateGetRoomInfoMsg());
    }

    // 离开房间
    public static void MsgLeaveRoom(ClientState c, BaseMsg msgBase)
    {
        MsgLeaveRoom msg = (MsgLeaveRoom)msgBase;
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

        room.RemovePlayer(player.id);
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