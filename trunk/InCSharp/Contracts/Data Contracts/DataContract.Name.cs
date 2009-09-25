using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WcfExamples
{
	[DataContract(Name = "Machine Part")]
	public class OriginalMachinePart
	{
		[DataMember]
		public int ID;
		[DataMember]
		public string Name = "Original Machine Part";
	}

	[DataContract(Name = "Machine Part")]
	public class ComplexMachinePart
	{
		[DataMember]
		public int ID;

		[DataMember]
		public string Name = "Complex Machine Part";

		[DataMember]
		public string SerialNumber = Guid.NewGuid().ToString();
	}

	[ServiceContract(Name = "IMyService")]
	public interface IOriginalService
	{
		[OperationContract]
		OriginalMachinePart GetMachinePart(int id);
	}

	[ServiceContract(Name = "IMyService")]
	public interface IComplexService
	{
		[OperationContract]
		ComplexMachinePart GetMachinePart(int id);
	}

	public class MyService : IOriginalService, IComplexService
	{
		OriginalMachinePart IOriginalService.GetMachinePart(int id)
		{
			return new OriginalMachinePart { ID = id };
		}

		ComplexMachinePart IComplexService.GetMachinePart(int id)
		{
			return new ComplexMachinePart { ID = id };
		}
	}

	[TestClass]
	public class DataContractNameTests
	{
		[TestMethod]
		public void OriginalMachinePartTest()
		{
			const string address = "net.pipe://localhost";
			using (var host = new ServiceHost(typeof(MyService), new Uri(address)))
			{
				var binding = new NetNamedPipeBinding();
				host.AddServiceEndpoint(typeof(IOriginalService), binding, "");
				host.Open();

				var proxy = ChannelFactory<IOriginalService>.CreateChannel(binding, new EndpointAddress(address));

				var part = proxy.GetMachinePart(5);

				Assert.AreEqual(5, part.ID);
				Assert.AreEqual("Original Machine Part", part.Name);

				((ICommunicationObject)proxy).Close();

				host.Close();
			}
		}

		[TestMethod]
		public void ComplexMachinePartTest()
		{
			const string address = "net.pipe://localhost";
			using (var host = new ServiceHost(typeof(MyService), new Uri(address)))
			{
				var binding = new NetNamedPipeBinding();
				host.AddServiceEndpoint(typeof(IComplexService), binding, "");
				host.Open();

				var proxy = ChannelFactory<IComplexService>.CreateChannel(binding, new EndpointAddress(address));

				var part = proxy.GetMachinePart(5);

				Assert.AreEqual(5, part.ID);
				Assert.AreEqual("Complex Machine Part", part.Name);

				((ICommunicationObject)proxy).Close();

				host.Close();
			}
		}

		[TestMethod]
		public void ComplexServiceOriginalClientTest()
		{
			const string address = "net.pipe://localhost";
			using (var host = new ServiceHost(typeof(MyService), new Uri(address)))
			{
				var binding = new NetNamedPipeBinding();
				host.AddServiceEndpoint(typeof(IComplexService), binding, "");
				host.Open();

				var proxy = ChannelFactory<IOriginalService>.CreateChannel(binding, new EndpointAddress(address));

				var part = proxy.GetMachinePart(5);

				Assert.AreEqual(5, part.ID);
				Assert.AreEqual("Complex Machine Part", part.Name);

				((ICommunicationObject)proxy).Close();

				host.Close();
			}
		}
	}
}