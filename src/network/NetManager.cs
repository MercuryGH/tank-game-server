namespace network;

using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Reflection;

using network.model;
using network.util;
using network.protocol;

public static class NetManager
{
    public static Socket listenfd = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp); // listened Socket

    public static Dictionary<Socket, ClientState> clients = new Dictionary<Socket, ClientState>(); // 客户端Socket及状态信息

    private static List<Socket> checkReads = new List<Socket>(); // Select的检查列表

    public const long pingInterval = 30; // 30s 不 ping 则认为掉线
    private const string NETWORK_PROTOCOL_NAMESPACE_PREFIX = "network.protocol.";

    // 服务端不使用 receiveQueue 暂存接收结果，这是因为服务端会进行 busy select，而非客户端固定每帧处理几条接收的消息

    public static void StartLoop(int listenPort)
    {
        IPAddress ipAdr = IPAddress.Parse("0.0.0.0");
        IPEndPoint ipEp = new IPEndPoint(ipAdr, listenPort);
        listenfd.Bind(ipEp);

        listenfd.Listen(0);
        Console.WriteLine("Server started loop");

        while (true)
        {
            ResetCheckRead(); // 重置checkRead

            // 轮询，只查是否可读，超时时间为 1000ms
            Socket.Select(checkReads, null, null, 1000);
            /**
            If you are in a listening state, readability 
            means that a call to Accept will succeed 
            without blocking. If you have already 
            accepted the connection, readability 
            means that data is available for reading. 
            In these cases, all receive operations 
            will succeed without blocking. Readability 
            can also indicate whether the remote Socket 
            has shut down the connection; in that case 
            a call to Receive will return immediately, 
            with zero bytes returned.
            */

            // 检查可读对象
            foreach (Socket s in checkReads)
            {
                if (s == listenfd) // 尝试 accept socket
                {
                    ReadListenfd(s);
                }
                else // 尝试读取 client socket 
                {
                    ReadClientfd(s);
                }
            }
            Timer(); // 计时，由于Select设置超时时间为 1000ms，所以至多每秒调用一次
        }
    }

    // 重置可读客户端 Socket 列表
    private static void ResetCheckRead()
    {
        checkReads.Clear();
        foreach (ClientState s in clients.Values)
        {
            checkReads.Add(s.socket);
        }
        checkReads.Add(listenfd);
    }

    // 处理监听事件，调用 accept 接受客户端连接
    public static void ReadListenfd(Socket listenfd)
    {
        try
        {
            Socket clientfd = listenfd.Accept();
            Console.WriteLine("Accept client: " + clientfd.RemoteEndPoint!.ToString());
            ClientState state = new ClientState(clientfd, GetTimeStamp());
            clients.Add(clientfd, state);
        }
        catch (SocketException e)
        {
            Console.WriteLine("Accept fail" + e.ToString());
        }
        catch (NullReferenceException e)
        {
            Console.WriteLine("Accept null clientfd" + e.ToString());
        }
    }

    // 主动关闭 Socket 连接
    public static void Close(ClientState state)
    {
        network.util.EventHandler.OnDisconnect(state);

        // 关闭
        state.socket.Close();
        clients.Remove(state.socket);
    }

    // 读取客户端 socket
    public static void ReadClientfd(Socket clientfd)
    {
        ClientState state = clients[clientfd];
        ByteBuffer readBuff = state.readBuff;
        // 接收
        int count = 0;

        // 缓冲区不够，清除，若依旧不够，只能返回
        // 当单条协议超过缓冲区长度时会发生
        if (readBuff.RemainSize <= 0 || readBuff.writeIdx > ByteBuffer.MAX_WRITE_INDEX)
        {
            ReceiveDataFrom(state);
            readBuff.MoveBytes();
        };
        if (readBuff.RemainSize <= 0)
        {
            Console.WriteLine("Receive failed, message is too long!!!");

            // 服务器炸了
            MsgKick msgKick = new MsgKick();
            msgKick.reason = 2;
            Send(state, msgKick);

            Close(state);
            return;
        }

        try
        {
            count = clientfd.Receive(readBuff.bytes, readBuff.writeIdx, readBuff.RemainSize, 0);
            // 客户端关闭
            if (count == 0)
            {
                Console.WriteLine("Socket Close " + clientfd.RemoteEndPoint!.ToString());
                Close(state);
                return;
            }
        }
        catch (SocketException e)
        {
            Console.WriteLine("Receive SocketException " + e.ToString());
            Close(state);
            return;
        }
        catch (NullReferenceException e)
        {
            Console.WriteLine("Closing null clientfd" + e.ToString());
            return;
        }
        catch (ArgumentOutOfRangeException e)
        {
            Console.WriteLine("ArgumentOutOfRangeException: " + e.ToString());
        } 
        catch (Exception e)
        {
            Console.WriteLine("Unknown Exception: " + e.ToString());
        }

        // 消息处理
        readBuff.writeIdx += count;
        // 处理二进制消息
        ReceiveDataFrom(state);
        // 移动缓冲区
        readBuff.CheckAndMoveBytes();
    }

    // 数据处理
    public static void ReceiveDataFrom(ClientState state)
    {
        ByteBuffer readBuff = state.readBuff;
        // 消息长度
        if (readBuff.CurSize <= 2)
        {
            return;
        }

        // 消息体长度
        int readIdx = readBuff.readIdx;
        byte[] bytes = readBuff.bytes;
        Int16 bodyLength = (Int16)((bytes[readIdx + 1] << 8) | bytes[readIdx]);
        if (readBuff.CurSize < bodyLength)
        {
            return;
        }
        readBuff.readIdx += 2;

        // 解析协议名
        string? protoName = BaseMsg.DecodeName(readBuff.bytes, readBuff.readIdx, out int nameCount);
        if (protoName == null || protoName == "")
        {
            Console.WriteLine("OnReceiveData MsgBase. DecodeName failed");

            // 发送特殊的踢下线协议
            MsgKick msgKick = new MsgKick();
            msgKick.reason = 1;
            Send(state, msgKick);

            Close(state);
            return;
        }
        readBuff.readIdx += nameCount;

        // 解析协议体
        int bodyCount = bodyLength - nameCount;
        if (bodyCount <= 0)
        {
            Console.WriteLine("OnReceiveData fail, bodyCount <= 0");

            // 发送特殊的踢下线协议
            MsgKick msgKick = new MsgKick();
            msgKick.reason = 1;
            Send(state, msgKick);

            Close(state);
            return;
        }

        MethodInfo decodeMethod = typeof(BaseMsg).GetMethod(nameof(BaseMsg.Decode))!;
        Type? genericType = Type.GetType(NETWORK_PROTOCOL_NAMESPACE_PREFIX + protoName);
        if (genericType == null)
        {
            Console.WriteLine("Cannot decode Message, OMG");

            // 发送特殊的踢下线协议
            MsgKick msgKick = new MsgKick();
            msgKick.reason = 1;
            Send(state, msgKick);

            Close(state);
            return;
        }
        MethodInfo genericMethod = decodeMethod.MakeGenericMethod(genericType);
        BaseMsg msg = (BaseMsg)genericMethod.Invoke(null, new object[] { readBuff.bytes, readBuff.readIdx, bodyCount })!;
        readBuff.readIdx += bodyCount;
        readBuff.CheckAndMoveBytes();
        if (msg == null) // 译码出了什么问题
        {
            Console.WriteLine("Cannot decode Message, OMG");

            // 发送特殊的踢下线协议
            MsgKick msgKick = new MsgKick();
            msgKick.reason = 1;
            Send(state, msgKick);

            Close(state);
            return;
        }

        // 分发消息
        MethodInfo mi = typeof(MsgHandler).GetMethod(protoName)!;
        object[] o = { state, msg };

        // Debug
        if (protoName != "MsgSyncTank")
            Console.WriteLine("Receive " + protoName);

        if (mi != null)
        {
            mi.Invoke(null, o);
        }
        else
        {
            Console.WriteLine("OnReceiveData Invoke fail " + protoName);
        }

        // 继续读取消息
        if (readBuff.CurSize > 2)
        {
            ReceiveDataFrom(state);
        }
    }

    // 不考虑未完整发送的 Send
    public static void Send(ClientState cs, BaseMsg msg)
    {
        if (cs == null || cs.socket.Connected == false)
        {
            return;
        }

        byte[] nameBytes = BaseMsg.EncodeName(msg);
        byte[] bodyBytes = BaseMsg.Encode(msg);
        int len = nameBytes.Length + bodyBytes.Length;
        byte[] sendBytes = new byte[2 + len];
        sendBytes[0] = (byte)(len % 256);
        sendBytes[1] = (byte)(len / 256);
        Array.Copy(nameBytes, 0, sendBytes, 2, nameBytes.Length);
        Array.Copy(bodyBytes, 0, sendBytes, 2 + nameBytes.Length, bodyBytes.Length);

        // 为简化代码，不设置回调
        try
        {
            cs.socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, null, null);
        }
        catch (SocketException ex)
        {
            Console.WriteLine("Socket Close on BeginSend" + ex.ToString());
        }

    }

    private static void Timer()
    {
        network.util.EventHandler.OnTimer();
    }

    // 获取时间戳
    public static long GetTimeStamp()
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(ts.TotalSeconds);
    }
}