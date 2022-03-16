namespace network.protocol;

// 玩家信息数据结构 TODO: modfiy field name
[System.Serializable]
public sealed class PlayerInfo
{
    public string id { get; set; } = "test";  // 账号
    public int team { get; set; } = 0;        // 阵营
    public int win { get; set; } = 0;         // 胜利数
    public int lost { get; set; } = 0;        // 失败数
    public int isOwner { get; set; } = 0;		// 是否是房主
}

// 查询当前所在的房间信息
public sealed class MsgGetRoomInfo : BaseMsg
{
    public MsgGetRoomInfo() { protoName = "MsgGetRoomInfo"; }

    // response
    public PlayerInfo[]? players { get; set; }
}

// 离开房间
public sealed class MsgLeaveRoom : BaseMsg
{
    public MsgLeaveRoom() { protoName = "MsgLeaveRoom"; }

    // response status code
    public int result { get; set; } = 0;
}

// 开战
public sealed class MsgStartBattle : BaseMsg
{
    public MsgStartBattle() { protoName = "MsgStartBattle"; }

    // response status code: success: 0, other: 1
    public int result { get; set; } = 0;
}