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
        Action<Socket> _onAcceptHandler;

        public void Init(IPEndPoint endpoint,Action<Socket> onAcceptHandler) // onAcceptHandler는 callback 방식으로 통신성공 후 할 일을 받아옴->다른 객체에서
        {
            _listenSocket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _onAcceptHandler += onAcceptHandler;

            _listenSocket.Bind(endpoint);


            //backlog - 최대 대기 수 
            _listenSocket.Listen(10);


            //아래의 SocketAsyncEventArgs를 늘리면 처리할 수 있는 이벤트가 늘어남
            SocketAsyncEventArgs e = new SocketAsyncEventArgs();
            e.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted); // 소켓 비동기 이벤트가 완료되면 즉 accept 되면 
            RegisterAccept(e);  //맨 최초로 acceptasync에 대한 control flow를 등록해준다. 

        }

        void RegisterAccept(SocketAsyncEventArgs args)
        {
            args.AcceptSocket = null;
            // 기존 SocketAsyncEventArgs를 재사용 하고 있기 때문에 기존 내용을 초기화 시켜주어야 함.

            bool pending = _listenSocket.AcceptAsync(args); //만약 실행과 동시에 성공하면 소켓 비동기 이벤트를 감시함.
            //AcceptAsync는 pending 상태이면 대기하다 자동으로 실행 
            if (!pending)  //fasle 즉, 대기 상태가 아니면 
                OnAcceptCompleted(null,args);

        }

        void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                //실제 accept 완료 후 할 일
                _onAcceptHandler.Invoke(args.AcceptSocket); //애초에 성공했을때이기 때문에 무조건 acceptSocket은 존재한다.

            }
            else
                Console.WriteLine(args.SocketError.ToString());

            RegisterAccept(args); //다음 클라이언트의 accept 위해 재등록 

        }

       
    }
}
