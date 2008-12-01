using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.ServiceModel;

namespace CodeRunner
{
    [CallbackBehavior(UseSynchronizationContext = false)]
    public partial class ClientForm : Form, ICounterCallback
    {
        SynchronizationContext m_context;
        CounterClient m_proxy;

        public ClientForm()
        {
            InitializeComponent();
            m_context = SynchronizationContext.Current;
            InstanceContext callbackContext = new InstanceContext(this);
            m_proxy = new CounterClient(callbackContext);
        }

        void ICounterCallback.CountChanged(int value)
        {
            SendOrPostCallback setCount = delegate
            {
                Count = value;
            };
            m_context.Post(setCount, null);
        }

        public int Count
        {
            get
            {
                int count;
                if (int.TryParse(countTextBox.Text, out count)) return count;
                else return 0;
            }
            set
            {
                countTextBox.Text = value.ToString();
            }
        }

        private void IncButton_Click(object sender, EventArgs e)
        {
            m_proxy.Increment();
        }

        private void DecButton_Click(object sender, EventArgs e)
        {
            m_proxy.Decrement();
        }

        private void ClientForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_proxy.Close();
        }
    }
}
