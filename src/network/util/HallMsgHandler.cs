namespace network.util;

using network.model;
using network.model.util;
using network.protocol;

public static partial class MsgHandler
{
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
        bool flag = room.AddPlayer(player.id);
        if (flag == false)
        {
            msg.result = 1; // 创建失败
            player.SendToSocket(msg);
            return;
        }

        msg.result = 0;
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
}