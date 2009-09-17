using System;
using System;
using System.IO;
using System.Runtime.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WcfExamples
{
	[TestClass]
	public class TranscenderExample
	{
		[TestMethod]
		public void ExampleTest()
		{
			var emp = new Employee { Name = "Jennifer Jones", HireID = 101, SessionID = 5 };
			var serData = DataContractSerializer<Employee>.Serialize(emp);
			const string expected = @"<CompanyEmployee xmlns=""http://intranet.company.com"" xmlns:i=""http://www.w3.org/2001/XMLSchema-instance""><Address/><FullName>Jennifer Jones</FullName><ID>101</ID></CompanyEmployee>";
			Assert.AreEqual(expected,serData);
		}

		#region Nested type: DataMemberIsRequired
		[DataContract(Namespace = "http://intranet.company.com")]
		public struct Address
		{
			public string Street;
			public string City;
			public string State;
			public int Zip;

		}

		[DataContract(Name = "CompanyEmployee", Namespace = "http://intranet.company.com")]
		public class Employee
		{
			[DataMember(Name = "FullName")]
			private string _name;
			public string Name
			{
				get { return _name; }
				set { _name = value; }
			}

			public int SessionID { get; set; }

			[DataMember(Name = "ID", Order = 0)]
			public int HireID { get; set; }

			[DataMember(IsRequired = false)]
			public Address Address { get; set; }
		}
		#endregion
	}
}