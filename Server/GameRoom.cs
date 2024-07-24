using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Server
{
    class GameRoom : IJobQueue
    {
        List<ClientSession> _sessions = new List<ClientSession>();
        JobQueue _jobQueue = new JobQueue();
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>(); 

        public void Push(Action job)
        {
            _jobQueue.Push(job);
        }



        public void BroadCast(ClientSession clientSession, string chat)
        {
            S_Chat packet = new S_Chat();
            packet.playerId = clientSession.SessionId;
            packet.chat = $" {chat} I am {packet.playerId}";
            ArraySegment<byte> segment = packet.Write();
            //멀티 스레드 영역 진입 
            _pendingList.Add(segment); // 펜딩 리스트에 추가 


        }

        public void Enter(ClientSession session)
        {

            _sessions.Add(session);
            session.room = this;


        }

        public void Leave(ClientSession session)
        {

            _sessions.Remove(session);

        }

        public void Flush()
        {
            foreach(ClientSession s in _sessions)
            {
                s.Send(_pendingList);
            }

            Console.WriteLine($"Flushed {_pendingList.Count} items");
            _pendingList.Clear();
        }
    }
}
