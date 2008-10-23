using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using System.Xml.Linq;
using System.Diagnostics;

namespace System.ServiceModel.Extensions
{
    public class Helper
    {
        public static string Serialize<T>(T dataContract)
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
        public static T Deserialize<T>(string xmlData)
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
