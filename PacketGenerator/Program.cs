
using System.Security.Cryptography.X509Certificates;
using System.Xml;

namespace PacketGenerator
{
    class Program
    {

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
                    
                    //Console.WriteLine(r.Name+" " + r["name"]);
                }
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

            ParseMenber(r);
        }

        private static void ParseMenber(XmlReader r) //정보 긁기
        {
            string packetName = r["name"];

            int depth = r.Depth + 1; // 깊이 1-> playerInfoReq 깊이 2 실제 long string 형 변수
            while (r.Read())
            {
                if (r.Depth != depth)
                    break;

                string memberName = r["name"];
                if (string.IsNullOrEmpty(memberName))
                {
                    Console.WriteLine("member witout name");
                    return;
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
        }
    }
}