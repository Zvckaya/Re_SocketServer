using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Server
{
    class GameRoom
    {
        List<ClientSession> _sessions = new List<ClientSession>();
        object _lock = new object();

        public void BroadCast(ClientSession clientSession, string chat)
        {
            S_Chat packet = new S_Chat();
            packet.playerId = clientSession.SessionId;
            packet.chat = chat;
            ArraySegment<byte> segment = packet.Write();
            //멀티 스레드 영역 진입 

            lock (_lock)
            {
                foreach (ClientSession session in _sessions)
                {
                    session.Send(segment);
                }
            }

        }

        public void Enter(ClientSession session)
        {
            lock (_lock)
            {
                _sessions.Add(session);
                session.room = this;
            }

        }

        public void Leave(ClientSession session) {
            lock (_lock)
            {
                _sessions.Remove(session);
            }
        }

        
    }
}
