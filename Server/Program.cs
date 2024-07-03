using System.Net;
using System.Net.Sockets;
using System.Text;
using ServerCore;
using System.Net;
using System.Text;

namespace Server
{
    class Knight
    {
        public int hp;
        public int attack;

    }

    class GameSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"[OnConnected]:{endPoint.ToString()}");
            Knight knight = new Knight() { hp = 100, attack = 100 };

            byte[] buffer =  BitConverter.GetBytes(knight.hp); // BitConvertor를 이용하여 int의 값을 4byte 배열로 변환 할수 있다 
            byte[] buffer2 = BitConverter.GetBytes(knight.attack); // 4bytez`


            ArraySegment<byte> openSegemnt = SendBufferHelper.Open(4096);
            Array.Copy(buffer, 0, openSegemnt.Array, openSegemnt.Offset, buffer.Length);
            Array.Copy(buffer2, 0, openSegemnt.Array, openSegemnt.Offset+ buffer.Length, buffer2.Length); // int byte만큼 더해줘야함 
            ArraySegment<byte> sendBuff = SendBufferHelper.Close(buffer.Length + buffer2.Length);


            Send(sendBuff);
            Thread.Sleep(500);
            Disconnect();
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"[Disconnect] {endPoint.ToString()}");
        }

        public override int OnRecv(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"[From Client] {recvData}");

            return buffer.Count;
        }

        public override void OnSend(int numOfByte)
        {
            Console.WriteLine($"Transferred bytes:{numOfByte}");

        }
    }

    class Program
    {
        static Listener _listenr = new Listener();

        static void Main(string[] args)
        {
            //DNS Domain Name System 사용 
            string host = Dns.GetHostName(); // 내 local 컴퓨터의 host 
            IPHostEntry ipHost = Dns.GetHostEntry(host); //hostname을 이용하여 iphostentry를 구한다
            IPAddress ipAddr = ipHost.AddressList[0]; //addresslsit에서 첫번째, 즉 자신 ip주소를 구함
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777); // 접속할 엔드포인트 정의



            _listenr.Init(endPoint, () => { return new GameSession(); });
            Console.WriteLine("Listening");

            while (true)
            {
                ;
            }
        }
    }

}