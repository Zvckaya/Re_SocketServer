﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacketGenerator
{
    class PacketFormat
    {
        
        // {0} 패킷 등록
        public static string managerFormat =
@"using ServerCore;
class PacketManager
{{
    #region Singleton
    static PacketManager _instance = new PacketManager();
    public static PacketManager Instance
    {{
        get
        {{
   
            return _instance;
        }}
    }}
    #endregion

    PacketManager()
    {{
        Register();
    }}

    Dictionary<ushort, Func<PacketSession, ArraySegment<byte>,IPacket>> _onRecv =
        new Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>>();

    Dictionary<ushort, Action<PacketSession, IPacket>> _handler =
        new Dictionary<ushort, Action<PacketSession, IPacket>>();

    public void Register()
    {{
       {0}
    }}


    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer,Action<PacketSession,IPacket> onRecvCallBack = null)
    {{
        ushort count = 0;

        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        Func<PacketSession, ArraySegment<byte>,IPacket> func = null;
        if (_onRecv.TryGetValue(id, out func))
        {{
            IPacket packet = func.Invoke(session, buffer); 
            if (onRecvCallBack != null)
                onRecvCallBack.Invoke(session, packet); 
            else
                HandlePacket(session, packet);
        }}
    }}

    T MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new() 
    {{
        T p = new T();
        p.Read(buffer);
        return p;
    }}

    public void HandlePacket(PacketSession session,IPacket p)
    {{
        Action<PacketSession, IPacket> action = null;
        if (_handler.TryGetValue(p.Protocol, out action))
        {{
            action.Invoke(session, p);
        }}
    }}
}}
";

        // {0} 패킷 이름 
        public static string managerRegisterFormat =
@"      _onRecv.Add((ushort)PacketID.{0}, MakePacket<{0}>);
        _handler.Add((ushort)PacketID.{0}, PacketHandler.{0}Handler);
";



        //{0} 패킷 이름 / 번호목록
        //{1} 패킷 목록

        public static string fileFormat =
@"using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

public enum PacketID
{{
    {0}
}}

public interface IPacket
{{
    ushort Protocol {{ get; }}
    void Read(ArraySegment<byte> segment);
    ArraySegment<byte> Write();
}}

{1}
";

        // {0} 패킷이름
        // {1} 멤버변수
        // {2} 멤버변수 Read
        // {3} 멤버번수 Write

        public static string packetFormat =
@"
class {0} : IPacket
{{
    {1}

    public ushort Protocol {{ get {{ return (ushort)PacketID.{0}; }} }}
    
    public void Read(ArraySegment<byte> segment)
    {{
        ushort count = 0;

        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
        count += sizeof(ushort);
        count += sizeof(ushort);
        {2}

    }}

    public ArraySegment<byte> Write()
    {{
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        bool success = true;
        ushort count = 0;

        Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.{0});
        count += sizeof(ushort);
        {3}
        success &= BitConverter.TryWriteBytes(s, count); 
        ArraySegment<byte> sendBuff = SendBufferHelper.Close(count);

        if (sendBuff == null)
            return null;
        return sendBuff;
    }}
}}
";
        //{0} 변수 형식
        //{1} 변수 이름
        public static string memberFormat =
@" public {0} {1};
";

        // {0} 패킷 이름
        // {1} 패킷 번호
        public static string packetEnumFormat =
@"{0} = {1},
";

        // {0} 리스트 이름 (대문자)
        // {1} 리스트 이름 (소문자)
        // {2} 멤버 변수
        // {3} 멤버 변수 Read
        // {4} 멤버 변수 write
        public static string memberListFormat =
@"
public struct {0}
{{
    {2}

    public void Read(ReadOnlySpan<byte> s, ref ushort count)
    {{
        {3}
    }}

    public bool Write(Span<byte> s, ref ushort count)
    {{
        bool success = true; 
        {4}
        return success;
    }}
}}

public List<{0}> {1}s = new List<{0}>();

";

        //{0} 변수이름
        //{1} To~ 변수 형식
        //{2} 변수형식
        public static string readFormat =
@"this.{0} = BitConverter.{1}(s.Slice(count, s.Length - count)); 
count += sizeof({2});
";

        //{0} 변수 이름 
        //{1} 변수 형식
        public static string readByteFormat =
@"this.{0} = segment.Array[segment.Offset + count];
count += sizeof({1});
";

        //{0} 변수이름
        public static string readStringFormat =
@"ushort {0}Len = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
count += sizeof(ushort);
this.{0} = Encoding.Unicode.GetString(s.Slice(count, {0}Len));
count += {0}Len;
";

        //{0} 리스트 이름 대문자
        //{1} 리스트 이름 소문자

        public static string readListFormat =
@"this.{1}s.Clear();
ushort {1}Len = BitConverter.ToUInt16(s.Slice(count, s.Length - count)); 
count += sizeof(ushort);
for(int i =0; i< {1}Len; i++)
{{
    {0} {1} = new {0}();
    {1}.Read(s, ref count);
    {1}s.Add({1});
}}
";

        //{0} 변수이름
        //{1} 변수 형식
        public static string writeFormat =
@"success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.{0});
count += sizeof({1});
";

        //{0} 변수 이름 
        //{1} 변수 형식
        public static string writeByteFormat =
@"segment.Array[segment.Offset + count] = this.{0};
count += sizeof({1});
";

        //{0} 변수이름

        public static string writeStringFormat =
@"
ushort {0}Len  = (ushort)Encoding.Unicode.GetBytes(this.{0},0,this.{0}.Length,segment.Array,segment.Offset+count+sizeof(ushort));
success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length-count),{0}Len);
count += sizeof(ushort);
count += {0}Len;
";

        //{0} 리스트 이름 대문자
        //{1} 리스트 이름 소문자

        public static string writeListFormat =
@"
success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort){1}s.Count);
count += sizeof(ushort);
foreach({0} {1} in {1}s)
    success &= {1}.Write(s, ref count);
";

      

    }
}
