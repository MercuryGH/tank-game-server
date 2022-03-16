namespace network.protocol;

// 同步坦克信息
public sealed class MsgSyncTank : BaseMsg
{
    public MsgSyncTank() { protoName = "MsgSyncTank"; }

    // sync 位置、旋转、炮塔旋转
    public float x { get; set; }  = 0f;
    public float y { get; set; } = 0f;
    public float z { get; set; } = 0f;
    public float ex { get; set; } = 0f;
    public float ey { get; set; } = 0f;
    public float ez { get; set; } = 0f;
    public float turretY { get; set; } = 0f;
    public float gunX { get; set; } = 0f;

    // broadcast
    public string id { get; set; } = ""; // 服务端广播时，补充发送 MsgSyncTank 的玩家id
}

// 开火
public sealed class MsgFire : BaseMsg
{
    public MsgFire() { protoName = "MsgFire"; }

    // sync 炮弹初始位置、旋转
    public float x { get; set; } = 0f;
    public float y { get; set; } = 0f;
    public float z { get; set; } = 0f;
    public float ex { get; set; } = 0f;
    public float ey { get; set; } = 0f;
    public float ez { get; set; } = 0f;

    // broadcast
    public string id { get; set; } = "";
}

// 击中（由shooter发送，可以作弊）
public sealed class MsgHit : BaseMsg
{
    public MsgHit() { protoName = "MsgHit"; }

    // sync
    public string targetId { get; set; } = ""; 
    // 击中点（该信息可以用于服务端计算伤害、辅助反作弊等）
    public float x { get; set; } = 0f;
    public float y { get; set; } = 0f;
    public float z { get; set; } = 0f;

    // broadcast
    public string id { get; set; } = ""; // shooter id
    public int hp { get; set; } = 0;     // target HP before being shooted
    public int damage { get; set; } = 0; // hit damage
}