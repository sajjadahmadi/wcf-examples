using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Windows.Forms;

namespace CodeRunner.ServiceModel.Hosting
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public abstract class FormHost<F> : Form
        where F : Form
    {
        ServiceHost<F> host;

        protected ServiceHost<F> Host
        {
            get { return host; }
            set { host = value; }
        }

        public FormHost(params string[] baseAddresses)
        {
            host = new ServiceHost<F>(this as F, baseAddresses);

            Load += delegate
                {
                    if (Host.State == CommunicationState.Created)
                    { Host.Open(); }
                };
            FormClosed += delegate
            {
                if (Host.State == CommunicationState.Opened)
                {
                    Host.Close();
                }
            };
        }

    }
}
