// GenPackets.cs
using ServerCore;
using System;
using System.Text;

namespace ChatServer
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
            success &= Encoding.UTF8.GetBytes(message, s.Slice(count));
            count += msgLen;

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

            count += sizeof(ushort); // 패킷 전체 길이
            BitConverter.TryWriteBytes(s.Slice(count), (ushort)PacketID.C_Chat);
            count += sizeof(ushort); // 패킷 ID

            ushort msgLen = (ushort)Encoding.UTF8.GetByteCount(message);
            BitConverter.TryWriteBytes(s.Slice(count), msgLen);
            count += sizeof(ushort); // 문자열 길이

            int byteCount = Encoding.UTF8.GetBytes(message, s.Slice(count));
            count += (ushort)byteCount; // 실제 메시지

            BitConverter.TryWriteBytes(s, count); // 최종 전체 패킷 길이 입력

            return SendBufferHelper.Close(count);
        }

    }
}