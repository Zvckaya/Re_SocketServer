using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class PacketHandler
{
    //public static void PlayerInfoReqHandler(PacketSession session, IPacket packet) //세션과 패킷을 받아옴
    //{
    //    PlayerInfoReq p = packet as PlayerInfoReq;

    //    Console.WriteLine($"PlayerInfoReq: {p.playerId} {p.name}");

    //    foreach (PlayerInfoReq.Skill skill in p.skills)
    //    {
    //        Console.WriteLine($"Skill({skill.id})({skill.level})({skill.duration})");
    //    }
    //}

    public static void S_TestHandler(PacketSession session, IPacket packet)
    {

    }

    internal static void S_ChatHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }
}