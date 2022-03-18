namespace network.model;

using System;
using db.dao;
using protocol;

public class BattlePlayer
{
    public string id = "";
    
    public ClientState socketState; // 指向 ClientState

    public BattlePlayer(ClientState state)
    {
        this.socketState = state;
		this.playerData = new Player();
    }

    // 坐标和旋转
    public float x;
    public float y;
    public float z;
    public float ex;
    public float ey;
    public float ez;

    
    public int roomId = -1; // 所在房间的 id。若值为 -1，则不在房间里
    public int team = 1; // 阵营
    public int hp = 100; // 坦克生命值

    // 指向该 BattlePlayer 的 dao
    public Player playerData;

    // 通过 BattlePlayer 向 socket 发送信息
    public void SendToSocket(BaseMsg msgBase)
    {
        NetManager.Send(socketState, msgBase);
    }
}
