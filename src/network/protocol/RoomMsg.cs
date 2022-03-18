namespace network.protocol;

[System.Serializable]
public sealed class PlayerInfo
{
    public string id = "test";  // 账号
    public int team = 0;        // 阵营
    public int win = 0;         // 胜利数
    public int lose = 0;        // 失败数
    public int isOwner = 0;		// 是否是房主
}

// 查询当前所在的房间信息
public sealed class MsgGetRoomInfo : BaseMsg
{
    public MsgGetRoomInfo(int playerCnt)
    {
        protoName = "MsgGetRoomInfo";
        players = new PlayerInfo[playerCnt];
    }

    public MsgGetRoomInfo()
    {
        protoName = "MsgGetRoomInfo";
    }

    // response
    public PlayerInfo[]? players;
}

// 离开房间
public sealed class MsgLeaveRoom : BaseMsg
{
    public MsgLeaveRoom() { protoName = "MsgLeaveRoom"; }

    // response status code
    public int result = 0;
}

// 开战
public sealed class MsgStartBattle : BaseMsg
{
    public MsgStartBattle() { protoName = "MsgStartBattle"; }

    // response status code: success: 0, other: 1
    public int result = 0;
}