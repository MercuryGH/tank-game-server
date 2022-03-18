namespace db.util;

using System;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using System.Text.Json;

using db.dao;

public static class DbManager
{
    public static MySqlConnection conn = new MySqlConnection();

    public static JsonSerializerOptions options = new(JsonSerializerDefaults.General)
    {
        IncludeFields = true
    };

    public static bool Connect(string db, string ip, int port, string user, string pw)
    {
        // 连接参数
        string s = string.Format("Database={0}; Data Source={1}; port={2}; User Id={3}; Password={4}",
                           db, ip, port, user, pw);
        conn.ConnectionString = s;

        try
        {
            conn.Open();
            Console.WriteLine("[DB] connection success!");

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("[DB] connection failed! " + e.Message);
            return false;
        }
    }

    private static void CheckAndReconnect()
    {
        try
        {
            if (conn.Ping() == true)
            {
                return;
            }
            conn.Close();
            conn.Open();
            Console.WriteLine("[DB] Reconnect!");
        }
        catch (Exception e)
        {
            Console.WriteLine("[DB] CheckAndReconnect failed " + e.Message);
        }
    }

    // 判定安全字符串
    private static bool IsSafeString(string str)
    {
        return !Regex.IsMatch(str, @"[-|;|,|\/|\(|\)|\[|\]|\}|\{|%|@|\*|!|\']");
    }

    public static bool ExistAccountId(string id)
    {
        CheckAndReconnect();
        if (!DbManager.IsSafeString(id))
        {
            return false;
        }

        string s = string.Format("select * from account where id = '{0}';", id);
        try
        {
            MySqlCommand cmd = new MySqlCommand(s, conn);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            bool hasRows = dataReader.HasRows;
            dataReader.Close();
            return hasRows;
        }
        catch (Exception e)
        {
            Console.WriteLine("[DB] ExistAccount err, " + e.Message);
            return false;
        }
    }

    public static int Register(string id, string pw)
    {
        CheckAndReconnect();
        if (!IsSafeString(id) || !IsSafeString(pw))
        {
            Console.WriteLine("[DB] Register failed, id or pw not safe");
            return 2;
        }
        if (ExistAccountId(id))
        {
            Console.WriteLine("[DB] Register failed, id exist");
            return 1;
        }

        string sql = string.Format("insert into account set id ='{0}', pw ='{1}';", id, pw);
        try
        {
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
            return 0;
        }
        catch (Exception e)
        {
            Console.WriteLine("[DB] Register failed " + e.Message);
            return 3;
        }
    }

    // 注册时调用，向 player 表中添加一条记录
    public static bool RegisterPlayer(string id)
    {
        CheckAndReconnect();
        if (!DbManager.IsSafeString(id))
        {
            Console.WriteLine("[DB] CreatePlayer failed, id not safe");
            return false;
        }

        // 确保 player 的 id 存在于 account （因此MySQL表声明中不必添加外键）
        if (ExistAccountId(id) == false)
        {
            Console.WriteLine("[DB] Register player failed, id not exist");
            return false;
        }

        Player playerData = new Player();
        string info = JsonSerializer.Serialize(playerData, options);
        string sql = string.Format("insert into player set id = '{0}', info = '{1}';", id, info);
        try
        {
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("[DB] CreatePlayer err, " + e.Message);
            return false;
        }
    }

    public static bool CheckIdAndPw(string id, string pw)
    {
        CheckAndReconnect();
        if (!DbManager.IsSafeString(id) || !DbManager.IsSafeString(pw))
        {
            Console.WriteLine("[DB] CheckPassword failed, id or pw not safe");
            return false;
        }

        string sql = string.Format("select * from account where id = '{0}' and pw = '{1}';", id, pw);
        try
        {
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            bool hasRows = dataReader.HasRows;
            dataReader.Close();
            return hasRows;
        }
        catch (Exception e)
        {
            Console.WriteLine("[DB] CheckPassword err, " + e.Message);
            return false;
        }
    }

    // 获取玩家数据
    public static Player? GetPlayerInfo(string id)
    {
        CheckAndReconnect();
        if (!DbManager.IsSafeString(id))
        {
            Console.WriteLine("[DB] GetPlayerData failed, id not safe");
            return null;
        }

        string sql = string.Format("select info from player where id = '{0}';", id);
        try
        {
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            if (!dataReader.HasRows)
            {
                dataReader.Close();
                return null;
            }
            dataReader.Read();
            string info = dataReader.GetString("info");

            // 反序列化
            Player playerData = JsonSerializer.Deserialize<Player>(info, options)!;

            dataReader.Close();
            return playerData;
        }
        catch (Exception e)
        {
            Console.WriteLine("[DB] GetPlayerData failed, " + e.Message);
            return null;
        }
    }

    // 更新玩家数据
    public static bool UpdatePlayerData(string id, Player playerData)
    {
        CheckAndReconnect();
        string info = JsonSerializer.Serialize(playerData, options);
        string sql = string.Format("update player set info = '{0}' where id = '{1}';", info, id);
        try
        {
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("[DB] UpdatePlayerData err, " + e.Message);
            return false;
        }
    }
}


