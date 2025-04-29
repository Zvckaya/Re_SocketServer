// SessionManager.cs
using ServerCore;
using System;
using System.Collections.Generic;

namespace ChatServer
{
    class SessionManager : IJobQueue
    {
        static SessionManager _instance = new SessionManager();
        public static SessionManager Instance => _instance;

        Dictionary<int, ChatSession> _sessions = new Dictionary<int, ChatSession>();
        JobQueue _jobQueue = new JobQueue();
        int _sessionId = 0;

        public void Push(Action job)
        {
            _jobQueue.Push(job);
        }

        public ChatSession Generate()
        {
            lock (this)
            {
                int id = ++_sessionId;
                ChatSession session = new ChatSession { SessionId = id };
                return session;
            }
        }

        public void Enter(ChatSession session)
        {
            _sessions.Add(session.SessionId, session);
            Program.Room.Push(() => Program.Room.Enter(session));
        }

        public void Remove(ChatSession session)
        {
            _sessions.Remove(session.SessionId);
        }
    }
}
