using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    class Session
    {
        Socket _socket;
        int _disconnected = 0;

        object _lock = new object();
        Queue<byte[]> _sendQueue = new Queue<byte[]>();
        bool _pending = false; 

        SocketAsyncEventArgs sendArgs;
        SocketAsyncEventArgs recvArgs;
        public void Start(Socket socket)
        {
            _socket = socket; //accept한 인증된 socket

            recvArgs = new SocketAsyncEventArgs(); //수신 담당 소켓 이벤트 
            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            recvArgs.SetBuffer(new byte[1024], 0, 1024); //수신에 사용할 버퍼 등록

            sendArgs = new SocketAsyncEventArgs(); //수신 담당 소켓 이벤트 
            sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv(recvArgs); //첫 등록은 수동으로 해주어야함 
        }

        public void Send(byte[] sendBuff) //매번 송신할때마다 비동기적으로 호출할 것인가?
        {
            lock (_lock) // Send를 동시 호출할 수도 있어서
            {
                _sendQueue.Enqueue(sendBuff);
                if (_pending == false) //만약 송신 대기가 없으면?
                    RegisterSend();
            }
          
        }

        public void Disconnect()
        {
            if (Interlocked.Exchange(ref _disconnected, 1) == 1) // Interlocked.Exchange 은 교환전의 숫자를 밷어줌 
                return;

            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }



        #region 네트워크 통신(외부통신)

        void RegisterSend()
        {
            _pending = true; // 동시 Send 호출 방지
            byte[] buff = _sendQueue.Dequeue();
            sendArgs.SetBuffer(buff, 0, buff.Length);

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
                        if (_sendQueue.Count != 0) //검사를 해야하는 이유->pending이 걸려있는 상태에서 단순 데이터 삽입만 될 수 있기 때문
                            RegisterSend();
                        else
                            _pending = false;

                        
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

        void RegisterRecv(SocketAsyncEventArgs args)
        {

            bool pending = _socket.ReceiveAsync(args);
            if (!pending)
                OnRecvCompleted(null, args);
        }

        void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success) //수신 데이터가 존재하고 , 소켓 통신 성공했을때
            {
                try
                {
                    string recvData = Encoding.UTF8.GetString(args.Buffer, args.Offset, args.BytesTransferred);
                    Console.WriteLine($"[From Client] {recvData}");
                    RegisterRecv(args); //재사용을 위한 재등록 
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
