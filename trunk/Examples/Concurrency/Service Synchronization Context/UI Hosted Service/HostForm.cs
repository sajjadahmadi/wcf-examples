using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ServiceModel;

namespace CodeRunner
{
    public partial class HostForm : Form
    {
        [ThreadStatic]
        internal static HostForm Current;

        ServiceHost<CounterService> host;

        public HostForm()
        {
            InitializeComponent();

            Current = this;

            host = new ServiceHost<CounterService>();
            host.Opened += new EventHandler(host_StateChanged);
            host.Closed += new EventHandler(host_StateChanged);
            host.Faulted += new EventHandler(host_StateChanged);
            host.Open();
        }

        void host_StateChanged(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "Host State: " + host.State.ToString();
        }

        public int Counter
        {
            get { return Convert.ToInt32(countTextBox.Text); }
            set { countTextBox.Text = value.ToString(); }
        }

        private void HostForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            host.Close();
        }
    }
}
