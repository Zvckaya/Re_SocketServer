
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

                    Console.WriteLine(r.Name+" " + r["name"]);
                }
            }


        }
    }
}