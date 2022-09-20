using System;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace SimpleWebsocket
{
    class WebsoketClient
    {
        const int BUFFER_SIZE = 2048;
        const int WS_HEADER_SIZE = 14;

        const int OP_CONTINUE = 0x00;
        const int OP_TEXT = 0x01;
        const int OP_BINARY = 0x02;
        const int OP_CLOSE = 0x08;
        const int OP_PING = 0x09;
        const int OP_PONG = 0x0A;

        string _remoteAddress;
        TcpClient _wsclient;
        Stream _wsstream;
        bool _wsHandshakeDone = false;


        byte[] buffer = new byte[BUFFER_SIZE];


        public event EventHandler<WebsocketMessageRecievedEventArgs> MessageRecieved;
        public event EventHandler<EventArgs> ClientDisconnected;

        public string ClientAddress { get => _remoteAddress; }

        public WebsoketClient( TcpClient client )
        {
            _remoteAddress = client.Client.RemoteEndPoint.ToString();
            _remoteAddress = _remoteAddress.Remove( _remoteAddress.IndexOf( ':' ) );

            setWebsocket( client );
        }

        public void setWebsocket( TcpClient wsclient )
        {
            _wsclient = wsclient;
            _wsstream = wsclient.GetStream();

            _wsHandshakeDone = false;
            _wsstream.BeginRead( buffer, 0, BUFFER_SIZE, WSRequestGot, null );

        }




        void WSRequestGot( IAsyncResult ar )
        {
            if ( !_wsclient.Connected ||
                 !ar.IsCompleted )
            {
                if ( ClientDisconnected != null )
                {
                    ClientDisconnected.Invoke( this, new EventArgs() );
                }
                return;
            }

            try
            {
                _wsstream.EndRead( ar );

            }
            catch ( Exception exp )
            {
                return;
            }

            if ( !_wsHandshakeDone )
            {
                sendWSHandshake( Encoding.ASCII.GetString( buffer ) );
            }
            else
                ReadRequest();

            if ( _wsclient.Connected )
                _wsstream.BeginRead( buffer, 0, BUFFER_SIZE, WSRequestGot, null );


        }

        void sendWSHandshake( string request )
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


            string response = "HTTP/1.1 101 Switching Protocols\r\n" +
                  "Upgrade: websocket\r\n" +
                  "Connection: Upgrade\r\n" +
                  "Sec-WebSocket-Accept: " + key + "\r\n\r\n";


            byte[] responseBuffer = Encoding.UTF8.GetBytes( response );
            _wsstream.Write( responseBuffer, 0, responseBuffer.Length );
            _wsHandshakeDone = true;
        }

        string getKeyResponse( string key )
        {
            key += "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

            byte[] keyBuffer = Encoding.ASCII.GetBytes( key );

            byte[] keyHash = SHA1.Create().ComputeHash( keyBuffer );
            key = Convert.ToBase64String( keyHash );

            return key;
        }

        void ReadRequest()
        {
            bool fin = ( buffer[0] & 0x80 ) != 0;
            int opcode = buffer[0] & 0x0f;

            bool masked = ( buffer[1] & 0x80 ) != 0;
            byte lengthCode = Convert.ToByte( buffer[1] & 0x7f );
            ulong length = 0;
            int mask = 0;
            int pointer = 2;

            if ( lengthCode < 126 )
                length = lengthCode;
            else
            if ( lengthCode == 126 )
            {
                length = BitConverter.ToUInt16( buffer, 2 );
                pointer += 2;
            }
            else
            {
                length = BitConverter.ToUInt64( buffer, 2 );
                pointer += 8;
            }

            if ( masked )
            {
                mask = BitConverter.ToInt32( buffer, pointer );
                pointer += 4;
            }

            byte[] payload = new byte[length];

            Buffer.BlockCopy( buffer, pointer, payload, 0, Convert.ToInt32( length ) );

            if ( masked )
                payload = UnmaskPayload( payload, mask );


            switch ( opcode )
            {
                case OP_TEXT:
                    WebsocketMessageRecievedEventArgs args = new WebsocketMessageRecievedEventArgs( Encoding.UTF8.GetString( payload ) );
                    if ( MessageRecieved != null )
                        MessageRecieved.Invoke( this, args );

                    break;

                case OP_PING:
                    sendPongOverWebsocket();
                    break;

                case OP_CLOSE:
                    _wsclient.Close();
                    _wsHandshakeDone = false;
                    break;
            }



        }

        byte[] UnmaskPayload( byte[] payload, int mask )
        {
            byte[] maskArray = BitConverter.GetBytes( mask );

            for ( int i = 0; i < payload.Length; i++ )
                payload[i] = Convert.ToByte( payload[i] ^ maskArray[i % 4] );

            return payload;
        }

        void sendPongOverWebsocket()
        {
            byte[] responseBuffer = new byte[WS_HEADER_SIZE];

            BinaryWriter bw = new BinaryWriter( new MemoryStream( responseBuffer ) );

            bw.Write( Convert.ToByte( 0x80 | OP_PONG ) ); // text frame 

            bw.Write( Convert.ToByte( WS_HEADER_SIZE - 2 ) );
            bw.Write( new byte[12] ); // rest header is empty

            _wsstream.Write( responseBuffer, 0, responseBuffer.Length );
        }

        public void WriteText( string message )
        {
            byte[] messageArray = Encoding.UTF8.GetBytes( message );
            ;
            byte[] responseBuffer = new byte[messageArray.Length + WS_HEADER_SIZE];

            BinaryWriter bw = new BinaryWriter( new MemoryStream( responseBuffer ) );

            bw.Write( Convert.ToByte( 0x80 | OP_TEXT ) ); // text frame 

            if ( messageArray.Length < 126 )
            {
                bw.Write( Convert.ToByte( messageArray.Length ) );
            }
            else
            if ( messageArray.Length < 0x10000 )
            {
                bw.Write( Convert.ToByte( 126 ) );
                byte[] bytes = BitConverter.GetBytes( Convert.ToUInt16( messageArray.Length ) );
                Array.Reverse( bytes );
                bw.Write( bytes );
            }
            else
            {
                bw.Write( Convert.ToByte( 127 ) );
                byte[] bytes = BitConverter.GetBytes( Convert.ToUInt64( messageArray.Length ) );
                Array.Reverse( bytes );
                bw.Write( bytes );
            }

            bw.Write( messageArray );

            if ( _wsclient.Connected )
                _wsstream.Write( responseBuffer, 0, responseBuffer.Length );
        }
    }
}
