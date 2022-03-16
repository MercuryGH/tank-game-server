namespace network.util;

using network.model;
using network.protocol;

public partial class MsgHandler
{
    public static void MsgPing(ClientState c, BaseMsg msg)
    {
        Console.WriteLine("MsgPing");

        c.lastPingTime = NetManager.GetTimeStamp();
        MsgPong msgPong = new MsgPong();
        NetManager.Send(c, msgPong);
    }
}