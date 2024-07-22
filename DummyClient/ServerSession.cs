using ServerCore;
using System;
using System.Net;
using System.Text;

namespace DummyClient
{

    class ServerSession : PacketSession
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"[OnConnected]:{endPoint.ToString()}");


        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"[Disconnect] {endPoint.ToString()}");
        }

  

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this,buffer);
        }

        public override void OnSend(int numOfByte)
        {
           // Console.WriteLine($"Transferred bytes:{numOfByte}");
        }
    }
}