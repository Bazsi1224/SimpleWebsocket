using System;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Threading;
using System.Net.Http;


namespace SimpleWebsocket
{


    public class WebsoketClient
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
        int _timeout = 0;
        bool _pingSent = false;

        Timer pingPongTimeout;

        byte[] buffer = new byte[BUFFER_SIZE];


        public event EventHandler<WebsocketMessageRecievedEventArgs> MessageRecieved;
        public event EventHandler<EventArgs> ClientDisconnected;

        public string ClientAddress { get => _remoteAddress; }
        public bool Connected { get => _wsclient.Connected; }
        public int Timeout { get => _timeout; set { _timeout = value; pingPongTimeout = new Timer( new TimerCallback( pingPongTimeoutTick ), null, 0, _timeout ); } }

        public WebsoketClient( TcpClient client )
        {
            setWebsocket( client );
        }
        public WebsoketClient( string uri )
        {
            Uri address = new Uri( uri );            

            TcpClient client = new TcpClient( address.Host, address.Port );

            string request = "GET / HTTP/1.1\r\n";
            request += "Connection: upgrade\r\n";
            request += "Upgrade: websocket\r\n";
            request += "\r\n";


            byte[] requestBuffer = UTF8Encoding.UTF8.GetBytes( request );

            client.GetStream().Write( requestBuffer, 0 , requestBuffer.Length );

            const int BUFFER_SIZE = 2048;
            byte[] responseBuffer = new byte[BUFFER_SIZE];
            client.GetStream().Read( responseBuffer, 0, BUFFER_SIZE );

            string response = UTF8Encoding.UTF8.GetString( responseBuffer );
            if ( response.Contains( "101" ) )
                setWebsocket( client );
        }

        void setWebsocket( TcpClient wsclient )
        {
            //pingPongTimeout = new Timer( new TimerCallback( pingPongTimeoutTick ), null, 0, _timeout );

            _remoteAddress = wsclient.Client.RemoteEndPoint.ToString();
            _remoteAddress = _remoteAddress.Remove( _remoteAddress.IndexOf( ':' ) );


            _wsclient = wsclient;
            _wsstream = wsclient.GetStream();

            _wsstream.BeginRead( buffer, 0, BUFFER_SIZE, WSRequestGot, null );

        }


        void pingPongTimeoutTick( object target )
        {
            if ( _pingSent )
            {
                _wsclient.Close();
                return;
            }
                
            sendPingOverWebsocket();
            _pingSent = true;
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

            ReadRequest();

            if ( _wsclient.Connected )
                _wsstream.BeginRead( buffer, 0, BUFFER_SIZE, WSRequestGot, null );


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

                case OP_BINARY:
                    WebsocketMessageRecievedEventArgs bargs = new WebsocketMessageRecievedEventArgs( payload );
                    if ( MessageRecieved != null )
                        MessageRecieved.Invoke( this, bargs );

                    break;

                case OP_PING:
                    sendPongOverWebsocket();
                    break;

                case OP_PONG:
                    _pingSent = false;
                    break;

                case OP_CLOSE:
                    _wsclient.Close();
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

            bw.Write( Convert.ToByte( 0x80 | OP_PONG ) ); // pong frame 

            bw.Write( Convert.ToByte( WS_HEADER_SIZE - 2 ) );
            bw.Write( new byte[12] ); // rest header is empty

            _wsstream.Write( responseBuffer, 0, responseBuffer.Length );
        }

        void sendPingOverWebsocket()
        {
            byte[] responseBuffer = new byte[WS_HEADER_SIZE];

            BinaryWriter bw = new BinaryWriter( new MemoryStream( responseBuffer ) );

            bw.Write( Convert.ToByte( 0x80 | OP_PING ) ); // ping frame 

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

        public void WriteByteArray( byte[] messageArray )
        {
            
            byte[] responseBuffer = new byte[messageArray.Length + WS_HEADER_SIZE];

            BinaryWriter bw = new BinaryWriter( new MemoryStream( responseBuffer ) );

            bw.Write( Convert.ToByte( 0x80 | OP_BINARY ) ); // text frame 

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
