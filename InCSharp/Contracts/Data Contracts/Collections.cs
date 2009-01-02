using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WcfExamples.CollectionDataContract
{
    [CollectionDataContract(Name = "MyCollectionOf{0}")]
    class MyCollection<T>
        : List<T>
    { }

    [TestClass]
    public class CollectionDataContractExample
    {
        [TestMethod]
        public void TestMethod1()
        {
            var c = new MyCollection<string>();
            c.Add("item 1");
            c.Add("item 2");
            c.Add("item 3");

            var serializer = new DataContractSerializer(c.GetType());
            var stream = new MemoryStream();
            serializer.WriteObject(stream, c);

            stream.Position = 0;

            var reader = new StreamReader(stream);
            var doc = XDocument.Parse(reader.ReadToEnd());
            const string exp =
@"<MyCollectionOfstring xmlns=""http://schemas.datacontract.org/2004/07/WcfExamples.CollectionDataContract"" xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">
  <string>item 1</string>
  <string>item 2</string>
  <string>item 3</string>
</MyCollectionOfstring>";

            Assert.AreEqual(exp, doc.ToString());
        }
    }
}
