namespace network.model;

using System;
using System.Collections.Generic;

using network.util;
using network.model.util;
using network.protocol;

public class Room
{
    private const int maxPlayer = 6; // 最大玩家数

    public int id = 0; // 房间 id
    public Dictionary<string, bool> playerIds { get; private set; } = new Dictionary<string, bool>(); // 玩家列表，其实是HashSet

    private string ownerId = ""; // 房主 id

    public enum Status // 房间状态
    {
        WAITING = 0, // 等待开始
        IN_BATTLE = 1, // 正在战斗
    }
    public Status status = Status.WAITING;

    // 硬编码出生点
    private static readonly float[,,] birthConfig = new float[2, 3, 6] {
		// 阵营1出生点
		{
            {262.3f, -8.0f, 342.7f, 0, -151.0f, 0f},
            {229.7f, -5.5f, 354.4f, 0, -164.2f, 0f},
            {197.1f, -3.6f, 347.7f, 0, -193.0f, 0f},
        },
		// 阵营2出生点
		{
            {-80.3f,  9.5f, 114.6f, 0, -294.0f,  0f},
            {-91.1f, 15.5f, 139.1f, 0, -294.2f, 0f},
            {-62.3f,  1.2f, 76.1f,  0, -315.4f, 0f},
        },
    };

    private bool isTrainingRoom = false; // 是否为训练房 (TODO: implements it)

    private long lastJudgeWinLoseTime = 0; // 上一次判断胜负的时间
    private const long JUDGE_WINLOSE_INTERVAL = 5; // 两次判断胜负的时间间隔

    // 添加玩家
    public bool AddPlayer(string id)
    {
        BattlePlayer? battlePlayer = BattlePlayerManager.GetPlayerById(id);
        if (battlePlayer == null)
        {
            Console.WriteLine("room.AddPlayer failed, player is null");
            return false;
        }
        if (playerIds.Count >= maxPlayer)
        {
            Console.WriteLine("room.AddPlayer failed, reach maxPlayer");
            return false;
        }
        if (status != Status.WAITING)
        {
            Console.WriteLine("room.AddPlayer failed, room status is not WAITING");
            return false;
        }
        if (playerIds.ContainsKey(id) == true)
        {
            Console.WriteLine("room.AddPlayer fail, already in this room");
            return false;
        }
        // 自动分配阵营，设置房间id
        battlePlayer.team = AutoSetTeam();
        battlePlayer.roomId = this.id;

        // 设置房主
        if (ownerId == "")
        {
            ownerId = battlePlayer.id;
        }

        // 加入玩家集合
        // TODO: 并发控制（可能在同一时间有很多人加入房间）
        playerIds[id] = true; // playerIds.insert(id);

        BaseMsg broadCastedRoomInfo = GenerateGetRoomInfoMsg();
        BroadcastExceptPlayer(broadCastedRoomInfo, id);

        return true;
    }

    // 自动分配阵营
    public int AutoSetTeam()
    {
        int count1 = 0;
        int count2 = 0;
        foreach (string id in playerIds.Keys)
        {
            BattlePlayer player = BattlePlayerManager.GetPlayerById(id)!;
            if (player.team == 1) { count1++; }
            if (player.team == 2) { count2++; }
        }

        Console.WriteLine("DEBUG: " + count1 + " " + count2);

        // 选择人数少的阵营。若人数一致，则选择阵营1
        if (count1 <= count2)
        {
            return 1;
        }
        else
        {
            return 2;
        }
    }

    // 判断传入的 BattlePlayer 是不是房主
    public bool isOwner(BattlePlayer battlePlayer)
    {
        return battlePlayer.id == ownerId;
    }

    // 删除玩家
    public bool RemovePlayer(string id)
    {
        // 获取玩家
        BattlePlayer? player = BattlePlayerManager.GetPlayerById(id);
        if (player == null)
        {
            Console.WriteLine("room.RemovePlayer fail, player is null");
            return false;
        }
        if (!playerIds.ContainsKey(id))
        {
            Console.WriteLine("room.RemovePlayer fail, not in this room");
            return false;
        }

        playerIds.Remove(id); // playerIds.remove(id)
        player.team = 0;
        player.roomId = -1;

        // 自动设置房主
        if (ownerId == player.id)
        {
            ownerId = AutoSwitchOwner();
        }

        // 退出后，房间为空
        if (playerIds.Count == 0)
        {
            RoomManager.RemoveRoom(this.id);
        }

        // 战斗状态退出
        if (status == Status.IN_BATTLE)
        {
            player.playerData.lose++;
            MsgLeaveBattle msg = new MsgLeaveBattle();
            msg.id = player.id;
            Broadcast(msg);
        }
        else // 非状态战斗退出
        {
            // 对除了退出者的全体玩家广播
            BaseMsg broadCastedRoomInfo = GenerateGetRoomInfoMsg();
            BroadcastExceptPlayer(broadCastedRoomInfo, id);
        }

        return true;
    }

    // 选择房主
    public string AutoSwitchOwner()
    {
        // 选择第一个玩家
        foreach (string id in playerIds.Keys)
        {
            return id;
        }
        // 房间没人
        return "";
    }


    // 广播消息
    public void Broadcast(BaseMsg msg)
    {
        foreach (string id in playerIds.Keys)
        {
            BattlePlayer battlePlayer = BattlePlayerManager.GetPlayerById(id)!;
            battlePlayer.SendToSocket(msg);
        }
    }

    // 除了 exceptId，广播消息
    public void BroadcastExceptPlayer(BaseMsg msg, string exceptId)
    {
        foreach (string id in playerIds.Keys)
        {
            if (id == exceptId)
            {
                continue;
            }
            BattlePlayer battlePlayer = BattlePlayerManager.GetPlayerById(id)!;
            battlePlayer.SendToSocket(msg);
        }
    }

    // 生成 MsgGetRoomInfo 协议
    public BaseMsg GenerateGetRoomInfoMsg()
    {
        MsgGetRoomInfo msg = new MsgGetRoomInfo(playerIds.Count);

        int i = 0;
        foreach (string id in playerIds.Keys)
        {
            BattlePlayer battlePlayer = BattlePlayerManager.GetPlayerById(id)!;

            PlayerInfo playerInfo = new PlayerInfo();
            playerInfo.id = battlePlayer.id;
            playerInfo.team = battlePlayer.team;
            playerInfo.win = battlePlayer.playerData.win;
            playerInfo.lose = battlePlayer.playerData.lose;
            playerInfo.isOwner = 0;
            if (isOwner(battlePlayer))
            {
                playerInfo.isOwner = 1;
            }

            msg.players![i] = playerInfo;
            i++;
        }
        return msg;
    }

    // 能否开战
    private bool CanStartBattle()
    {
        // 已经是战斗状态
        if (status == Status.IN_BATTLE)
        {
            return false;
        }

        // 统计每个队伍的玩家数
        int count1 = 0;
        int count2 = 0;
        foreach (string id in playerIds.Keys)
        {
            BattlePlayer player = BattlePlayerManager.GetPlayerById(id)!;
            if (player.team == 1) { count1++; }
            else { count2++; }
        }
        //每个队伍至少要有1名玩家
        if (count1 < 1 || count2 < 1)
        {
            isTrainingRoom = true;
            return false;
        }
        isTrainingRoom = false;
        return true;
    }

    // 根据传来的玩家索引 index，初始化 battlePlayer 的位置（出生点）
    private void SetBirthPos(BattlePlayer battlePlayer, int index)
    {
        int camp = battlePlayer.team;

        battlePlayer.x = birthConfig[camp - 1, index, 0];
        battlePlayer.y = birthConfig[camp - 1, index, 1];
        battlePlayer.z = birthConfig[camp - 1, index, 2];
        battlePlayer.ex = birthConfig[camp - 1, index, 3];
        battlePlayer.ey = birthConfig[camp - 1, index, 4];
        battlePlayer.ez = birthConfig[camp - 1, index, 5];
    }

    // 玩家数据转成TankInfo，便于发送数据包
    private TankInfo PlayerToTankInfo(BattlePlayer player)
    {
        TankInfo tankInfo = new TankInfo();
        tankInfo.team = player.team;
        tankInfo.id = player.id;
        tankInfo.hp = player.hp;

        tankInfo.x = player.x;
        tankInfo.y = player.y;
        tankInfo.z = player.z;
        tankInfo.ex = player.ex;
        tankInfo.ey = player.ey;
        tankInfo.ez = player.ez;

        return tankInfo;
    }

    // 重置玩家战斗属性
    private void ResetPlayers()
    {
        //位置和旋转
        int count1 = 0;
        int count2 = 0;
        foreach (string id in playerIds.Keys)
        {
            BattlePlayer player = BattlePlayerManager.GetPlayerById(id)!;
            if (player.team == 1)
            {
                SetBirthPos(player, count1);
                count1++;
            }
            else
            {
                SetBirthPos(player, count2);
                count2++;
            }
            player.hp = 100;
        }
    }

    // 开战，并广播 MsgEnterBattle
    public bool StartBattle()
    {
        if (CanStartBattle() == false)
        {
            return false;
        }

        status = Status.IN_BATTLE;

        ResetPlayers();

        const int mapId = 1; // 目前只有一个地图，硬编码mapId

        BaseMsg msg = GenerateEnterBattleMsg(mapId);
        Broadcast(msg);
        return true;
    }

    private BaseMsg GenerateEnterBattleMsg(int mapId)
    {
        MsgEnterBattle msg = new MsgEnterBattle(playerIds.Count, 1);

        // 组装数据包
        int i = 0;
        foreach (string id in playerIds.Keys)
        {
            BattlePlayer player = BattlePlayerManager.GetPlayerById(id)!;
            msg.tanks[i] = PlayerToTankInfo(player);
            i++;
        }
        return msg;
    }

    public bool IsDie(BattlePlayer player)
    {
        return player.hp <= 0;
    }

    // 定时更新
    public void Update()
    {
        if (status != Status.IN_BATTLE)
        {
            return;
        }

        // 两次更新的时间间隔至少是 5s，避免服务端压力过大
        if (NetManager.GetTimeStamp() - lastJudgeWinLoseTime < JUDGE_WINLOSE_INTERVAL)
        {
            return;
        }
        lastJudgeWinLoseTime = NetManager.GetTimeStamp();

        // 胜负判断
        int winCamp = JudgeWinLose();
        // 尚未分出胜负
        if (winCamp == 0)
        {
            return;
        }

        // 某一方胜利，结束战斗，房间变为等待状态
        status = Status.WAITING;
        // 统计信息
        foreach (string id in playerIds.Keys)
        {
            BattlePlayer player = BattlePlayerManager.GetPlayerById(id)!;
            if (player.team == winCamp) { player.playerData.win++; }
            else { player.playerData.lose++; }
        }

        // 发送 Result
        MsgBattleResult msg = new MsgBattleResult();
        msg.winTeam = winCamp;
        Broadcast(msg);
    }

    /**
     * 定时调用的胜负判断
     * @return 0 if 还未分出胜负; 1 if team1 胜利; 2 if team2 胜利 
     */
    private int JudgeWinLose()
    {
        // 存活人数
        int count1 = 0;
        int count2 = 0;
        foreach (string id in playerIds.Keys)
        {
            BattlePlayer player = BattlePlayerManager.GetPlayerById(id)!;
            if (!IsDie(player))
            {
                if (player.team == 1) { count1++; };
                if (player.team == 2) { count2++; };
            }
        }

        if (count1 <= 0)
        {
            return 2;
        }
        else if (count2 <= 0)
        {
            return 1;
        }
        return 0;
    }
}

