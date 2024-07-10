using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public abstract class Packet
    {
        public ushort size;
        public ushort packetId;

        public abstract ArraySegment<byte> Write();
        public abstract void Read(ArraySegment<byte> s);
    }

    class PlayerInfoReq : Packet
    {
        public long playerId;
        public string name;

        public struct SkillInfo
        {
            public int id;
            public short level;
            public float duration;

            public bool Write(Span<byte> s, ref ushort count)
            {
                bool success = true;
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), id);
                count += sizeof(int);
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), level);
                count += sizeof(short);
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), duration);
                count += sizeof(float);

                return success;
            }

            public void Read(ReadOnlySpan<byte> s, ref ushort count)
            {
                id = BitConverter.ToInt32(s.Slice(count, s.Length - count)); // 4에서부터 
                count += sizeof(int);
                level = BitConverter.ToInt16(s.Slice(count, s.Length - count)); // 4에서부터 
                count += sizeof(short);
                duration = BitConverter.ToSingle(s.Slice(count, s.Length - count)); // 4에서부터 
                count += sizeof(float);
            }

        }

        public List<SkillInfo> skills = new List<SkillInfo>();


        public PlayerInfoReq()
        {
            this.packetId = (ushort)PacketId.PlayerInfoReq;
        }

        public override void Read(ArraySegment<byte> segment)
        {
            ushort count = 0;

            ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
            // ushort size = BitConverter.ToUInt16(s.Array, s.Offset + count);
            count += sizeof(ushort);
            //ushort id = BitConverter.ToUInt16(s.Array, s.Offset + count); //파싱한 size(2byte)를 더해줌
            count += sizeof(ushort);
            this.playerId = BitConverter.ToInt64(s.Slice(count, s.Length - count)); // 4에서부터 
            count += sizeof(long);

            //string 

            ushort nameLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count)); // 맨처음 string의 길이를 읽어온다.
            count += sizeof(ushort);
            this.name = Encoding.Unicode.GetString(s.Slice(count, nameLen));
            count += nameLen;

            //skill list 
            ushort skillLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count)); //맨처음 스킬list의 사이즈를 받아온다.
            count += sizeof(ushort);
            skills.Clear(); // 기존에 다른 정보를 들고 있었을 수 있기 때문에.
            for (int i = 0; i < skillLen; i++)
            {
                SkillInfo skill = new SkillInfo();
                skill.Read(s, ref count);
                skills.Add(skill);
            }

        }

        public override ArraySegment<byte> Write()
        {
            ArraySegment<byte> segment = SendBufferHelper.Open(4096);
            bool success = true;
            ushort count = 0;


            Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.packetId);
            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
            count += sizeof(long);


            ushort nameLen = (ushort)Encoding.Unicode.GetBytes
                (this.name, 0, this.name.Length, segment.Array, segment.Offset + count + sizeof(ushort));
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen);
            count += sizeof(ushort);
            count += nameLen;

            //skill list 
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)skills.Count);
            count += sizeof(ushort);
            foreach (SkillInfo skill in skills) //skills를 순회하며 count와 전송할 
                success &= skill.Write(s, ref count);

            success &= BitConverter.TryWriteBytes(s, count); //마지막은 패킷의 사이즈를 나타낸다.

            ArraySegment<byte> sendBuff = SendBufferHelper.Close(count);

            if (sendBuff == null)
                return null;

            return sendBuff;
        }
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

            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count); //파싱한 size(2byte)를 더해줌
            count += 2;

            switch ((PacketId)id)
            {
                case PacketId.PlayerInfoReq:
                    {
                        PlayerInfoReq p = new PlayerInfoReq();
                        p.Read(buffer);
                        Console.WriteLine($"PlayerInfoReq: {p.playerId} ");
                        Console.WriteLine($"Playername: {p.name}");

                        foreach (PlayerInfoReq.SkillInfo skill in p.skills)
                        {
                            Console.WriteLine($"Skill {skill.id} {skill.duration} {skill.level}");
                        }
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
