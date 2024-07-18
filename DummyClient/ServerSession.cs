using ServerCore;
using System;
using System.Net;
using System.Text;

namespace DummyClient
{


    //class PlayerInfoReq
    //{
    //    public long playerId;
    //    public string name;

    //    public struct SkillInfo
    //    {
    //        public int id;
    //        public short level;
    //        public float duration;

    //        public bool Write(Span<byte> s, ref ushort count)
    //        {
    //            bool success = true;
    //            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), id);
    //            count += sizeof(int);
    //            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), level);
    //            count += sizeof(short);
    //            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), duration);
    //            count += sizeof(float);

    //            return success;
    //        }

    //        public void Read(ReadOnlySpan<byte> s, ref ushort count)
    //        {
    //            id = BitConverter.ToInt32(s.Slice(count, s.Length - count)); // 4에서부터 
    //            count += sizeof(int);
    //            level = BitConverter.ToInt16(s.Slice(count, s.Length - count)); // 4에서부터 
    //            count += sizeof(short);
    //            duration = BitConverter.ToSingle(s.Slice(count, s.Length - count)); // 4에서부터 
    //            count += sizeof(float);
    //        }

    //    }


    //    public List<SkillInfo> skills = new List<SkillInfo>();

    //    public List<int> myList = new List<int>();



    //    public void Read(ArraySegment<byte> segment)
    //    {
    //        ushort count = 0;

    //        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
    //        // ushort size = BitConverter.ToUInt16(s.Array, s.Offset + count);
    //        count += sizeof(ushort);
    //        //ushort id = BitConverter.ToUInt16(s.Array, s.Offset + count); //파싱한 size(2byte)를 더해줌
    //        count += sizeof(ushort);
    //        this.playerId = BitConverter.ToInt64(s.Slice(count, s.Length - count)); // 4에서부터 
    //        count += sizeof(long);

    //        //string 

    //        ushort nameLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count)); // 맨처음 string의 길이를 읽어온다.
    //        count += sizeof(ushort);
    //        this.name = Encoding.Unicode.GetString(s.Slice(count, nameLen));
    //        count += nameLen;

    //        skills.Clear();
    //        //list 
    //        ushort listLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
    //        count += sizeof(ushort);
    //        for (int i = 0; i < listLen; i++)
    //        {
    //            int num = BitConverter.ToInt32(s.Slice(count, s.Length - count));
    //            myList.Add(num);
    //            count += sizeof(int);
    //        }



    //        //skill list 
    //        ushort skillLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count)); //맨처음 스킬list의 사이즈를 받아온다.
    //        count += sizeof(ushort);
    //        skills.Clear(); // 기존에 다른 정보를 들고 있었을 수 있기 때문에.
    //        for (int i = 0; i < skillLen; i++)
    //        {
    //            SkillInfo skill = new SkillInfo();
    //            skill.Read(s, ref count);
    //            skills.Add(skill);
    //        }

    //    }

    //    public ArraySegment<byte> Write()
    //    {
    //        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
    //        bool success = true;
    //        ushort count = 0;


    //        Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

    //        count += sizeof(ushort);
    //        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketId.PlayerInfoReq);
    //        count += sizeof(ushort);
    //        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
    //        count += sizeof(long);


    //        ushort nameLen = (ushort)Encoding.Unicode.GetBytes
    //            (this.name, 0, this.name.Length, segment.Array, segment.Offset + count + sizeof(ushort));
    //        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen);
    //        count += sizeof(ushort);
    //        count += nameLen;

    //        //list 
    //        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)myList.Count);
    //        count += sizeof(ushort);
    //        foreach (int i in myList)
    //        {
    //            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), i);
    //            count += sizeof(int);
    //        }


    //        //skill list 
    //        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)skills.Count);
    //        count += sizeof(ushort);
    //        foreach (SkillInfo skill in skills) //skills를 순회하며 count와 전송할 
    //            success &= skill.Write(s, ref count);

    //        success &= BitConverter.TryWriteBytes(s, count); //마지막은 패킷의 사이즈를 나타낸다.

    //        ArraySegment<byte> sendBuff = SendBufferHelper.Close(count);

    //        if (sendBuff == null)
    //            return null;

    //        return sendBuff;
    //    }
    //}

    //public enum PacketId
    //{
    //    PlayerInfoReq = 1,
    //    PlayerInfoOk = 2,
    //}

    class ServerSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"[OnConnected]:{endPoint.ToString()}");


            //for (int i = 0; i < 5; i++)
            //{
            PlayerInfoReq packet = new PlayerInfoReq() { playerId = 1001, name = "ABCD" };

   
            packet.skills.Add(new PlayerInfoReq.Skill() { id = 101, level = 1, duration = 3.0f });
            packet.skills.Add(new PlayerInfoReq.Skill() { id = 102, level = 2, duration = 4.0f });
            packet.skills.Add(new PlayerInfoReq.Skill() { id = 103, level = 3, duration = 5.0f });

            ArraySegment<byte> s = packet.Write();

            if (s != null)
                Send(s);

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