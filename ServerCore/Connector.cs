using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    class Connector
    {
        Func<Session> _sessionFactory;

        //멤버 변수 형식으로 받지 않는 이유
        //다중 접속이 일어날 수 있기 떄문에 인스턴스별로 구현 
        public void Connect(IPEndPoint endPoint, Func<Session> sessionFactory)
        {
            Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _sessionFactory = sessionFactory;

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += OnConnectedCompleted;
            args.RemoteEndPoint = endPoint;
            args.UserToken = socket;

            RegisterConncet(args);
        }

        void RegisterConncet(SocketAsyncEventArgs args)
        {
            Socket socket = args.UserToken as Socket;
            if (socket == null)
                return;

            bool pending = socket.AcceptAsync(args);
            if (!pending)
                OnConnectedCompleted(null, args);
        }

        void OnConnectedCompleted(object sender, SocketAsyncEventArgs args) 
        {
            if(args.SocketError == SocketError.Success)
            {
                Session session = _sessionFactory.Invoke();
                session.Start(args.ConnectSocket); // 세션을 사용할 때는 Socket이 있어야한다.
                session.OnConnected(args.RemoteEndPoint);
            }
            else
            {
                Console.WriteLine($"OnConnectedCompleted Fail{args.SocketError}");
            }
        }
    }
}
