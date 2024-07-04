

using ServerCore;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DummyClient
{
    class Packet
    {
        public ushort size;
        public ushort packetId;
    }

    class GameSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"[OnConnected]:{endPoint.ToString()}");


            Packet packet = new Packet() { size = 4, packetId = 7 };

            ArraySegment<byte> openSegemnt = SendBufferHelper.Open(4096);
            byte[] buffer = BitConverter.GetBytes(packet.size); // BitConvertor를 이용하여 int의 값을 4byte 배열로 변환 할수 있다 
            byte[] buffer2 = BitConverter.GetBytes(packet.packetId); // 4bytez
            Array.Copy(buffer, 0, openSegemnt.Array, openSegemnt.Offset, buffer.Length);
            Array.Copy(buffer2, 0, openSegemnt.Array, openSegemnt.Offset + buffer.Length, buffer2.Length); // int byte만큼 더해줘야함 
            ArraySegment<byte> sendBuff = SendBufferHelper.Close(packet.size);
            Send(sendBuff);

        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"[Disconnect] {endPoint.ToString()}");
        }

        public override int OnRecv(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, 0, buffer.Count);
            Console.WriteLine($"[From Server] {recvData}");
            return buffer.Count;
        }

        public override void OnSend(int numOfByte)
        {
            Console.WriteLine($"Transferred bytes:{numOfByte}");
        }
    }


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

            connector.Connect(endPoint, () => { return new GameSession(); });

            while (true)
            {
                try
                {

                }
                catch (Exception ex)
                {
                }

                Thread.Sleep(1000);
            }


        }
    }

}