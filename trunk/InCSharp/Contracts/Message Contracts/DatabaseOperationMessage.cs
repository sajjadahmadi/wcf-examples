using System.ServiceModel.Channels;
using System.Xml;

namespace WcfExamples.MessageContracts
{
    internal class DatabaseOperationMessage : Message
    {
        // TODO: See Transcender 1.4.2

        private MessageHeaders _headers;
        private MessageProperties _properties;
        private MessageVersion _version;
        public string Username { get; set; }
        public string Password { internal get; set; }
        public string ConnectionString { get; set; }
        public string SQLStatement { get; set; }

        public override MessageHeaders Headers
        {
            get { return _headers; }
        }

        public override MessageProperties Properties
        {
            get { return _properties; }
        }

        public override MessageVersion Version
        {
            get { return _version; }
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            writer.WriteElementString("SQLStatement", SQLStatement);
        }

        // Other implementation omitted 
    }
}