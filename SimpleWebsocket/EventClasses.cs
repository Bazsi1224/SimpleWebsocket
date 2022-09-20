using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleWebsocket
{
    public class WebsocketMessageRecievedEventArgs : EventArgs
    {
        string _message;
        public string Message { get => _message; }

        public WebsocketMessageRecievedEventArgs( string message )
        {
            _message = message;
        }
    }
}
