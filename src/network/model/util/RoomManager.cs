namespace network.model.util;

using System;
using System.Collections.Generic;

using network.model;
using network.protocol;

public static class RoomManager
{
    // 当前已创建的房间的最大id，非常简单的自动上升，不回收
    private static int curMaxRoomId = 1;
    // 房间键值对 {id: Room}
    public static Dictionary<int, Room> rooms = new Dictionary<int, Room>();

    //创建房间
    public static Room AddRoom()
    {
        curMaxRoomId++;
        Room room = new Room();
        room.id = curMaxRoomId;
        rooms.Add(room.id, room);
        return room;
    }

    public static bool RemoveRoom(int id)
    {
        rooms.Remove(id);
        return true;
    }

    public static Room? GetRoom(int id)
    {
        if (rooms.ContainsKey(id))
        {
            return rooms[id];
        }
        return null;
    }

    // 生成 MsgGetRoomList 协议
    public static BaseMsg GenerateGetRoomListMsg()
    {
        MsgGetRoomList msg = new MsgGetRoomList(rooms.Count);

        int i = 0;
        foreach (Room room in rooms.Values)
        {
            RoomInfo roomInfo = new RoomInfo();
            roomInfo.id = room.id;
            roomInfo.count = room.playerIds.Count;
            roomInfo.status = (int)room.status; // 枚举类型转 int

            msg.rooms![i] = roomInfo;
            i++;
        }
        return msg;
    }

    // 至多每秒调用一次，判断有战况的房间的获胜情况
    public static void Update()
    {
        foreach (Room room in rooms.Values)
        {
            room.Update();
        }
    }
}

