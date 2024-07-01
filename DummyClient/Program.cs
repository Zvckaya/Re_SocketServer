

using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DummyClient
{
    class Program
    {
        static void Main(string[] arg)
        {
            //DNS Domain Name System 사용 
            string host = Dns.GetHostName(); // 내 local 컴퓨터의 host 
            IPHostEntry ipHost = Dns.GetHostEntry(host); //hostname을 이용하여 iphostentry를 구한다
            IPAddress ipAddr = ipHost.AddressList[0]; //addresslsit에서 첫번째, 즉 자신 ip주소를 구함
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777); // 접속할 엔드포인트 정의

            

            while (true)
            {
                Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    socket.Connect(endPoint);// listen socket의 주소가 있어야함 
                    Console.WriteLine($"Connected to {socket.RemoteEndPoint.ToString()}"); //

                    byte[] sendBuff = Encoding.UTF8.GetBytes("Hello World");
                    //for(int i =0; i < sendBuff.Length; i++) { 
                    //    Console.Write(sendBuff[i]);
                    //}
                    //Console.WriteLine( );

                    int sendByte = socket.Send(sendBuff);

                    byte[] recvBuff = new byte[1024];
                    int recvByte = socket.Receive(recvBuff);
                    string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvByte);
                    Console.WriteLine($"[From Server] {recvData}");

                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                Thread.Sleep(100);
            }


        }
    }

}