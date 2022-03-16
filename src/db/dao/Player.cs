namespace db.dao;

public sealed class Player
{
    //金币
    public int coin { get; set; } = 0;
    //记事本
    public string text { get; set; } = "New player default text";
    //胜利数
    public int win { get; set; } = 0;
    //失败数
    public int lose { get; set; } = 0;

    // public Player()
    // {
    // }
}