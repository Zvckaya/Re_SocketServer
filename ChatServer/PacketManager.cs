using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    class PacketManager
    {
        static PacketManager _instance = new PacketManager();
        public static PacketManager Instance => _instance;

        Dictionary<ushort, Action<PacketSession, IPacket>> _handler = new Dictionary<ushort, Action<PacketSession, IPacket>>();
        Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>> _onRecv = new Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>>();

        PacketManager()
        {
            Register();
        }

        public void Register()
        {
            _onRecv.Add((ushort)PacketID.C_Chat, MakePacket<C_Chat>);
            _handler.Add((ushort)PacketID.C_Chat, PacketHandler.C_ChatHandler);
        }

        public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
        {
            ushort count = 0;

            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;

            if (_onRecv.TryGetValue(id, out var func))
            {
                IPacket packet = func.Invoke(session, buffer);
                HandlePacket(session, packet);
            }
        }

        T MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new()
        {
            T p = new T();
            p.Read(buffer);
            return p;
        }

        void HandlePacket(PacketSession session, IPacket packet)
        {
            if (_handler.TryGetValue(packet.Protocol, out var action))
            {
                action.Invoke(session, packet);
            }
        }
    }
}
