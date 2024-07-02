using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public abstract class Session
    {
        Socket _socket;
        int _disconnected = 0;

        object _lock = new object();
        Queue<byte[]> _sendQueue = new Queue<byte[]>();


        SocketAsyncEventArgs sendArgs;
        SocketAsyncEventArgs recvArgs;
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();

        public abstract void OnConnected(EndPoint endPoint);
        public abstract void OnRecv(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfByte);
        public abstract void OnDisconnected(EndPoint endPoint);


        public void Start(Socket socket)
        {
            _socket = socket; //accept한 인증된 socket

            recvArgs = new SocketAsyncEventArgs(); //수신 담당 소켓 이벤트 
            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            recvArgs.SetBuffer(new byte[1024], 0, 1024); //수신에 사용할 버퍼 등록

            sendArgs = new SocketAsyncEventArgs(); //수신 담당 소켓 이벤트 
            sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted); //콜백 메소드 등록

            RegisterRecv(); //첫 등록은 수동으로 해주어야함 
        }

        public void Send(byte[] sendBuff) //매번 송신할때마다 비동기적으로 호출할 것인가?
        {
            lock (_lock) // Send를 동시 호출할 수도 있어서
            {
                _sendQueue.Enqueue(sendBuff);
                if (_pendingList.Count==0) //만약 송신 대기가 없으면?
                    RegisterSend();
            }
          
        }

        public void Disconnect()
        {
            if (Interlocked.Exchange(ref _disconnected, 1) == 1) // Interlocked.Exchange 은 교환전의 숫자를 밷어줌 
                return;

            OnDisconnected(_socket.RemoteEndPoint);
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }



        #region 네트워크 통신(외부통신)

        void RegisterSend()
        {
            //실제 보내는 리스트와 대기 리스트가 따로 존재함 

            while (_sendQueue.Count > 0) // send큐가 빌떄까지
            {
                byte[] buff = _sendQueue.Dequeue();
                _pendingList.Add(new ArraySegment<byte>(buff, 0, buff.Length));
            }
            sendArgs.BufferList = _pendingList; //대기 리스트의 목록을 실제 송신 리스트에 올림

            bool pending = _socket.SendAsync(sendArgs);
            if (!pending)
                OnSendCompleted(null, sendArgs);
        }

        void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            lock (_lock)  // 콜백형식으로 OnSendCompleted 
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        sendArgs.BufferList = null;
                        _pendingList.Clear(); //대기 목록을 초기화함 
                        OnSend(args.BytesTransferred); 

                        if (_sendQueue.Count != 0) //검사를 해야하는 이유->pending이 걸려있는 상태에서 단순 데이터 삽입만 될 수 있기 때문
                            RegisterSend();  // pendingList.Clear를 했다
                        

                        
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
                else
                {
                    Disconnect();
                }
            }
          
        }

        void RegisterRecv()
        {

            bool pending = _socket.ReceiveAsync(recvArgs);
            if (!pending)
                OnRecvCompleted(null, recvArgs);
        }

        void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success) //수신 데이터가 존재하고 , 소켓 통신 성공했을때
            {
                try
                {
                    OnRecv(new ArraySegment<byte>(args.Buffer, args.Offset, args.BytesTransferred));
                   
                    RegisterRecv(); //재사용을 위한 재등록 
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            else
            {
                Disconnect();
            }
        }
        #endregion
    }
}
