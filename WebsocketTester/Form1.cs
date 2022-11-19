using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SimpleWebsocket;


namespace WebsocketTester
{    

    public partial class Form1 : Form
    {
        WebsocketListener listener = new WebsocketListener( 1224 );
        WebsoketClient server;
        WebsoketClient client;

        public Form1()
        {            
            InitializeComponent();

            listener.ClientConnected += Listener_ClientConnected;

            ServerAddress.Text = "ws://localhost:1224";
            ClientAddress.Text = "ws://localhost:1224";
        }
        
        private void Listener_ClientConnected( object sender, WebsocketConnectedEventArgs e )
        {
            server = e.Client;
            server.MessageRecieved += Server_MessageRecieved;

            Invoke( new MethodInvoker( EnableServerField ) );
            
        }

        private void Server_MessageRecieved( object sender, WebsocketMessageRecievedEventArgs e )
        {
            ServerLog.Invoke( (MethodInvoker) delegate { ServerLog.Text = e.Message + "\r\n"; }, e.Message );
          
        }

        void EnableServerField()
        { ServerField.Enabled = true; }

        private void ServerField_KeyDown( object sender, KeyEventArgs e )
        {
            if ( e.KeyCode == Keys.Enter )
            {
                server.WriteText( ServerField.Text );
                ServerField.Text = "";
            }

            
        }

        private void button1_Click( object sender, EventArgs e )
        {
            client = new WebsoketClient( ClientAddress.Text );
            ClientField.Enabled = true;

            client.MessageRecieved += Client_MessageRecieved;
        }

        private void Client_MessageRecieved( object sender, WebsocketMessageRecievedEventArgs e )
        {
            ServerLog.Invoke( (MethodInvoker) delegate { ClientLog.Text = e.Message + "\r\n"; }, e.Message );
        }

        private void ClientField_KeyDown( object sender, KeyEventArgs e )
        {
            if ( e.KeyCode == Keys.Enter )
            {
                client.WriteText( ClientField.Text );
                ClientField.Text = "";
            }

            
        }
    }
}
