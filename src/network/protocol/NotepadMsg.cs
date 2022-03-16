namespace network.protocol;
public sealed class MsgGetText : BaseMsg
{
    public MsgGetText() { protoName = "MsgGetText"; }

    // push
    public string text { get; set; } = "";
}

public sealed class MsgSaveText : BaseMsg
{
    public MsgSaveText() { protoName = "MsgSaveText"; }

    // request
    public string text { get; set; } = "";

    // response status code （0-成功 1-文字太长）
    public int result { get; set; } = 0;
}
