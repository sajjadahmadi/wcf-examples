using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Channels;

namespace WcfExamples.MessageContracts
{
    class DatabaseOperationMessage : Message
    {
        // TODO: See Transcender 1.4.2

        public string Username { get; set; }
        public string Password { internal get; set; }
        public string ConnectionString { get; set; }
        public string SQLStatement { get; set; }

        private MessageProperties _properties;
        private MessageHeaders _headers;
        private MessageVersion _version;

        public override MessageHeaders Headers
        {
            get { return _headers; }
        }

        protected override void OnWriteBodyContents(System.Xml.XmlDictionaryWriter writer)
        {
            writer.WriteElementString("SQLStatement", SQLStatement);
        }

        public override MessageProperties Properties
        {
            get { return _properties; }
        }

        public override MessageVersion Version
        {
            get { return _version; }
        }

        // Other implementation omitted 
    }
}
