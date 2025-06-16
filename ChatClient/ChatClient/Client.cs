using System;
using System.Net;
using System.Net.Sockets;
using System.Text;


class TcpClient
{
    Socket socket = null;
    string name = null;
    public class AsyncStateData
    {
        public byte[] buffer;
        public TcpClient client;
    }
    public class Message
    {
        public int type;
        public string[] receivers_name;
        public string text;
    }
    AsyncStateData data = null;
    public TcpClient()
    {
        data = new AsyncStateData();
        data.client = this;
        data.buffer = new byte[1024];
    }

    public void receive()
    {
        data.client.socket.BeginReceive(data.buffer, 0, data.buffer.Length,
                                    SocketFlags.None, AsyncReceiveCallback, data);
    }
    private static void AsyncReceiveCallback(IAsyncResult async_result)
    {
        AsyncStateData rcv_data = async_result.AsyncState as AsyncStateData;
        int rcv_len = rcv_data.client.socket.EndReceive(async_result);
        string text = Encoding.UTF8.GetString(rcv_data.buffer, 0, rcv_len);
        Message message = MessageSplit(text);
        Console.WriteLine("[{0}]: " + message.text, message.receivers_name[0]);
        rcv_data.client.receive();
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
    void SendName(String recievers)
    {
        String send_text = "0!" + recievers;
        byte[] buf = Encoding.UTF8.GetBytes(send_text);
        socket.Send(buf);
        byte[] rcv_buf = new byte[4096];
        socket.Receive(rcv_buf);
        string s = Encoding.UTF8.GetString(rcv_buf);

        if (s.Equals("empty"))
        {
            Console.WriteLine("message empty");
        }
        else
        {
            ChatLogWrite(s);
        }
    }
    public void ChatLogWrite(string chat_log)
    {
        string[] chat = chat_log.Split("$");

        foreach (string s in chat[..^1])
        {

            Message m = MessageSplit(s);
            Console.WriteLine("[{0}]: {1}", m.receivers_name[0], m.text);

        }
    }
    public string MessageFormat(int type, string receiver, string text)
    {
        string message = type.ToString() + "!" + this.name + "@" + receiver + "#" + text;
        return message;
    }
    private void MessageSendAsync(int type, string receiver)
    {
        Task<string> send_text = ReadLineAsync();
        string message = MessageFormat(type, receiver, send_text.Result.ToString());
        byte[] buf = Encoding.UTF8.GetBytes(message);

        this.socket.Send(buf);
    }

    public async Task<string> ReadLineAsync()
    {
        string result = string.Empty;
        result = await Console.In.ReadLineAsync();

        return result;
    }

    static void Main()
    {
        TcpClient client = new TcpClient();
        Console.Write("사용할 이름: ");
        client.name = Console.ReadLine();

        Console.WriteLine("대화 상대를 입력하세요. @로 복수 상대 지정 가능. 예) user1@user2");
        Console.Write("대화 상대: ");
        string receivers = Console.ReadLine();
        Console.WriteLine("대화 시작==>");
        int type = 0;
        try
        {
            client.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPAddress ip = IPAddress.Parse("127.0.0.1");
            EndPoint endpoint = new IPEndPoint(ip, 13000);

            client.socket.Connect(endpoint);


            client.SendName(client.name + "@" + receivers + "#None");

            client.receive();
            while (true)
            {
                client.MessageSendAsync(1, receivers);
                client.receive();
            }

            client.socket.Close();
            Console.WriteLine("TCP Client socket closed.");
        }
        catch (Exception e)
        {
            Console.WriteLine("예외: " + e.Message);
        }
    }
}