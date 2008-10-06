using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

namespace System.Runtime.Serialization
{
    class DataContractSerializer<T> : XmlObjectSerializer
    {
        DataContractSerializer _serializer;

        public DataContractSerializer()
        {
            _serializer = new DataContractSerializer(typeof(T));
        }

        public override bool IsStartObject(XmlDictionaryReader reader)
        {
          return  _serializer.IsStartObject(reader);
        }

        public override object ReadObject(XmlDictionaryReader reader, bool verifyObjectName)
        {
            return _serializer.ReadObject(reader, verifyObjectName);
        }
        public new T ReadObject(Stream stream)
        {
            return (T)_serializer.ReadObject(stream);
        }
        public new T ReadObject(XmlReader reader)
        {
            return (T)_serializer.ReadObject(reader);
        }

        public void WriteObject(Stream stream, T graph)
        {
            _serializer.WriteObject(stream, graph);
        }
        public void WriteObject(XmlWriter writer, T graph)
        {
            _serializer.WriteObject(writer, graph);
        }
        public override void WriteEndObject(XmlDictionaryWriter writer)
        {
            _serializer.WriteEndObject(writer);
        }
        public override void WriteObjectContent(XmlDictionaryWriter writer, object graph)
        {
            _serializer.WriteObjectContent(writer, graph);
        }
        public override void WriteStartObject(XmlDictionaryWriter writer, object graph)
        {
            _serializer.WriteStartObject(writer, graph);
        }
    }
}
