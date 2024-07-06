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


            //for (int i = 0; i < 5; i++)
            //{
            PlayerInfoReq packet = new PlayerInfoReq() { size = 4, packetId = (ushort)PacketId.PlayerInfoReq };

            ArraySegment<byte> s = SendBufferHelper.Open(4096);

            byte[] size = BitConverter.GetBytes(packet.size); // BitConvertor를 이용하여 int의 값을 2byte 배열로 변환 할수 있다 
            byte[] packetId = BitConverter.GetBytes(packet.packetId); // 2bytez
            byte[] playerId = BitConverter.GetBytes(packet.playerId);  // long-> 8

            ushort count = 0;
            Array.Copy(size, 0, s.Array, s.Offset + count , 2);
            count += 2;
            Array.Copy(packetId, 0, s.Array, s.Offset + count, 2); // int byte만큼 더해줘야함
            count += 2;
            Array.Copy(playerId, 0, s.Array, s.Offset + count, 8);
            count += 8;

            ArraySegment<byte> sendBuff = SendBufferHelper.Close(count);
            Send(sendBuff);
            //}


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
