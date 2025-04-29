using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    class ChatSession : PacketSession
    {
        public int SessionId { get; set; }
        public ChatRoom Room { get; set; }

        public override void OnConnected(System.Net.EndPoint endPoint)
        {
            Console.WriteLine($"[Connected] {endPoint}");
            SessionManager.Instance.Push(() => SessionManager.Instance.Enter(this));
        }

        public override void OnDisconnected(System.Net.EndPoint endPoint)
        {
            Console.WriteLine($"[Disconnected] {endPoint}");
            if (Room != null)
            {
                ChatRoom room = Room;
                Room = null;
                room.Push(() => room.Leave(this));
            }

            SessionManager.Instance.Push(() => SessionManager.Instance.Remove(this));
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnSend(int numOfByte)
        {
            // Optionally log sent bytes
        }
    }
}
