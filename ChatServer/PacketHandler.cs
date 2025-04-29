using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    class PacketHandler
    {
        public static void C_ChatHandler(PacketSession session, IPacket packet)
        {
            C_Chat chatPacket = packet as C_Chat;
            ChatSession chatSession = session as ChatSession;

            if (chatSession == null || chatSession.Room == null)
                return;

            ChatRoom room = chatSession.Room;
            room.Push(() => room.Chat(chatSession, chatPacket.message));
        }
    }
}
