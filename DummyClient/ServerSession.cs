using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DummyClient
{
    class Packet
    {
        public ushort size;
        public ushort packetId;
    }

    class PlayerInfoReq : Packet
    {
        public long playerId;
    }

    class PlayerInfoOk : Packet
    {
        public int hp;
        public int attack;
    }

    public enum PacketId
    {
        PlayerInfoReq = 1,
        PlayerInfoOk = 2,
    }

    class ServerSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"[OnConnected]:{endPoint.ToString()}");


            for (int i = 0; i < 10; i++)
            {
                Packet packet = new Packet() { size = 4, packetId = (ushort)i };

                ArraySegment<byte> openSegemnt = SendBufferHelper.Open(4096);
                byte[] buffer = BitConverter.GetBytes(packet.size); // BitConvertor를 이용하여 int의 값을 4byte 배열로 변환 할수 있다 
                byte[] buffer2 = BitConverter.GetBytes(packet.packetId); // 4bytez
                Array.Copy(buffer, 0, openSegemnt.Array, openSegemnt.Offset, buffer.Length);
                Array.Copy(buffer2, 0, openSegemnt.Array, openSegemnt.Offset + buffer.Length, buffer2.Length); // int byte만큼 더해줘야함 
                ArraySegment<byte> sendBuff = SendBufferHelper.Close(packet.size);
                Send(sendBuff);
            }


        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"[Disconnect] {endPoint.ToString()}");
        }

        public override int OnRecv(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, 0, buffer.Count);
            Console.WriteLine($"[From Server] {recvData}");
            return buffer.Count;
        }

        public override void OnSend(int numOfByte)
        {
            Console.WriteLine($"Transferred bytes:{numOfByte}");
        }
    }
}
