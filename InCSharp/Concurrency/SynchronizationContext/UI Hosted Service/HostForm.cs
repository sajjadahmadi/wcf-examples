﻿using System;
using System.ServiceModel;
using System.Windows.Forms;

namespace CodeRunner
{
    public partial class HostForm : Form
    {
        [ThreadStatic]
        internal static HostForm Current;

        ServiceHost<CounterService> host;

        public HostForm(string baseAddress)
        {
            InitializeComponent();

            this.Text = baseAddress;

            Current = this;

            host = new ServiceHost<CounterService>(new Uri(baseAddress));
            host.Opening += new EventHandler(host_StateChanged);
            host.Opened += new EventHandler(host_StateChanged);
            host.Closing += new EventHandler(host_StateChanged);
            host.Closed += new EventHandler(host_StateChanged);
            host.Faulted += new EventHandler(host_StateChanged);
            host.BeginOpen(null, null);
        }

        void host_StateChanged(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "Host State: " + host.State.ToString();
        }

        public int Counter
        {
            get
            {
                int count;
                if (int.TryParse(countTextBox.Text, out count)) return count;
                else return 0;
            }
            set { countTextBox.Text = value.ToString(); }
        }

        private void HostForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            host.Close();
        }
    }
}
