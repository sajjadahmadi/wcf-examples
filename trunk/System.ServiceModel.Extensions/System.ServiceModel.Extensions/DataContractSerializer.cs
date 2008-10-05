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
        public new T ReadObject(Stream stream)
        {
            return (T)_serializer.ReadObject(stream);
        }
        public new T ReadOjbect(XmlReader reader)
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
        public override bool IsStartObject(System.Xml.XmlDictionaryReader reader)
        {
            throw new NotImplementedException();
        }
        public override object ReadObject(System.Xml.XmlDictionaryReader reader, bool verifyObjectName)
        {
            throw new NotImplementedException();
        }
        public override void WriteEndObject(System.Xml.XmlDictionaryWriter writer)
        {
            throw new NotImplementedException();
        }
        public override void WriteObjectContent(System.Xml.XmlDictionaryWriter writer, object graph)
        {
            throw new NotImplementedException();
        }
        public override void WriteStartObject(System.Xml.XmlDictionaryWriter writer, object graph)
        {
            throw new NotImplementedException();
        }
    }
}
