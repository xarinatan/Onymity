using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;

namespace NetworkbotClienttest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            textBox1.KeyDown += new KeyEventHandler(textBox1_KeyDown);
        }

        void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            { richTextBox1.AppendText( ">" + textBox1.Text + Environment.NewLine + "Bot: " + transceive(textBox1.Text) + Environment.NewLine); richTextBox1.ScrollToCaret(); textBox1.Text = ""; }
        }

        string transceive(string sendstring)
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            s.Connect("nicolas.ced", 5000);
            CedLib.Networking.socketfunctions.sendstring(s, "interactsteam\n" + textBox_steamID.Text + "\n" + textBox_steamname.Text + "\n" + textBox_chattype.Text  + "\n" + sendstring);
            if (CedLib.Networking.socketfunctions.waitfordata(s, 30000, false))
            {
                return CedLib.Networking.socketfunctions.receivestring(s, false);

            }
            else
                return null;
        }
    }
}
