using System;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;
using System.Xml;

class Script
{

    [DataContract(Name = "MyHeader", Namespace = "http://schemas.mynamespace.org")]
    class MyHeaderData
    {
        [DataMember]
        public string Content;
    }

    [STAThread]
    static public void Main(string[] args)
    {
        var headerData = new MyHeaderData() { Content = "data" };
        var ns = "http://schemas.mynamespace.org";
        var ver = MessageVersion.Soap12;
        var msg = Message.CreateMessage(ver, "action", "body");
        var header = MessageHeader.CreateHeader(
            "MyHeader",
            ns,
            headerData);
        msg.Headers.Add(header);

        var result = msg.Headers.GetHeader<MyHeaderData>("MyHeader", ns);
        Console.WriteLine(result.Content);
    }
}

