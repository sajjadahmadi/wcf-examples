using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

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
        public DemarcatingOperations()
        { }

        [TestMethod]
        public void TestMethod1()
        {
            IOrderManager manager = InProcFactory.CreateChannel<OrderManager, IOrderManager>();
            manager.SetCustomerId(123);
            manager.AddItem(4);
            manager.SetCustomerId(123);
            manager.AddItem(5);
            manager.AddItem(6);
            Assert.AreEqual(3, manager.GetItemCount());
            manager.ProcessItems();
            try
            {
                manager.GetItemCount();
                Assert.Fail("This call shouldn't have been allowed.");
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                Assert.IsInstanceOfType(ex, typeof(InvalidOperationException));
            }

            ((ICommunicationObject)manager).Close();
        }
    }
}
