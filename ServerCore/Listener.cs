using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    class Listener
    {
        Socket _listenSocket;

        public void Init(IPEndPoint endpoint)
        {
            _listenSocket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            _listenSocket.Bind(endpoint);

            _listenSocket.Listen(10);

            SocketAsyncEventArgs e = new SocketAsyncEventArgs();
            e.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted); // 소켓 비동기 이벤트가 완료되면 즉 accept 되면 
            RegisterAccept(e);  //맨 최초로 acceptasync에 대한 control flow를 등록해준다. 

        }

        void RegisterAccept(SocketAsyncEventArgs args)
        {
            bool pending = _listenSocket.AcceptAsync(args); //만약 실행과 동시에 성공하면 소켓 비동기 이벤트를 감시함.
            //AcceptAsync는 pending 상태이면 대기하다 자동으로 실행 
            if (!pending)
                OnAcceptCompleted(null,args);

        }

        void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                //실제 접속후 할 일


            }
            else
                Console.WriteLine(args.SocketError.ToString());

            RegisterAccept(args); //다음 클라이언트의 accept 위해 재등록 

        }

       
    }
}
