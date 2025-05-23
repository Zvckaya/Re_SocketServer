﻿using ServerCore;
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



        public void BroadCast(ArraySegment<byte> segment)
        {
           _pendingList.Add(segment);
        }

        public void Enter(ClientSession session)
        {

            _sessions.Add(session);
            session.room = this;

            // 입장시 원래 플레이어 목록 수신 
            S_PlayerList players = new S_PlayerList();
            foreach (ClientSession s in _sessions) {
                players.players.Add(new S_PlayerList.Player()
                {
                    isSelf = (s == session),
                    playerId = s.SessionId,
                    posX = s.PosX,
                    posY = s.PosY,
                    posZ = s.PosZ,
                }); 
            }
            session.Send(players.Write());

            S_BroadcastEnterGame enter = new S_BroadcastEnterGame();
            enter.playerId = session.SessionId;
            enter.posX = 0;
            enter.posY = 0;
            enter.posZ = 0;
            BroadCast(enter.Write());
        }

        public void Leave(ClientSession session)
        {

            _sessions.Remove(session);

            S_BroadcastLeaveGame leave = new S_BroadcastLeaveGame();
            leave.playerId = session.SessionId;

            BroadCast(leave.Write());
        }

        public void Move(ClientSession s, C_Move packet)
        {
            //좌표 갱신 
            s.PosX = packet.posX;
            s.PosY = packet.posY;
            s.PosZ = packet.posZ;

            //브로드캐스트
            S_BroadcastMove move = new S_BroadcastMove();
            move.playerId = s.SessionId;
            move.posX = s.PosX;
            move.posY = s.PosY;
            move.posZ = s.PosZ;

            BroadCast(move.Write());
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
