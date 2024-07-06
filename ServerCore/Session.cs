using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public abstract class PacketSession: Session
    {
        public static readonly int HeaderSize = 2;
        //[size(2)][packetId(2)][.....] <- 이 패킷의 모습이 될것이다.

        public sealed override int OnRecv(ArraySegment<byte> buffer) //seal를 통해 재 override를 제한함
        {
            int processLen = 0; //몇 byte 처리했는가?
            while (true)
            {
                //최소한 헤더는 파싱할 수 있는가? 
                if (buffer.Count < HeaderSize)
                {
                    Console.WriteLine($"해설할 패킷 없음{buffer.Count}");
                    break;
                }
                    

                //패킷이 완전체로 도착했는가? 
                ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
                Console.WriteLine($"dataSize:{dataSize}");
                if (buffer.Count < dataSize)
                {
                    Console.WriteLine("데이터 사이즈 이상");
                    break;
                }
             

                //여기 도달했으면 패킷 조립가능 
                OnRecvPacket(new ArraySegment<byte>(buffer.Array,buffer.Offset,dataSize));//파싱한 패킷 전체를 전달->스택에 복사
                //Console.WriteLine(" 패킷 전달 ");

                processLen += dataSize;
                buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + dataSize, buffer.Count - dataSize); //다음 부분을 선택 
            }
            return processLen;
        }

        public abstract void OnRecvPacket(ArraySegment<byte> a); //컨텐츠에서 유효한 패킷을 해석 
    }

    public abstract class Session
    {
        Socket _socket;
        int _disconnected = 0;

        RecvBuffer _recvBuffer = new RecvBuffer(1024); //Recv 버퍼사용 

        object _lock = new object();
        Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();


        SocketAsyncEventArgs sendArgs;
        SocketAsyncEventArgs recvArgs;
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();

        public abstract void OnConnected(EndPoint endPoint);
        public abstract int OnRecv(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfByte);
        public abstract void OnDisconnected(EndPoint endPoint);



        public void Start(Socket socket)
        {
            _socket = socket; //accept한 인증된 socket

            recvArgs = new SocketAsyncEventArgs(); //수신 담당 소켓 이벤트 
            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);


            sendArgs = new SocketAsyncEventArgs(); //수신 담당 소켓 이벤트 
            sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted); //콜백 메소드 등록

            RegisterRecv(); //첫 등록은 수동으로 해주어야함 
        }

        public void Send(ArraySegment<byte> sendBuff) //매번 송신할때마다 비동기적으로 호출할 것인가?
        {
            lock (_lock) // Send를 동시 호출할 수도 있어서
            {
                _sendQueue.Enqueue(sendBuff);
                if (_pendingList.Count == 0) //만약 송신 대기가 없으면?
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
                ArraySegment<byte> buff = _sendQueue.Dequeue();
                _pendingList.Add(buff);
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
            _recvBuffer.Clean(); // 혹시라도 커서가 너무 뒤로 이동하는 것 방지 
            ArraySegment<byte> segment = _recvBuffer.WriteSegment; //Recv에 사용할 segent
            recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);  // 이만큼 사용가능 

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
                    //Write 커서 이동
                    if (_recvBuffer.OnWrite(args.BytesTransferred) == false)
                    {
                        Disconnect();
                        return;
                    }

                    //컨텐츠쪽으로 데이터를 넘긴 후 얼마나 처리했는지 받는다. 
                    int processLen = OnRecv(_recvBuffer.ReadSegment);  // ->OnRecv는 컨텐츠에서 OnRecv 핸들링을 해주기 위해 사용됨 
                    if(processLen < 0 || _recvBuffer.DataSzie < processLen)
                    {
                        Disconnect();
                        return;
                    }

                    //처리를 한 Read커서 이동
                    if (_recvBuffer.OnRead(processLen) == false)
                    {
                        Disconnect();
                        return;
                    }

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
