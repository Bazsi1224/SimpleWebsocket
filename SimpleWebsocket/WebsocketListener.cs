using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Net;
using System.Net.Sockets;
using System.Net.Http;

namespace SimpleWebsocket
{
    public class WebsocketListener
    {
        const int BUFFER_SIZE = 2048;

        TcpListener listener;
        byte[] buffer = new byte[BUFFER_SIZE];

        public event EventHandler<WebsocketConnectedEventArgs> ClientConnected;

        public WebsocketListener( int port )
        {
            listener = new TcpListener( IPAddress.Any, port );
            listener.Start();

            listener.BeginAcceptTcpClient( new AsyncCallback( TCPClientConnected ), null );

            listener.Start();
        }


        void TCPClientConnected( IAsyncResult ar )
        {
            TcpClient tcpClient = listener.EndAcceptTcpClient( ar );
            listener.BeginAcceptTcpClient( new AsyncCallback( TCPClientConnected ), null );

            tcpClient.GetStream().BeginRead( buffer, 0, BUFFER_SIZE, new AsyncCallback( RequestGot ), tcpClient );
        }

        void RequestGot( IAsyncResult ar )
        {
            TcpClient client = (TcpClient) ar.AsyncState;
            client.GetStream().EndRead( ar );
            string request = Encoding.ASCII.GetString( buffer );

            request = request.ToUpper();

            string[] requestParts = request.Split( ' ' );

            if ( requestParts[0] == "GET" &&
                 request.ToLower().Contains( "connection: upgrade" ) )
            {
                sendWSHandshake( requestParts[1], client );
            }
            else
                client.Close();
        }


        void sendWSHandshake( string request, TcpClient client )
        {
            string[] requestLines = request.Split( '\n' );

            string key = "";

            foreach ( string line in requestLines )
            {
                string[] fields = line.Split( ':' );

                if ( fields.Length == 2 &&
                     fields[0] == "Sec-WebSocket-Key" )
                    key = fields[1].Trim();
            }

            key = getKeyResponse( key );


            if ( string.IsNullOrEmpty( key ) )
                return;

            string response = "HTTP/1.1 101 Switching Protocols\r\n";
            response += "Connection: Upgrade\r\n";
            response += "Upgrade: websocket\r\n";
            response += "Sec-WebSocket-Accept: " + key + " \r\n";
            response += "Sec-WebSocket-Protocol: v1.1.main_update\r\n";
            response += "Sec-WebSocket-Version: 13\r\n";
            response += "\r\n";


            byte[] responseBuffer = Encoding.UTF8.GetBytes( response );
            client.GetStream().Write( responseBuffer, 0, responseBuffer.Length );

            if ( ClientConnected != null )
                ClientConnected.Invoke( this, new WebsocketConnectedEventArgs( new WebsoketClient( client ) ) );
        }

        string getKeyResponse( string key )
        {
            key += "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

            byte[] keyBuffer = Encoding.ASCII.GetBytes( key );

            byte[] keyHash = SHA1.Create().ComputeHash( keyBuffer );
            key = Convert.ToBase64String( keyHash );

            return key;
        }

    }
}
