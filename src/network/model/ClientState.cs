namespace network.model;

using System.Net.Sockets;
using network.util;

public sealed class ClientState
{
    public Socket socket;
    public ByteBuffer readBuff = new ByteBuffer();
    public long lastPingTime = 0;

    // 注意：登陆后就会立即创建 BattlePlayer（预加载），未登录则不会有
    public BattlePlayer? battlePlayer; // 是否对应了一个 BattlePlayer，有则非null


    public ClientState(Socket socket, long lastPingTime)
    {
        this.socket = socket;
        this.lastPingTime = lastPingTime;
    }
}