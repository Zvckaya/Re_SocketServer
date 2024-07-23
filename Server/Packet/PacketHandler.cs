using Server;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


//수동으로 관리 
class PacketHandler
{
    
    public static void TestHandler(PacketSession session, IPacket packet)
    {

    }

    public static void C_ChatHandler(PacketSession session, IPacket packet)
    {
        C_Chat chatPacket = packet as C_Chat;
        ClientSession clientSession = session as ClientSession;

        if (clientSession.room == null)
            return;
        clientSession.room.Push(() => clientSession.room.BroadCast(clientSession, chatPacket.chat));
        //JobQueu 사용 
        //clientSession.room.BroadCast(clientSession,chatPacket.chat);
    }
}
