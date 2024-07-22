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

        public void Enter(ClientSession session)
        {
            _sessions.Add(session);
        }

        public void Leave(ClientSession session) {
            _sessions.Remove(session);
        }
    }
}
