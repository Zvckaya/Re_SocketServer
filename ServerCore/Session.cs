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

        public void Start(Socket socket)
        {
            _socket = socket; //accept한 인증된 socket

            SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs(); //수신 담당 소켓 이벤트 
            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            recvArgs.SetBuffer(new byte[1024], 0, 1024); //수신에 사용할 버퍼 등록

            RegisterRecv(recvArgs); //첫 등록은 수동으로 해주어야함 
        }

        public void Send(byte[] sendBuff)
        {
            _socket.Send(sendBuff);
        }

        public void Disconnect()
        {
            if (Interlocked.Exchange(ref _disconnected, 1) == 1) // Interlocked.Exchange 은 교환전의 숫자를 밷어줌 
                return;

            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }

        #region 네트워크 통신(외부통신)
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
            }
        }
        #endregion
    }
}
