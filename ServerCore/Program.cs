
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    class Program
    {
        static void Main(string[] args)
        {
            //DNS Domain Name System 사용 
            string host = Dns.GetHostName(); // 내 local 컴퓨터의 host 
            IPHostEntry ipHost = Dns.GetHostEntry(host); //hostname을 이용하여 iphostentry를 구한다
            IPAddress ipAddr = ipHost.AddressList[0]; //addresslsit에서 첫번째, 즉 자신 ip주소를 구함
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777); // 접속할 엔드포인트 정의

            //리슨소켓 생성
            Socket listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp); //endpoint, 소켓타입 스트림, tcp사용


            try
            {
                //바인딩
                listenSocket.Bind(endPoint); //주소와 해당 포트번호를 리슨 소켓으로 

                listenSocket.Listen(10); // backlog를 받고 있다 -> backlog=최대 대기수-> accept를 처리하고 있을때 기다릴 수 있는 대기수
                while (true)
                {
                    Console.WriteLine("Listening");

                    Socket clientSocket = listenSocket.Accept(); // accept를 하면 Socket을 반환한다(문제 없을시) blocking 방식-> 입장안하면 안넘어감

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