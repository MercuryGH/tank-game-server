namespace db.util;

using System;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using System.Text.Json;
using db.dao;

public class DbManager
{
    public static MySqlConnection conn;

    public static bool Connect(string db, string ip, int port, string user, string pw)
    {
        conn = new MySqlConnection();
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
            Console.WriteLine("[DB] CheckAndReconnect fail " + e.Message);
        }
    }

    // 判定安全字符串
    private static bool IsSafeString(string str)
    {
        return !Regex.IsMatch(str, @"[-|;|,|\/|\(|\)|\[|\]|\}|\{|%|@|\*|!|\']");
    }

    //是否存在该用户
    public static bool ExistAccount(string id)
    {
        CheckAndReconnect();
        if (!DbManager.IsSafeString(id))
        {
            return false;
        }

        //sql语句
        string s = string.Format("select * from account where id = '{0}';", id);
        //查询
        try
        {
            MySqlCommand cmd = new MySqlCommand(s, conn);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            bool hasRows = dataReader.HasRows;
            dataReader.Close();
            return !hasRows;
        }
        catch (Exception e)
        {
            Console.WriteLine("[DB] ExistAccount err, " + e.Message);
            return false;
        }
    }

    public static bool Register(string id, string pw)
    {
        CheckAndReconnect();
        if (!IsSafeString(id) || !IsSafeString(pw))
        {
            Console.WriteLine("[DB] Register fail, id or pw not safe");
            return false;
        }

        if (!ExistAccount(id))
        {
            Console.WriteLine("[DB] Register fail, id exist");
            return false;
        }
        
        string sql = string.Format("insert into account set id ='{0}', pw ='{1}';", id, pw);
        try
        {
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("[DB] Register failed " + e.Message);
            return false;
        }
    }

    public static bool CreatePlayer(string id)
    {
        CheckAndReconnect();
        //防sql注入
        if (!DbManager.IsSafeString(id))
        {
            Console.WriteLine("[DB] CreatePlayer fail, id not safe");
            return false;
        }
        //序列化
        // PlayerData playerData = new PlayerData();
        // string data = Js.Serialize(playerData);
        //写入DB
        // string sql = string.Format("insert into player set id ='{0}' ,data ='{1}';", id, data);
        string sql = "123";
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


    //检测用户名密码
    public static bool CheckPassword(string id, string pw)
    {
        CheckAndReconnect();
        //防sql注入
        if (!DbManager.IsSafeString(id))
        {
            Console.WriteLine("[DB] CheckPassword fail, id not safe");
            return false;
        }
        if (!DbManager.IsSafeString(pw))
        {
            Console.WriteLine("[DB] CheckPassword fail, pw not safe");
            return false;
        }
        //查询
        string sql = string.Format("select * from account where id='{0}' and pw='{1}';", id, pw);

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


    //获取玩家数据
    public static Player GetPlayerData(string id)
    {
        CheckAndReconnect();
        //防sql注入
        if (!DbManager.IsSafeString(id))
        {
            Console.WriteLine("[DB] GetPlayerData fail, id not safe");
            return null;
        }

        //sql
        string sql = string.Format("select * from player where id ='{0}';", id);
        try
        {
            //查询
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            if (!dataReader.HasRows)
            {
                dataReader.Close();
                return null;
            }
            //读取
            dataReader.Read();
            string data = dataReader.GetString("data");
            //反序列化
            // Player playerData = Js.Deserialize<Player>(data);
            Player playerData = new Player();

            dataReader.Close();
            return playerData;
        }
        catch (Exception e)
        {
            Console.WriteLine("[DB] GetPlayerData fail, " + e.Message);
            return null;
        }
    }


    //保存角色
    public static bool UpdatePlayerData(string id, Player playerData)
    {
        CheckAndReconnect();
        //序列化
        // string data = Js.Serialize(playerData);
        //sql
        // string sql = string.Format("update player set data='{0}' where id ='{1}';", data, id);
        string sql = "";
        //更新
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


