namespace network.model.util;

using System.Collections.Generic;

using network.model;

public static class BattlePlayerManager
{
    // {playerId: BattlePlayer}
    static Dictionary<string, BattlePlayer> players = new Dictionary<string, BattlePlayer>();

    public static bool IsOnline(string id)
    {
        return players.ContainsKey(id);
    }

    public static BattlePlayer? GetPlayerById(string id)
    {
        if (players.ContainsKey(id))
        {
            return players[id];
        }
        return null;
    }

    public static void AddPlayer(string id, BattlePlayer player)
    {
        players.Add(id, player);
    }

    public static void RemovePlayer(string id)
    {
        players.Remove(id);
    }
}
