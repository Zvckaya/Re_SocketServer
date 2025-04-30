// GenPackets.cs
using ServerCore;
using System;
using System.Text;

namespace ChatServer.Packet
{
    public enum PacketID
    {
        C_Chat = 1,
        S_Chat = 2,
    }

    public interface IPacket
    {
        ushort Protocol { get; }
        void Read(ArraySegment<byte> segment);
        ArraySegment<byte> Write();
    }

    public class C_Chat : IPacket
    {
        public string message;

        public ushort Protocol => (ushort)PacketID.C_Chat;

        public void Read(ArraySegment<byte> segment)
        {
            ushort count = 0;
            ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
            count += sizeof(ushort);
            count += sizeof(ushort);
            ushort msgLen = BitConverter.ToUInt16(s.Slice(count));
            count += sizeof(ushort);
            message = Encoding.UTF8.GetString(s.Slice(count, msgLen));
        }

        public ArraySegment<byte> Write()
        {
            ArraySegment<byte> segment = SendBufferHelper.Open(4096);
            bool success = true;
            ushort count = 0;

            Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);
            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(count), (ushort)PacketID.C_Chat);
            count += sizeof(ushort);

            ushort msgLen = (ushort)Encoding.UTF8.GetByteCount(message);
            success &= BitConverter.TryWriteBytes(s.Slice(count), msgLen);
            count += sizeof(ushort);

            int byteLen = Encoding.UTF8.GetBytes(message, s.Slice(count));
            count += (ushort)byteLen;
            
            success &= BitConverter.TryWriteBytes(s, count);
            return SendBufferHelper.Close(count);
        }
    }

    public class S_Chat : IPacket
    {
        public string message;

        public ushort Protocol => (ushort)PacketID.S_Chat;

        public void Read(ArraySegment<byte> segment)
        {
            ushort count = 0;
            ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
            count += sizeof(ushort);
            count += sizeof(ushort);
            ushort msgLen = BitConverter.ToUInt16(s.Slice(count));
            count += sizeof(ushort);
            message = Encoding.UTF8.GetString(s.Slice(count, msgLen));
        }

        public ArraySegment<byte> Write()
        {
            ArraySegment<byte> segment = SendBufferHelper.Open(4096);
            ushort count = 0;
            Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

            count += sizeof(ushort);                                   // [size] 자리 비워두기
            BitConverter.TryWriteBytes(s.Slice(count), (ushort)PacketID.S_Chat);
            count += sizeof(ushort);                                   // [id]

            ushort msgLen = (ushort)Encoding.UTF8.GetByteCount(message);
            BitConverter.TryWriteBytes(s.Slice(count), msgLen);
            count += sizeof(ushort);                                   // [strlen]

            int byteCnt = Encoding.UTF8.GetBytes(message, s.Slice(count));
            count += (ushort)byteCnt;                                  // [string]

            BitConverter.TryWriteBytes(s, count);                      // size 필드 마무리
            return SendBufferHelper.Close(count);
        }

    }
}