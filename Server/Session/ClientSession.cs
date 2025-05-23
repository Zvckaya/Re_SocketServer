﻿using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server
{

    class ClientSession : PacketSession
    {
        public int SessionId { get; set; }
        public GameRoom room { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }

        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"[OnConnected]:{endPoint.ToString()}");

            // 원래는 클라이언트 리소스 로딩 완료시 입장해야함.
            Program.Room.Push(() => Program.Room.Enter(this));

        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            SessionManager.Instance.Remove(this);
            if(room != null)
            {
                GameRoom Room = room;
                Room.Push(() => Program.Room.Leave(this));
                room=null; 
            }
            Console.WriteLine($"[Disconnect] {endPoint.ToString()}");
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnSend(int numOfByte)
        {
            //Console.WriteLine($"Transferred bytes:{numOfByte}");

        }
    }
}
