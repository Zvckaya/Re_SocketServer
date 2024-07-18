using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class PacketManager
    {
        #region Singleton
        static PacketManager _instance;
        public static PacketManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new PacketManager();
                return _instance;
            }
        }
        #endregion

        // 프로토콜 id와 action 등록 
        Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> _onRecv =
            new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>>();

        // 핸들러에 
        Dictionary<ushort, Action<PacketSession, IPacket>> _handler =
            new Dictionary<ushort, Action<PacketSession, IPacket>>();

        public void Register()
        {
            _onRecv.Add((ushort)PacketID.PlayerInfoReq, MakePacket<PlayerInfoReq>);
            _handler.Add((ushort)PacketID.PlayerInfoReq, PacketHandler.PlayerInfoReqHandler);//패킷이름에 handler를 붙인 함수 
        }


        public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
        {
            ushort count = 0;

            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;

            Action<PacketSession, ArraySegment<byte>> action = null;
            if (_onRecv.TryGetValue(id, out action))
            {
                action.Invoke(session, buffer);
            }
        }

        void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new() //패킷 클래스를 넘겨 등록 
        { 
            T p = new T();
            p.Read(buffer);

            Action<PacketSession, IPacket> action = null;
            if(_handler.TryGetValue(p.Protocol,out action))
            {
                action.Invoke(session, p);
            }
        }
    }
}
