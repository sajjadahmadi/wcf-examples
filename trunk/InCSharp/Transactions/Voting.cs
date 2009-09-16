using CodeRunner.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.ServiceModel.Examples
{
    [TestClass]
    public class Voting
    {
        public static Transactional<string> StringResource;

        #region Additional test attributes
        static NetNamedPipeBinding binding;
        static ServiceHost host;
        static string address = "net.pipe://localhost/" + Guid.NewGuid().ToString();

        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            binding = new NetNamedPipeBinding();
			host = new ServiceHost(typeof(MyService));
            host.AddServiceEndpoint(typeof(IChangeResource), binding, address);
            host.Open();
        }

        // Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup()]
        public static void MyClassCleanup()
        {
            host.Close();
        }

        //// Use TestInitialize to run code before running each test 
        [TestInitialize()]
        public void MyTestInitialize()
        {
            StringResource = new Transactional<string>("Original Value");
        }

        //// Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup() { }

        #endregion

        [ServiceContract(SessionMode = SessionMode.Required)]
        interface IChangeResource
        {
            [OperationContract]
            void ChangeWithAutoComplete();
            [OperationContract]
            void ChangeWithExplicitComplete();

            [OperationContract]
            [FaultContract(typeof(string))]
            void ChangeThenFault();
        }

		[ServiceBehavior(IncludeExceptionDetailInFaults=true)]
        class MyService : IChangeResource
        {
            // When TransactionAutoComplete = true, WCF will automatically vote 
            // to commit the transaction if there were no unhandled exceptions
            [OperationBehavior(
                TransactionScopeRequired = true,
                TransactionAutoComplete = true)]
            public void ChangeWithAutoComplete()
            {
                // Some work
                Voting.StringResource.Value = "New Value";
                // If no exceptions, WCF will automatically vote to commit
            }

            // When TransactionAutoComplete = false, WCF will vote to abort
            // the transaction unless SetTransactionComplete is called.
            [OperationBehavior(
                TransactionScopeRequired = true,
                TransactionAutoComplete = false)]
            public void ChangeWithExplicitComplete()
            {
                // Some work
                Voting.StringResource.Value = "New Value";
                OperationContext.Current.SetTransactionComplete(); // Vote
                // SetTransactionComplete should always be called last
            }

            // When an unhandled exception occures, WCF will vote to abort
            [OperationBehavior(
                TransactionScopeRequired = true,
                TransactionAutoComplete = true)]
            public void ChangeThenFault()
            {
                // Some work
                Voting.StringResource.Value = "New Value";
                throw new FaultException<string>("An error occured and the transaction was aborted.");
            }

        }

        [TestMethod]
        public void DeclarativeVotingTest()
        {
            Assert.AreEqual("Original Value", StringResource.Value);
            var channel = ChannelFactory<IChangeResource>.CreateChannel(binding, new EndpointAddress(address));
            channel.ChangeWithAutoComplete();
            Assert.AreEqual("New Value", StringResource.Value);
            ((ICommunicationObject)channel).Close();
        }

        [TestMethod]
        public void ExplicitVotingTest()
        {
            Assert.AreEqual("Original Value", StringResource.Value);
			var channel = ChannelFactory<IChangeResource>.CreateChannel(binding, new EndpointAddress(address));
			channel.ChangeWithExplicitComplete();
            Assert.AreEqual("New Value", StringResource.Value);
			((ICommunicationObject)channel).Close();
		}

        [TestMethod]
        public void AbortAfterException()
        {
            Assert.AreEqual("Original Value", StringResource.Value);
			var channel = ChannelFactory<IChangeResource>.CreateChannel(binding, new EndpointAddress(address));

            try
            {
                channel.ChangeThenFault();
            }
            catch (FaultException<string> ex)
            {
                Assert.AreEqual("An error occured and the transaction was aborted.",
                    ex.Detail);
            }

            Assert.AreEqual("Original Value", StringResource.Value, "Expected rollback.");
			((ICommunicationObject)channel).Close();
		}
    }
}
