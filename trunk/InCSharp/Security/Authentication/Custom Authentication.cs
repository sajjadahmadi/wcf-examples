using System;
using System.IdentityModel.Selectors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IdentityModel.Tokens;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Security;

namespace CodeRunner.Security
{
	public class CustomValidator : UserNamePasswordValidator
	{
		public override void Validate(string userName, string password)
		{
			if (null == userName || null == password)
				throw new ArgumentNullException();

			// Validate userName and password here
			var valid = (userName == "valid" && password == "valid");

			if (!valid)
				throw new SecurityTokenException("Unknown Username or Password");
		}
	}

	[ServiceContract]
	internal interface IMyContract
	{
		[OperationContract]
		void MyMethod();
	}

	internal class MyService : IMyContract
	{
		public void MyMethod()
		{
			return;
		}
	}

	[TestClass]
	public class CustomAuthentication
	{
		static readonly string Address = "http://localhost:8888/" + Guid.NewGuid().ToString();
		static ServiceHost _host;

		[ClassInitialize]
		public static void CreateHost(TestContext testContext)
		{
			var binding = new WSHttpBinding();
			binding.Security.Mode = SecurityMode.Message;
			binding.Security.Message.ClientCredentialType = MessageCredentialType.UserName;
			binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;

			_host = new ServiceHost(typeof(MyService));
			_host.AddServiceEndpoint(typeof(IMyContract), binding, Address);
			_host.Credentials.UserNameAuthentication.UserNamePasswordValidationMode = UserNamePasswordValidationMode.Custom;
			_host.Credentials.UserNameAuthentication.CustomUserNamePasswordValidator = new CustomValidator();

			_host.Open();
		}

		[ClassCleanup]
		public static void CloseHost()
		{
			if (_host != null)
				_host.Close();
		}

		[TestMethod]
		public void CredentialsValid()
		{
			using (var factory = new ChannelFactory<IMyContract>(new WSHttpBinding(), Address))
			{
				factory.Credentials.UserName.UserName = "valid";
				factory.Credentials.UserName.Password = "valid";
				var proxy = factory.CreateChannel();
				proxy.MyMethod();
			}

		}

		[TestMethod]
		[ExpectedException(typeof(SecurityTokenException))]
		public void CredentialsInvalid()
		{
			using (var factory = new ChannelFactory<IMyContract>(new WSHttpBinding(), Address))
			{
				factory.Credentials.UserName.UserName = "invalid";
				factory.Credentials.UserName.Password = "invalid";
				var proxy = factory.CreateChannel();
				proxy.MyMethod();
			}

		}
	}
}