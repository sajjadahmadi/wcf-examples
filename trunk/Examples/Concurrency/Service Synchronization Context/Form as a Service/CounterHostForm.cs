﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ServiceModel;
using System.Threading;
using System.Diagnostics;
using System.Text.RegularExpressions;
using CodeRunner.ServiceModel.Hosting;

namespace CodeRunner
{
    [ServiceContract]
    interface ICounter
    {
        [OperationContract]
        void Increment();
        [OperationContract]
        void Decrement();
        [OperationContract]
        int GetCount();
    }

    public partial class CounterForm : FormHost<CounterForm>, ICounter
    {
        [ThreadStatic]
        internal static CounterForm Current;

        public CounterForm(params string[] baseAddresses)
            : base(baseAddresses)
        {
            InitializeComponent();

            Current = this;

            Host.Opening += new EventHandler(host_StateChanged);
            Host.Opened += new EventHandler(host_StateChanged);
            Host.Closing += new EventHandler(host_StateChanged);
            Host.Closed += new EventHandler(host_StateChanged);
            Host.Faulted += new EventHandler(host_StateChanged);
        }

        void host_StateChanged(object sender, EventArgs e)
        {
            ServiceHost host = sender as ServiceHost;
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

        #region ICounter Members

        public void Increment()
        {
            Counter++;
        }

        public void Decrement()
        {
            Counter--;
        }

        public int GetCount()
        {
            return Counter;
        }

        #endregion
    }
}
