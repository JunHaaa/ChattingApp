using StackExchange.Redis;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using static TcpServer;
class TcpServer
{
    static Socket server_socket = null;
    static Dictionary<string, Socket> client_dic = new Dictionary<string, Socket>();
    static Dictionary<Socket, string> client_socket_dic = new Dictionary<Socket, string>();
    static ConnectionMultiplexer redis;
    static IDatabase db;
    
    class ChattingRoom
    {
        static int id = 0;
        string[] users;
        string chatlog;
    }
    public enum meesageType
    {
        set_name = 0,
        message_send = 1
    }
    public class Message
    {
        public int type;
        public string[] receivers_name;
        public string text;
    }
    public class AsyncStateData
    {
        public byte[] buffer;
        public Socket socket;
    }
    public static void StartServer()
    {
        server_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        server_socket.Bind(new IPEndPoint(IPAddress.Any, 13000));
        server_socket.Listen(10);
        Console.WriteLine("서버 시작");

        
        redis = ConnectionMultiplexer.Connect("127.0.0.1:6379");
        if (!redis.IsConnected)
        {
            Console.WriteLine("redis 서버에 연결 실패");
        }
        db = redis.GetDatabase();
        

        while (true)
        {
            server_socket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

    }
    public static void AcceptCallback(IAsyncResult ar)
    {
        Socket client_socket = server_socket.EndAccept(ar);
        Console.WriteLine("클라이언트 접속");
        AsyncStateData data = new AsyncStateData();
        data.socket = client_socket;
        data.buffer = new byte[1024];
        client_socket.BeginReceive(data.buffer, 0, data.buffer.Length, SocketFlags.None, new AsyncCallback(ReciveCallback), data);
    }
    public static string GetKeyFormat(string[] keys)
    {
        string key = "";
        Array.Sort(keys);
        foreach (string k in keys)
        {
            key += (k + "@");
        }
        return key;
    }
    private static void ReciveCallback(IAsyncResult ar)
    {
        AsyncStateData rcv_data = ar.AsyncState as AsyncStateData;
        Socket client_socket = rcv_data.socket;
        int recived = 0;
        string key = "";
        try
        {
            recived = client_socket.EndReceive(ar);//recive == data.length
            byte[] data_buffer = new byte[recived];
            Array.Copy(rcv_data.buffer, data_buffer, recived);

            string decoded_text = Encoding.UTF8.GetString(data_buffer);
            Message message = MessageSplit(decoded_text);
            Console.WriteLine(DateTime.Now.ToString() + " [{0}]: " + message.text, message.receivers_name);
            switch (message.type)
            {
                case (int)meesageType.set_name:
                    client_dic.Add(message.receivers_name[0], client_socket);
                    client_socket_dic.Add(client_socket, message.receivers_name[0]);
                    key = GetKeyFormat(message.receivers_name);
                    string log = "empty";
                    byte[] log_byte = Encoding.UTF8.GetBytes(log);
                    if (db.KeyExists(key))
                    {
                        log = db.StringGet(key);
                        log_byte = Encoding.UTF8.GetBytes(log);
                    }
                    client_socket.BeginSend(log_byte, 0, log_byte.Length, SocketFlags.None, new AsyncCallback(SendCallback), rcv_data);
                    break;
                case (int)meesageType.message_send:
                    List<Socket> receiver_list = GetreceiverSocket(message.receivers_name);
                    foreach (Socket receiver_socket in receiver_list)
                    {
                        receiver_socket.BeginSend(data_buffer, 0, data_buffer.Length, SocketFlags.None, new AsyncCallback(SendCallback), rcv_data);
                    }
                    key = GetKeyFormat(message.receivers_name);
                    string value = Encoding.UTF8.GetString(data_buffer);
                    value += "$";
                    db.StringAppend(key, value);
                    break;
            }


            client_socket.BeginReceive(rcv_data.buffer, 0, rcv_data.buffer.Length, SocketFlags.None, new AsyncCallback(ReciveCallback), rcv_data);
        }
        catch (SocketException e)
        {
            Console.WriteLine("socket exception");
            string name = client_socket_dic[client_socket];
            client_socket_dic.Remove(client_socket);
            client_dic.Remove(name);
            client_socket.Dispose();
            return;
        }
    }

    public static List<Socket> GetreceiverSocket(string[] receivers_name)
    {
        List<Socket> receiver_list = new List<Socket>();

        foreach (string receiver in receivers_name[1..])
        {
            if (client_dic.ContainsKey(receiver))
            {
                receiver_list.Add(client_dic[receiver]);
            }
        }

        return receiver_list;
    }
    public static Message MessageSplit(string text)
    {
        Message message = new Message();
        string[] type_text = text.Split("!");
        message.type = Int32.Parse(type_text[0]);


        string[] receivers_text = type_text[1].Split("#");
        message.receivers_name = receivers_text[0].Split("@");

        message.text = receivers_text[1];


        return message;
    }

    public static void SendCallback(IAsyncResult ar)
    {
        AsyncStateData rcv_data = ar.AsyncState as AsyncStateData;
        Socket client_socket = rcv_data.socket;
        client_socket.EndSend(ar);
        //socket.Close();
    }
    static void Main()
    {
        StartServer();
    }
}