// Program.cs
using ServerCore;
using System;
using System.Net;

namespace ChatServer
{
    class Program
    {
        static Listener _listener = new Listener();
        public static ChatRoom Room = new ChatRoom();

        static void FlushRoom()
        {
            Room.Push(() => Room.Flush());
            JobTimer.Instance.Push(FlushRoom, 100); // 100ms 주기로 플러시
        }

        static void Main(string[] args)
        {
            PacketManager.Instance.Register();

            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            _listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
            Console.WriteLine("[Server] Listening...");

            FlushRoom();

            while (true)
            {
                JobTimer.Instance.Flush();
            }
        }
    }
}
