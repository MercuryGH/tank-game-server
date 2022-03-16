namespace network.protocol;

// 坦克信息
[System.Serializable]
public sealed class TankInfo
{
    public string id { get; set; } = "";  // 玩家id
    public int camp { get; set; } = 0;    // 阵营    TODO: modify name
    public int hp { get; set; } = 0;      // 生命值

    public float x { get; set; } = 0;     // 位置
    public float y { get; set; } = 0;
    public float z { get; set; } = 0;
    public float ex { get; set; } = 0;    // 旋转
    public float ey { get; set; } = 0;
    public float ez { get; set; } = 0;
}


// 进入战场
public sealed class MsgEnterBattle : BaseMsg
{
    public MsgEnterBattle() { protoName = "MsgEnterBattle"; }

    // push
    public TankInfo[]? tanks { get; set; }  // 初始化所有坦克的阵营、位置等
    public int mapId { get; set; } = 1;	 // 地图id
}

// 战斗结果
public sealed class MsgBattleResult : BaseMsg
{
    public MsgBattleResult() { protoName = "MsgBattleResult"; }

    // push
    public int winCamp { get; set; } = 0;	 // 获胜的阵营
}

// 玩家退出
public sealed class MsgLeaveBattle : BaseMsg
{
    public MsgLeaveBattle() { protoName = "MsgLeaveBattle"; }

    // push
    public string id { get; set; } = "";
}