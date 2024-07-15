
using System.Security.Cryptography.X509Certificates;
using System.Xml;

namespace PacketGenerator
{
    class Program
    {
        static string genPackets;

        static void Main(string[] args)
        {
            XmlReaderSettings settings = new XmlReaderSettings()
            {
                IgnoreComments = true,
                IgnoreWhitespace = true,
            };

            using (XmlReader r = XmlReader.Create("PDL.xml", settings))
            {
                r.MoveToContent();//헤더를 건너뜀 

                while (r.Read())//stream방식으로 read-> 1줄씩 
                {

                    if (r.Depth == 1 && r.NodeType == XmlNodeType.Element)//endelement parsing 방지
                        ParsePacket(r);

                }

                File.WriteAllText("GenPackets.cs", genPackets);
            }

        }

        public static void ParsePacket(XmlReader r)
        {
            if (r.NodeType == XmlNodeType.EndElement)
                return;
            if (r.Name.ToLower() != "packet")
                return;

            string packetName = r["name"];
            if (string.IsNullOrEmpty(packetName))
            {
                Console.WriteLine("Packet without name");
                return;
            }

            Tuple<string,string,string> t= ParseMenber(r); //멤버 하나하나 파싱
            genPackets += string.Format(PacketFormat.packetFormat, packetName, t.Item1, t.Item2, t.Item3);
        }

        // {1} 멤버변수
        // {2} 멤버변수 Read
        // {3} 멤버번수 Write

        private static Tuple<string,string,string> ParseMenber(XmlReader r) //정보 긁기
        {
            string packetName = r["name"];
            string memeberCode = "";
            string readCode = "";
            string writeCode = "";

            int depth = r.Depth + 1; // 깊이 1-> playerInfoReq 깊이 2 실제 long string 형 변수
            while (r.Read())
            {
                if (r.Depth != depth)
                    break;

                string memberName = r["name"];
                if (string.IsNullOrEmpty(memberName))
                {
                    Console.WriteLine("member witout name");
                    return null;
                }

                string memberType = r.Name.ToLower();
                switch (memberType)
                {
                    case "bool":
                    case "byte":
                    case "short":
                    case "ushort":
                    case "int":
                    case "long":
                    case "float":
                    case "double":
                    case "string":
                    case "list":
                        break;
                    default:
                        break;

                }

            }

            return new Tuple<string, string, string>(memeberCode, readCode, writeCode);
        }
    }
}