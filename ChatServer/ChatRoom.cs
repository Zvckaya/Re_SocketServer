// ChatRoom.cs
using ChatServer.Packet;
using ChatServer.Session;
using ServerCore;
using System;
using System.Collections.Generic;

namespace ChatServer
{
    class ChatRoom : IJobQueue
    {
        List<ChatSession> _sessions = new List<ChatSession>();
        JobQueue _jobQueue = new JobQueue();
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();

        public void Push(Action job)
        {
            _jobQueue.Push(job);
        }

        public void Broadcast(ArraySegment<byte> segment)
        {
            // 디버그용: PacketSession 파서 재사용
            S_Chat pkt = new S_Chat();
            pkt.Read(segment);
            Console.WriteLine($"[Broadcast] {pkt.message}");

            _pendingList.Add(segment);
        }

        public void Enter(ChatSession session)
        {
            _sessions.Add(session);
            session.Room = this;

            // Notify others that someone joined
            S_Chat chat = new S_Chat { message = $"[System] User {session.SessionId} joined." };
            Broadcast(chat.Write());
        }

        public void Leave(ChatSession session)
        {
            _sessions.Remove(session);

            S_Chat chat = new S_Chat { message = $"[System] User {session.SessionId} left." };
            Broadcast(chat.Write());
        }

        public void Chat(ChatSession session, string message)
        {
            S_Chat chat = new S_Chat { message = $"User {session.SessionId}: {message}" };
            Broadcast(chat.Write());
        }

        public void Flush()
        {
            foreach (ChatSession session in _sessions)
            {
                session.Send(_pendingList);
            }

            _pendingList.Clear();
        }
    }
}
