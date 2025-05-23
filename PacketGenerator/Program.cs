﻿
using System.Security.Cryptography.X509Certificates;
using System.Xml;

namespace PacketGenerator
{
    class Program
    {
        static string genPackets;
        static ushort packetId;
        static string packetEnum;

        static string clientRegister;
        static string serverRegister;

        static void Main(string[] args)
        {
            string pdlPath = "../PDL.xml";

            XmlReaderSettings settings = new XmlReaderSettings()
            {
                IgnoreComments = true,
                IgnoreWhitespace = true,
            };

            if (args.Length >= 1)
                pdlPath = args[0];

            using (XmlReader r = XmlReader.Create(pdlPath, settings))
            {
                r.MoveToContent();//헤더를 건너뜀 

                while (r.Read())//stream방식으로 read-> 1줄씩 
                {

                    if (r.Depth == 1 && r.NodeType == XmlNodeType.Element)//endelement parsing 방지
                        ParsePacket(r);

                }

                string fileText= string.Format(PacketFormat.fileFormat, packetEnum, genPackets);
                File.WriteAllText("GenPackets.cs", fileText);
                string clientmanagerText = string.Format(PacketFormat.managerFormat, clientRegister);
                File.WriteAllText("ClientPacketManager.cs", clientmanagerText);
                string servermanagerText = string.Format(PacketFormat.managerFormat, serverRegister);
                File.WriteAllText("ServerPacketManager.cs", servermanagerText);
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

            Tuple<string,string,string> t= ParseMember(r); //멤버 하나하나 파싱
            genPackets += string.Format(PacketFormat.packetFormat, packetName, t.Item1, t.Item2, t.Item3);
            packetEnum += string.Format(PacketFormat.packetEnumFormat, packetName, ++packetId) + Environment.NewLine+"\t";
            if(packetName.StartsWith("S_") || packetName.StartsWith("s_"))
                clientRegister += string.Format(PacketFormat.managerRegisterFormat, packetName) + Environment.NewLine;
            else
                serverRegister += string.Format(PacketFormat.managerRegisterFormat, packetName) + Environment.NewLine;
        }

        // {1} 멤버변수
        // {2} 멤버변수 Read
        // {3} 멤버번수 Write

        private static Tuple<string,string,string> ParseMember(XmlReader r) //정보 긁기
        {
            string packetName = r["name"];
            string memberCode = "";
            string readCode = "";
            string writeCode = "";

            int depth = r.Depth + 1; // 깊이 1-> playerInfoReq 깊이 2 실제 long string 형 변수
            while (r.Read())
            {
                if (r.Depth != depth)
                    break;

                string memberName = r["name"];
                if (string.IsNullOrEmpty(memberName))  //pdl 을 한줄 씩 긁어온다
                {
                    Console.WriteLine("member witout name");
                    return null;
                }

                if (string.IsNullOrEmpty(memberCode) == false)
                    memberCode += Environment.NewLine; // 가상 enter 
                if (string.IsNullOrEmpty(readCode) == false)
                    readCode += Environment.NewLine; // 가상 enter 
                if (string.IsNullOrEmpty(writeCode) == false)
                    writeCode += Environment.NewLine; // 가상 enter 

                string memberType = r.Name.ToLower();
                switch (memberType)
                {
                    case "byte":
                    case "sbyte":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.readByteFormat, memberName, memberType);
                        writeCode += string.Format(PacketFormat.writeByteFormat, memberName, memberType);
                        break;
                    case "bool":
                    case "short":
                    case "ushort":
                    case "int":
                    case "long":
                    case "float":
                    case "double":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType,memberName);
                        readCode += string.Format(PacketFormat.readFormat, memberName, ToMemberType(memberType), memberType);
                        writeCode += string.Format(PacketFormat.writeFormat, memberName, memberType);
                        break;
                    case "string":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.readStringFormat, memberName);
                        writeCode += string.Format(PacketFormat.writeStringFormat, memberName);
                        break;
                    case "list":
                        Tuple<string,string,string> t= ParseList(r);
                        memberCode += t.Item1;
                        readCode += t.Item2;
                        writeCode += t.Item3;
                        break;
                    default:
                        break;

                }

            }

            memberCode.Replace("\n", "\n\t");
            readCode.Replace("\n", "\n\t\t");
            writeCode.Replace("\n", "\n\t\t");
            return new Tuple<string, string, string>(memberCode, readCode, writeCode);
        }

        private static Tuple<string,string,string> ParseList(XmlReader r)
        {
            string listName = r["name"];
            if (string.IsNullOrEmpty(listName))
            {
                Console.WriteLine("list without name");
                return null;
            }

            Tuple<string,string,string> t= ParseMember(r);

            string memberCode = string.Format(PacketFormat.memberListFormat,
                FirstCharToUpper(listName), FirstCharToLower(listName),
                t.Item1,
                t.Item2,
                t.Item3
                );

            string readCode = string.Format(PacketFormat.readListFormat, FirstCharToUpper(listName), FirstCharToLower(listName));

            string writeCode = string.Format(PacketFormat.writeListFormat, FirstCharToUpper(listName), FirstCharToLower(listName));

            return new Tuple<string, string, string>( memberCode, readCode, writeCode );

        }

        public static string ToMemberType(string memberType)
        {
            switch (memberType)
            {
                case "bool":
                    return "ToBoolean";
                case "byte":
                case "short":
                    return "ToInt16";
                case "ushort":
                    return "ToUInt16";
                case "int":
                    return "ToInt32";
                case "long":
                    return "ToInt64";
                case "float":
                    return "ToSingle";
                case "double":
                    return "ToDouble";
                default:
                    return "";
            }
        }

        public static string FirstCharToUpper(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            return input[0].ToString().ToUpper() + input.Substring(1);
        }

        public static string FirstCharToLower(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            return input[0].ToString().ToLower() + input.Substring(1);
        }
    }
}