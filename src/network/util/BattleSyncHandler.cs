namespace network.util;

using network.model;
using network.model.util;
using network.protocol;

public static partial class MsgHandler
{

    private static bool FindIfCheatMovement(BattlePlayer battlePlayer, MsgSyncTank msg)
    {
        if (Math.Abs(battlePlayer.x - msg.x) > 5 ||
    Math.Abs(battlePlayer.y - msg.y) > 5 ||
    Math.Abs(battlePlayer.z - msg.z) > 5)
        {
            return true;
        }
        return false;
    }

    // 同步位置协议
    public static void MsgSyncTank(ClientState c, BaseMsg msgBase)
    {
        MsgSyncTank msg = (MsgSyncTank)msgBase;
        BattlePlayer? battlePlayer = c.battlePlayer;
        if (battlePlayer == null)
        {
            return;
        }

        Room? room = RoomManager.GetRoom(battlePlayer.roomId);
        if (room == null)
        {
            return;
        }

        if (room.status != Room.Status.IN_BATTLE)
        {
            return;
        }

        // 检测是否作弊（粗略）
        bool cheatFlag = FindIfCheatMovement(battlePlayer, msg);
        if (cheatFlag == true)
        {
            Console.WriteLine("Cheat detected: " + battlePlayer.id);
        }

        // 更新信息
        battlePlayer.x = msg.x;
        battlePlayer.y = msg.y;
        battlePlayer.z = msg.z;
        battlePlayer.ex = msg.ex;
        battlePlayer.ey = msg.ey;
        battlePlayer.ez = msg.ez;

        msg.id = battlePlayer.id;
        room.Broadcast(msg);
    }

    // 开火协议
    public static void MsgFire(ClientState c, BaseMsg msgBase)
    {
        MsgFire msg = (MsgFire)msgBase;
        BattlePlayer? player = c.battlePlayer;
        if (player == null) 
        {
            return;
        }

        Room? room = RoomManager.GetRoom(player.roomId);
        if (room == null)
        {
            return;
        }
        if (room.status != Room.Status.IN_BATTLE)
        {
            return;
        }

        msg.id = player.id;
        room.Broadcast(msg);
    }

    // 击中协议
    public static void MsgHit(ClientState c, BaseMsg msgBase)
    {
        MsgHit msg = (MsgHit)msgBase;
        BattlePlayer? shooter = c.battlePlayer;
        if (shooter == null) 
        {
            return;
        }

        BattlePlayer? shootee = BattlePlayerManager.GetPlayerById(msg.targetId);
        if (shootee == null)
        {
            return;
        }

        Room? room = RoomManager.GetRoom(shooter.roomId);
        if (room == null)
        {
            return;
        }

        if (room.status != Room.Status.IN_BATTLE)
        {
            return;
        }
        // 发送者校验 + 在同一房间校验
        if (shooter.id != msg.id || shooter.roomId != shootee.roomId)
        {
            return;
        }

        // 状态
        int damage = 35; // 硬编码伤害，未进行计算
        shootee.hp -= damage;

        // 广播
        msg.id = shooter.id;
        msg.hp = shooter.hp;
        msg.damage = damage;
        room.Broadcast(msg);
    }
}


