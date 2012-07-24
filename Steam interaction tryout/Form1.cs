using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SteamKit2;

namespace Steam_interaction_tryout
{
    public partial class Form1 : Form
    {
        static SteamClient sclient = new SteamClient(System.Net.Sockets.ProtocolType.Tcp);
        SteamFriends sFriends = sclient.GetHandler<SteamFriends>();
        SteamUser sUser = sclient.GetHandler<SteamUser>();

        public Form1()
        {
            InitializeComponent();
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            sclient.Connect();
            
        }

        void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            sclient.Disconnect();
        }

        private void button1_Click(object sender, EventArgs e)
        {
        }
    }
}
