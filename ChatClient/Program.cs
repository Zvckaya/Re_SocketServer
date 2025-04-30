using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ServerCore;
using static System.Collections.Specialized.BitVector32;

namespace ChatClient
{
    class Program
    {
        static Socket _socket;
        static byte[] _recvBuffer = new byte[4096];

        static void Main(string[] args)
        {
            string host = Dns.GetHostName(); // 내 local 컴퓨터의 host 
            IPHostEntry ipHost = Dns.GetHostEntry(host); //hostname을 이용하여 iphostentry를 구한다
            IPAddress ipAddr = ipHost.AddressList[0]; //addresslsit에서 첫번째, 즉 자신 ip주소를 구함
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777); // 접속할 엔드포인트 정의

            Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _socket = socket;
            socket.Connect(endPoint);


            Console.WriteLine("[Client] Connected to server.");

            // 수신 쓰레드
            new Thread(RecvLoop) { IsBackground = true }.Start();

            while (true)
            {
                string msg = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(msg))
                    continue;

                byte[] sendBuffer = Make_C_Chat_Packet(msg);
                _socket.Send(sendBuffer);
            }
        }

        static void RecvLoop()
        {
            while (true)
            {
                int bytesReceived = _socket.Receive(_recvBuffer);
                if (bytesReceived == 0)
                    break;

                ParseAndPrintS_Chat(_recvBuffer, bytesReceived);
            }
        }

        static byte[] Make_C_Chat_Packet(string message)
        {
            ushort packetId = 1; // C_Chat
            ushort msgLen = (ushort)Encoding.UTF8.GetByteCount(message);
            ushort totalSize = (ushort)(2 + 2 + 2 + msgLen); // header + id + len + data

            byte[] buffer = new byte[totalSize];
            int offset = 0;

            // [0~1] total size
            BitConverter.TryWriteBytes(new Span<byte>(buffer, offset, 2), totalSize);
            offset += 2;

            // [2~3] packet id
            BitConverter.TryWriteBytes(new Span<byte>(buffer, offset, 2), packetId);
            offset += 2;

            // [4~5] string length
            BitConverter.TryWriteBytes(new Span<byte>(buffer, offset, 2), msgLen);
            offset += 2;

            // [6~] string data
            Encoding.UTF8.GetBytes(message, 0, message.Length, buffer, offset);

            return buffer;
        }

        static void ParseAndPrintS_Chat(byte[] buffer, int size)
        {
            int offset = 0;

            ushort totalSize = BitConverter.ToUInt16(buffer, offset);
            offset += 2;

            ushort packetId = BitConverter.ToUInt16(buffer, offset);
            offset += 2;

            if (packetId != 2) // S_Chat
                return;

            ushort msgLen = BitConverter.ToUInt16(buffer, offset);
            offset += 2;

            string message = Encoding.UTF8.GetString(buffer, offset, msgLen);
            Console.WriteLine(message);
        }
    }
}
