using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ServiceModel;
using CodeRunner.Client;
using System.Diagnostics;
using System.Threading;

namespace CodeRunner
{
    public partial class CounterForm : Form
    {
        SynchronizationContext m_SynchronizationContext;
        CounterClient proxy;

        public CounterForm()
        {
            InitializeComponent();

            m_SynchronizationContext = SynchronizationContext.Current;

            Thread.CurrentThread.Name = "Form Thread";  

            string address = "net.pipe://localhost/" + Guid.NewGuid().ToString();
            ServiceHost<Service.Counter> host;
            host = new ServiceHost<CodeRunner.Service.Counter>();
            this.FormClosed += delegate { proxy.Close(); host.Close(); };
            host.AddServiceEndpoint<Service.ICounter>(new NetNamedPipeBinding(), address);
            host.Open();

            proxy = new CounterClient(new NetNamedPipeBinding(), address);
        }

        void OnComplete(IAsyncResult result)
        {
            Debug.Assert(Thread.CurrentThread.Name != "Form Thread");
            Debug.Assert(result.IsCompleted == true);

            int count = proxy.EndIncrement(result);  // This will not block
            result.AsyncWaitHandle.Close();          // Clean up
            SendOrPostCallback callback = delegate
                        {
                            Debug.Assert(Thread.CurrentThread.Name == "Form Thread");
                            countLabel.Text = count.ToString();
                        };
            m_SynchronizationContext.Send(callback, null);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            proxy.BeginIncrement(Counter, OnComplete, proxy);
        }

        public int Counter
        {
            get
            {
                int count;
                if (int.TryParse(countLabel.Text, out count)) return count;
                else return 0;
            }
            set { countLabel.Text = value.ToString(); }
        }
    }
}
