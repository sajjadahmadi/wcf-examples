//css_ref System.Runtime.Serialization.dll;
using System;
using System.IO;
using System.Xml;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml.Linq;

namespace WcfExamples.Messages
{
    class WriteMessageExample
    {
        [STAThread]
        static public void Main(string[] args)
        {
            var body = "Body";
            var stream = new MemoryStream();
            var xmlWriter = XmlDictionaryWriter.CreateTextWriter(stream);

            var version = MessageVersion.Soap12;
            var message = Message.CreateMessage(version, "action", body);
            message.WriteMessage(xmlWriter);

            xmlWriter.Flush();
            stream.Position = 0;

            var el = XElement.Parse(new StreamReader(stream).ReadToEnd());
            Console.WriteLine("Message");
            Console.WriteLine("  Status: {0}", message.State);
            Console.WriteLine("  Message:{0}\n", el.ToString());
        }
    }
}