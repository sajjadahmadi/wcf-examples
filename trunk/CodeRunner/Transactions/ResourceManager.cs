using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace CodeRunner.Transactions
{
    public static class ResourceManager
    {
        public static void ConstrainType(Type type)
        {
            if (type.IsSerializable == false)
            { throw new InvalidOperationException("The type " + type + " is not serializable"); }
        }
        public static T Clone<T>(T source)
        {
            if (Object.ReferenceEquals(source, null))
            { return default(T); }

            Debug.Assert(typeof(T).IsSerializable);
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Position = 0;
                T clone = (T)formatter.Deserialize(stream);
                return clone;
            }
        }

    }
}
