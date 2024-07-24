using ServerCore;
class PacketManager
{
    #region Singleton
    static PacketManager _instance = new PacketManager();
    public static PacketManager Instance
    {
        get
        {
   
            return _instance;
        }
    }
    #endregion

    PacketManager()
    {
        Register();
    }

    Dictionary<ushort, Func<PacketSession, ArraySegment<byte>,IPacket>> _onRecv =
        new Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>>();

    Dictionary<ushort, Action<PacketSession, IPacket>> _handler =
        new Dictionary<ushort, Action<PacketSession, IPacket>>();

    public void Register()
    {
             _onRecv.Add((ushort)PacketID.S_Chat, MakePacket<S_Chat>);
        _handler.Add((ushort)PacketID.S_Chat, PacketHandler.S_ChatHandler);


    }


    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer,Action<PacketSession,IPacket> onRecvCallBack = null)
    {
        ushort count = 0;

        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        Func<PacketSession, ArraySegment<byte>,IPacket> func = null;
        if (_onRecv.TryGetValue(id, out func))
        {
            IPacket packet = func.Invoke(session, buffer); 
            if (onRecvCallBack != null)
                onRecvCallBack.Invoke(session, packet); 
            else
                HandlePacket(session, packet);
        }
    }

    T MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new() 
    {
        T p = new T();
        p.Read(buffer);
        return p;
    }

    public void HandlePacket(PacketSession session,IPacket p)
    {
        Action<PacketSession, IPacket> action = null;
        if (_handler.TryGetValue(p.Protocol, out action))
        {
            action.Invoke(session, p);
        }
    }
}
