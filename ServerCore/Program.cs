
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    class Program
    {
        static Listener _listenr = new Listener();
        static void onAcceptHandler(Socket clientSocket)
        {
            Session session = new Session();
            try
            {
                session.Start(clientSocket);


                //send
                byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to Server!"); // string을 utf8 byte로 변환
                session.Send(sendBuff);

                Thread.Sleep(500);

                session.Disconnect();
                session.Disconnect();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        static void Main(string[] args)
        {
            //DNS Domain Name System 사용 
            string host = Dns.GetHostName(); // 내 local 컴퓨터의 host 
            IPHostEntry ipHost = Dns.GetHostEntry(host); //hostname을 이용하여 iphostentry를 구한다
            IPAddress ipAddr = ipHost.AddressList[0]; //addresslsit에서 첫번째, 즉 자신 ip주소를 구함
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777); // 접속할 엔드포인트 정의



            _listenr.Init(endPoint, onAcceptHandler);
            Console.WriteLine("Listening");

            while (true)
            {
                ;
            }
        }
    }

}