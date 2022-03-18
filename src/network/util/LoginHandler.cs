namespace network.util;

using network.protocol;
using network.model;
using network.model.util;
using db.util;
using db.dao;

public static partial class MsgHandler
{
    public static void MsgRegister(ClientState c, BaseMsg msgBase)
    {
        MsgRegister msg = (MsgRegister)msgBase;

        int flag = DbManager.Register(msg.id, msg.pw);
        if (flag == 0)
        {
            DbManager.RegisterPlayer(msg.id);
            msg.result = 0;
        }
        else 
        {
            msg.result = flag;
        }
        NetManager.Send(c, msg); // response
    }

    public static void MsgLogin(ClientState c, BaseMsg msgBase)
    {
        MsgLogin msg = (MsgLogin)msgBase;
        if (!DbManager.CheckIdAndPw(msg.id, msg.pw))
        {
            msg.result = 1;
            NetManager.Send(c, msg);
            return;
        }
        if (c.battlePlayer != null) // 通过此 socket 重复登录
        {
            msg.result = 1;
            NetManager.Send(c, msg);
            return;
        }

        if (BattlePlayerManager.IsOnline(msg.id)) // 已经通过另一 socket 登录了
        {
            // 发送踢下线协议
            BattlePlayer alreadyExistedBattlePlayer = BattlePlayerManager.GetPlayerById(msg.id)!;
            MsgKick msgKick = new MsgKick();
            msgKick.reason = 0;
            alreadyExistedBattlePlayer.SendToSocket(msgKick);
            NetManager.Close(alreadyExistedBattlePlayer.socketState);
        }

        // 获取玩家数据
        Player? playerData = DbManager.GetPlayerInfo(msg.id);
        if (playerData == null) // 居然查不到，不太可能
        {
            msg.result = 1;
            NetManager.Send(c, msg);
            return;
        }

        // 在登陆后，进入战场之前，预加载 BattlePlayer
        BattlePlayer battlePlayer = new BattlePlayer(c);
        battlePlayer.id = msg.id;
        battlePlayer.playerData = playerData;
        BattlePlayerManager.AddPlayer(msg.id, battlePlayer);
        c.battlePlayer = battlePlayer;

        // status = OK
        msg.result = 0;
        NetManager.Send(c, msg);
    }
}
