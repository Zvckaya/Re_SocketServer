using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class Connector
    {
        Func<Session> _sessionFactory;
        IPEndPoint _endPoint;
        int _retryCount = 0;
        const int MaxRetry = 5;
        const int RetryDelay = 3000;



        //멤버 변수 형식으로 받지 않는 이유
        //다중 접속이 일어날 수 있기 떄문에 인스턴스별로 구현 

        public void Connect(IPEndPoint endPoint, Func<Session> sessionFactory, int count = 1)
        {

            _sessionFactory = sessionFactory;
            _endPoint = endPoint;

            for (int i = 0; i < count; i++)
            {
                CreateSocket();
            }
        

        }

        void CreateSocket()
        {
            Socket socket = new Socket(_endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);//소켓 생성


            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += OnConnectedCompleted;
            args.RemoteEndPoint = _endPoint; //소켓 비동기 이벤트 엔드포인트 등록
            args.UserToken = socket; //생성한 소켓을 유저 토큰에 등록 

            RegisterConncet(args);
        }

        void RegisterConncet(SocketAsyncEventArgs args)
        {
            Socket socket = args.UserToken as Socket;
            if (socket == null)
                return;

            bool pending = socket.ConnectAsync(args);
            if (!pending)
                OnConnectedCompleted(null, args);
        }

        void OnConnectedCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                Session session = _sessionFactory.Invoke();
                session.Start(args.ConnectSocket); // 세션을 사용할 때는 Socket이 있어야한다.
                session.OnConnected(args.RemoteEndPoint);
            }
            else
            {
                Console.WriteLine($"OnConnectedCompleted Fail{args.SocketError}");
                if(_retryCount < MaxRetry)
                {
                    _retryCount++;
                    Console.WriteLine($"Retry {_retryCount} 회 시도중");

                    Task.Delay(RetryDelay).ContinueWith((t) =>
                    {
                        CreateSocket();
                    });
                }
                else
                {
                    Console.WriteLine("최대 재시도 횟수 도달.... 종료합니다.");
                }
            }
        }
    }
}
