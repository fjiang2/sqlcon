using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Sys.Networking.WebSockets
{
    public class WebSocketMessage
    {
        private static readonly Encoding encoding = Encoding.UTF8;
        private byte[] bytes;

        public NetworkCredential Credential { get; set; }

        public WebSocketMessage(byte[] buffer, int count)
        {
            bytes = new byte[count];
            Buffer.BlockCopy(buffer, 0, bytes, 0, count);
        }

        public WebSocketMessage(byte[] buffer)
        {
            bytes = buffer;
        }

        public WebSocketMessage(string text)
        {
            bytes = encoding.GetBytes(text);
        }

        public string Text => encoding.GetString(bytes);

        public byte[] Bytes => bytes;

        public override string ToString()
        {
            return Text;
        }
    }

}
