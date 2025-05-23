﻿

using ServerCore;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DummyClient
{
    class Program
    {
        static void Main(string[] arg)
        {
            //DNS Domain Name System 사용 
            string host = Dns.GetHostName(); // 내 local 컴퓨터의 host 
            IPHostEntry ipHost = Dns.GetHostEntry(host); //hostname을 이용하여 iphostentry를 구한다
            IPAddress ipAddr = ipHost.AddressList[0]; //addresslsit에서 첫번째, 즉 자신 ip주소를 구함
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777); // 접속할 엔드포인트 정의


            Connector connector = new Connector();

            connector.Connect(endPoint, () => { return SessionManager.Instance.Generate(); },10);

            while (true)
            {
                try
                {
                    SessionManager.Instance.SendForEach(); // 모든 세션들이 채팅 메시지를 서버쪽으로 전송 
                }
                catch (Exception ex)
                {
                }

                Thread.Sleep(250);
            }


        }
    }

}