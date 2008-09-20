using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ServiceModel;

namespace Microsoft.ServiceModel.Samples
{
    public partial class Form1 : Form, IChat
    {
        const string defaultName = "Default Name";
        string member = defaultName;
        InstanceContext instanceContext;
        IChatChannel participant;
        IOnlineStatus ostat;
        DuplexChannelFactory<IChatChannel> factory;
        
        public Form1()
        {
            InitializeComponent();
            displayNameTextBox.Text = defaultName;
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            // Handle messages on callback interface
            instanceContext = new InstanceContext(this);

            // Create the participant
            // Each participant opens a duplex channel to the mesh
            // participant is an instance of the chat application that has openeed a channel to the mesh
            factory = new DuplexChannelFactory<IChatChannel>(instanceContext, "ChatEndpoint");
            participant = factory.CreateChannel();

            // Retrieve the PeerNode associated with the participant and register for on/offline events
            ostat = participant.GetProperty<IOnlineStatus>();
            ostat.Online += new EventHandler(OnOnline);
            ostat.Offline += new EventHandler(OnOffline);

            try
            {
                participant.Open();
            }
            catch (CommunicationException ex)
            {
                MessageBox.Show("Could not find resolver.  If you are using a custom resolver, please ensure that the service is running before executing this sample.  Refer to the readme for more details.");
                return;
            }


            if (string.IsNullOrEmpty(displayNameTextBox.Text))
            {
                member = defaultName;
            }
            else
            {
                member = displayNameTextBox.Text;
            }
            receiveBox.AppendText(member + " is ready" + Environment.NewLine);
            participant.Join(member);
            receiveBox.AppendText("Begin chatting..." + Environment.NewLine);
        }

        void OnOffline(object sender, EventArgs e)
        {
            receiveBox.AppendText("** Offline");
        }

        void OnOnline(object sender, EventArgs e)
        {
            receiveBox.AppendText("** Online");
        }


        #region IChat Members

        void IChat.Join(string member)
        {
            receiveBox.AppendText(string.Format("[{0} joined]", member));
        }

        void IChat.Chat(string member, string message)
        {
            receiveBox.AppendText(string.Format("[{0}] {1}", member, message));
        }

        void IChat.Leave(string member)
        {
            receiveBox.AppendText(string.Format("[{0} left]", member));
        }

        #endregion
    }
}
