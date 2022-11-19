using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleWebsocket
{
    public class WebsocketMessageRecievedEventArgs : EventArgs
    {
        string _message;
        byte[] _data;
        public string Message { get => _message; }        
        public byte[] Data { get => _data; }


        public WebsocketMessageRecievedEventArgs( string message )
        {
            _message = message;
            _data = UTF8Encoding.UTF8.GetBytes( _message );
        }

        public WebsocketMessageRecievedEventArgs( byte[] data )
        {
            _data = data;
            _message = UTF8Encoding.UTF8.GetString( data );
        }
    }

    public class WebsocketConnectedEventArgs : EventArgs
    {
        WebsoketClient _client;
        public WebsoketClient Client { get => _client; }

        public WebsocketConnectedEventArgs( WebsoketClient client )
        {
            _client = client;
        }

    }
}