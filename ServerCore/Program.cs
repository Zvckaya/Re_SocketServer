
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
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


            try
            {
                _listenr.Init(endPoint);
             
                while (true)
                {
                    Console.WriteLine("Listening");

                    Socket clientSocket = _listenr.Accept(); // accept를 하면 Socket을 반환한다(문제 없을시) blocking 방식-> 입장안하면 안넘어감

                    //recv
                    byte[] recvBuff = new byte[1024];
                    int recvBytes = clientSocket.Receive(recvBuff); // 몇바이트를 받았는가?
                    string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes); // recvbuff에 받은 recvbyte를 인코딩 
                    Console.WriteLine($"[From Client] {recvData}");

                    //send
                    byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to Server!"); // string을 utf8 byte로 변환
                    clientSocket.Send(sendBuff);
                        
                    //end
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();


                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }


            
        }
    }

}