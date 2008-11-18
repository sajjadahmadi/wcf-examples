using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.ServiceModel.Examples
{
    [ServiceContract(SessionMode = SessionMode.Required)]
    public interface IOrderManager
    {
        [OperationContract]
        void SetCustomerId(int customerId);

        [OperationContract(IsInitiating = false)]
        void AddItem(int itemId);

        [OperationContract(IsInitiating = false)]
        decimal GetItemCount();

        [OperationContract(IsInitiating = false, IsTerminating = true)]
        bool ProcessItems();
    }
    public class OrderManager : IOrderManager
    {
        int _customerId = default(int);
        List<int> items = new List<int>();

        public void SetCustomerId(int customerId)
        {
            _customerId = customerId;
        }

        public void AddItem(int itemId)
        {
            items.Add(itemId);
        }

        public decimal GetItemCount()
        {
            return items.Count();
        }

        public bool ProcessItems()
        {
            items.ForEach(i =>
            {
                Trace.WriteLine(i);
            });
            items.Clear();
            return true;
        }
    }

    [TestClass]
    public class DemarcatingOperations
    {
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), 
            "The operation 'AddItem' cannot be the first operation to be called because IsInitiating is false.")]
        public void IsInitiating_False()
        {
            IOrderManager manager = InProcFactory.CreateChannel<OrderManager, IOrderManager>();
            manager.AddItem(4); // Invalid
            ((ICommunicationObject)manager).Close();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException),
            "This channel cannot send any more messages because IsTerminating operation 'ProcessItems' has already been called.")]
        public void IsTerminating_True()
        {
            IOrderManager manager = InProcFactory.CreateChannel<OrderManager, IOrderManager>();
            manager.SetCustomerId(123);
            manager.ProcessItems();
            manager.AddItem(4); // Invalid
            ((ICommunicationObject)manager).Close();
        }

        [TestMethod]
        public void NormalDemarcation()
        {
            IOrderManager manager = InProcFactory.CreateChannel<OrderManager, IOrderManager>();
            manager.SetCustomerId(123);
            manager.AddItem(4);
            manager.GetItemCount();
            manager.ProcessItems();
            ((ICommunicationObject)manager).Close();
        }
    }
}
