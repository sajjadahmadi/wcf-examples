﻿using System.IO;
using System.Xml;

namespace System.Runtime.Serialization
{
    public class DataContractSerializer<T> : XmlObjectSerializer
    {
        DataContractSerializer _serializer;

        public DataContractSerializer()
        {
            _serializer = new DataContractSerializer(typeof(T));
        }

        public override bool IsStartObject(XmlDictionaryReader reader)
        {
            return _serializer.IsStartObject(reader);
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

        public static string Serialize(T dataContract)
        {
            DataContractSerializer<T> formatter = new DataContractSerializer<T>();
            using (Stream stream = new MemoryStream())
            {
                formatter.WriteObject(stream, dataContract);
                stream.Position = 0;
                StreamReader reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
        }

        public static T Deserialize(string xmlData)
        {
            T obj;
            DataContractSerializer<T> formatter = new DataContractSerializer<T>();
            using (Stream stream = new MemoryStream(Text.Encoding.Default.GetBytes(xmlData)))
            {
                obj = formatter.ReadObject(stream);
            }
            return obj;
        }
    }
}
