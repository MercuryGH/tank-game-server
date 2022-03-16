namespace network.protocol;

public sealed class MsgRegister : BaseMsg
{
    public MsgRegister() { protoName = "MsgRegister"; }

    // request
    public string id { get; set; } = "";
    public string pw { get; set; } = "";

    // response status code（0-成功，1-失败）
    public int result { get; set; } = 0;
}

public sealed class MsgLogin : BaseMsg
{
    public MsgLogin() { protoName = "MsgLogin"; }

    // request
    public string id { get; set; } = "";
    public string pw { get; set; } = "";

    // response status code （0-成功，1-失败）
    public int result { get; set; } = 0;
}

public sealed class MsgKick : BaseMsg
{
    public MsgKick() { protoName = "MsgKick"; }

    // push 被踢原因（0-其他人登陆同一账号）
    public int reason { get; set; } = 0;
}