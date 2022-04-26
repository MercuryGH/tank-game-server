namespace network.protocol;
using System.Text.Json;

public class BaseMsg
{
    public string protoName { get; set; } = "null";
    /**
     * 协议格式为 
     * varlen struct Message {
     *     short totalLength; // size(Message) - sizeof(short)
     *     short msgTypeLength;
     *     byte[] msgType;
     *     byte[] infoDetail;
     * };
     */

    /**
     * 将BaseMsg串行化成JSON
     * 没有考虑长度等其他信息，还需要编码成协议格式才能发送
     * TODO: 封装编码方法
     */

    public static JsonSerializerOptions options = new(JsonSerializerDefaults.General)
    {
        IncludeFields = true
    };  // System.Text.Json 默认居然是不串行化字段的，坑

    public static byte[] Encode(BaseMsg baseMsg)
    {
        // Debug only
        if (baseMsg.GetType() != typeof(MsgSyncTank))
        {
            Console.WriteLine("Debug encode: " + JsonSerializer.Serialize(baseMsg, baseMsg.GetType(), options));
        }

        return JsonSerializer.SerializeToUtf8Bytes(baseMsg, baseMsg.GetType(), options);
    }

    /**
     * 将解码后的结果反串行化成 BaseMsg
     * @generic <T> 协议类
     * @params bytes 原字节流
     * @params offset 从 bytes[offset] 的位置开始解释JSON
     * @params count 到 bytes[offset + count] 的位置停止解释JSON
     */
    public static BaseMsg? Decode<T>(byte[] bytes, int offset, int count) where T : BaseMsg
    {
        BaseMsg? baseMsg = null;
        try
        {
            string s = System.Text.Encoding.UTF8.GetString(bytes, offset, count);
            baseMsg = (BaseMsg)JsonSerializer.Deserialize<T>(s, options)!;

            // DEBUG only
            if (baseMsg.GetType() != typeof(MsgSyncTank))
            {
                Console.WriteLine("Debug encode: " + s);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Error while decode JSON: " + e.ToString());
        }
        return baseMsg;
    }

    /**
     * 编码协议名
     * @params BaseMsg 待发送消息
     * @return bytes 串行化后的 协议名长度 + 协议名 构成的 byte array
     */
    public static byte[] EncodeName(BaseMsg baseMsg)
    {
        byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(baseMsg.protoName);
        Int16 len = (Int16)nameBytes.Length;

        byte[] bytes = new byte[2 + len];
        // 组装2字节的长度信息
        bytes[0] = (byte)(len % 256);
        bytes[1] = (byte)(len / 256);
        // 组装名字bytes
        Array.Copy(nameBytes, 0, bytes, 2, len);

        return bytes;
    }

    /**
     * 解码协议名
     * @params bytes  原字节流
     * @params offset 从 bytes[offset] 开始提取 msgTypeLength
     * @return 协议名 msgType
     * @return_by_out count = sizeof(short) + len(msgType)
     */
    public static string? DecodeName(byte[] bytes, int offset, out int count)
    {
        count = 0;
        // 边界情况考虑，避免数组越界
        if (offset + 2 > bytes.Length)
        {
            return "";
        }

        // 读取长度
        Int16 len = (Int16)((bytes[offset + 1] << 8) | bytes[offset]);
        if (offset + 2 + len > bytes.Length)
        {
            return "";
        }

        count = 2 + len;
        string? name = null;
        try {
            name = System.Text.Encoding.UTF8.GetString(bytes, offset + 2, len);
        } catch (ArgumentOutOfRangeException e) {
            Console.WriteLine("Argument Error while decode JSON: " + e.ToString());
        } catch (Exception e) {
            Console.WriteLine("Error while decode JSON: " + e.ToString());
        }
        return name;
    }
}
