using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server
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


    class ClientSession : PacketSession
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"[OnConnected]:{endPoint.ToString()}");
            Packet packet = new Packet() { size = 100, packetId = 100 };


            ArraySegment<byte> openSegemnt = SendBufferHelper.Open(4096);
            byte[] buffer = BitConverter.GetBytes(packet.size); // BitConvertor를 이용하여 int의 값을 4byte 배열로 변환 할수 있다 
            byte[] buffer2 = BitConverter.GetBytes(packet.packetId); // 4bytez`


            Array.Copy(buffer, 0, openSegemnt.Array, openSegemnt.Offset, buffer.Length);
            Array.Copy(buffer2, 0, openSegemnt.Array, openSegemnt.Offset + buffer.Length, buffer2.Length); // int byte만큼 더해줘야함 
            ArraySegment<byte> sendBuff = SendBufferHelper.Close(buffer.Length + buffer2.Length);


            //Send(sendBuff);
            Thread.Sleep(500);
            Disconnect();
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"[Disconnect] {endPoint.ToString()}");
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            ushort count = 0;

            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset+count);
            count += 2;
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count); //파싱한 size(2byte)를 더해줌
            count += 2;

            switch ((PacketId)id)
            {
                case PacketId.PlayerInfoReq:
                    {
                        long playerId = BitConverter.ToInt64(buffer.Array, buffer.Offset + count);
                        count += 8;
                        Console.WriteLine($"PlayerInfoReq:{playerId}");
                    }
                    break;
           
            }

            Console.WriteLine($"RecvPacketId:{id} Size,{size}");

        }

        public override void OnSend(int numOfByte)
        {
            Console.WriteLine($"Transferred bytes:{numOfByte}");

        }
    }
}
