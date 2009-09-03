using System;
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

	[DataContract]
	class DataRecord
	{
		[DataMember]
		public DateTime OperatingDate { get; set; }

		[DataMember]
		public decimal RecordValue { get; set; }
	} 

    [TestClass]
    public class CollectionDataContractExample
    {
        [TestMethod]
        public void StringTest()
        {
            var c = new MyCollection<string> {"item 1", "item 2", "item 3"};

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

		[TestMethod]
		public void DataRecordTest()
		{
			var c = new MyCollection<DataRecord>
			        {
			        	new DataRecord {OperatingDate = DateTime.Today, RecordValue = 1},
			        	new DataRecord {OperatingDate = DateTime.Today, RecordValue = 2},
			        	new DataRecord {OperatingDate = DateTime.Today, RecordValue = 3}
			        };

			var serializer = new DataContractSerializer(c.GetType());
			var stream = new MemoryStream();
			serializer.WriteObject(stream, c);

			stream.Position = 0;

			var reader = new StreamReader(stream);
			var doc = XDocument.Parse(reader.ReadToEnd());
			const string exp =
@"<MyCollectionOfDataRecord xmlns=""http://schemas.datacontract.org/2004/07/WcfExamples.CollectionDataContract"" xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">
  <DataRecord>
    <OperatingDate>2009-09-03T00:00:00-04:00</OperatingDate>
    <RecordValue>1</RecordValue>
  </DataRecord>
  <DataRecord>
    <OperatingDate>2009-09-03T00:00:00-04:00</OperatingDate>
    <RecordValue>2</RecordValue>
  </DataRecord>
  <DataRecord>
    <OperatingDate>2009-09-03T00:00:00-04:00</OperatingDate>
    <RecordValue>3</RecordValue>
  </DataRecord>
</MyCollectionOfDataRecord>";

			Assert.AreEqual(exp, doc.ToString());
		}
    }
}
