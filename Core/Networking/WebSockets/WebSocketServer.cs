using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Net.Http;
using System.Net.WebSockets;

namespace Sys.Networking.WebSockets
{
    public class WebSocketServer
    {
        private CancellationTokenSource cts;
        private HttpListener listener;
        private Uri prefix;

        public string Name { get; set; }

        public int BufferSize { get; set; } = 1024 * 16;
        public string SubProtocol { get; set; } = "chat";

        public TextWriter cout { get; set; } = Console.Out;
        public TextWriter cerr { get; set; } = Console.Error;

        public WebSocketServer(Uri uri)
        {
            this.prefix = uri;
            this.cts = new CancellationTokenSource();

            this.listener = new HttpListener();
            listener.Prefixes.Add(uri.AbsoluteUri);
            listener.AuthenticationSchemes = AuthenticationSchemes.Basic;
        }

        public async Task Start()
        {
            try
            {
                listener.Start();
                cout.WriteLine($"listening on {prefix} ...");
            }
            catch (Exception ex)
            {
                cerr.WriteLine($"cannot start server {prefix}, {ex.Message}");
                return;
            }

            while (true)
            {
                HttpListenerContext listenerContext = await listener.GetContextAsync();
                if (listenerContext.Request.IsWebSocketRequest)
                {
                    await Accept(listenerContext);
                }
                else
                {
                    listenerContext.Response.StatusCode = 400;
                    listenerContext.Response.Close();
                }
            }
        }

        public void Stop()
        {
            if (this.listener != null)
                this.listener.Stop();
        }

        private async Task Accept(HttpListenerContext context)
        {
            var credential = GetClientCredential(context);

            HttpListenerWebSocketContext ctx = null;

            try
            {
                ctx = await context.AcceptWebSocketAsync(SubProtocol, BufferSize, TimeSpan.FromSeconds(10000));
                IPAddress ipAddress = context.Request.RemoteEndPoint.Address;
            }
            catch (Exception e)
            {
                context.Response.StatusCode = 500;
                context.Response.Close();
                cerr.WriteLine($"Exception: {e.Message}");
                return;
            }

            WebSocket ws = ctx.WebSocket;

            try
            {
                await ReceiveAsync(ws, credential);
            }
            catch (Exception e)
            {
                cerr.WriteLine($"{e.Message}");
            }
            finally
            {
                if (ws != null)
                    ws.Dispose();
            }
        }



        private async Task ReceiveAsync(WebSocket ws, NetworkCredential credential)
        {
            byte[] buffer = new byte[BufferSize];
            while (ws.State == WebSocketState.Open)
            {
                var segment = new ArraySegment<byte>(buffer);
                WebSocketReceiveResult receiveResult = await ws.ReceiveAsync(segment, CancellationToken.None);

                int count = receiveResult.Count;
                while (!receiveResult.EndOfMessage)
                {
                    if (count >= buffer.Length)
                    {
                        string err = $"receiving message size is greater than buffer size = {BufferSize}";
                        cerr.WriteLine(err);
                        await ws.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, err, cts.Token);
                        return;
                    }

                    segment = new ArraySegment<byte>(buffer, count, buffer.Length - count);
                    receiveResult = await ws.ReceiveAsync(segment, CancellationToken.None);
                    count += receiveResult.Count;
                }

                switch (receiveResult.MessageType)
                {
                    case WebSocketMessageType.Text:
                        WebSocketMessage message = new WebSocketMessage(buffer, count) { Credential = credential };
                        Enque(ws, message);
                        break;

                    case WebSocketMessageType.Binary:
                        {
                            string err = "binary message not supported";
                            cerr.WriteLine(err);
                        }
                        break;

                    case WebSocketMessageType.Close:
                        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                        break;
                }
            }
        }

        private BlockingCollection<WebSocketMessage> queue = new BlockingCollection<WebSocketMessage>();

        private void Enque(WebSocket ws, WebSocketMessage message)
        {
            if (!queue.TryAdd(message, 1000, cts.Token))
            {
                cerr.WriteLine($"fails to enque {message}");
            }
            else
            {
                cout.WriteLine($"recv: {message}");
            }

        }

        public async Task SendAsync(WebSocket ws, WebSocketMessage message)
        {
            var sendBuffer = new ArraySegment<byte>(message.Bytes);

            if (ws.State == WebSocketState.Open)
                await ws.SendAsync(sendBuffer, WebSocketMessageType.Text, endOfMessage: true, cancellationToken: cts.Token);
            else
                cout.WriteLine($"fails to send {message}, WebSockets State={ws.State}");
        }



        private NetworkCredential GetClientCredential(HttpListenerContext context)
        {
            System.Security.Principal.IPrincipal user = context.User;
            System.Security.Principal.IIdentity id = null;

            if (user != null)
                id = user.Identity;

            if (id == null)
            {
                cerr.WriteLine("client authentication is not enabled for this Web server.");
                return new NetworkCredential();
            }

            if (id.IsAuthenticated)
            {
                cout.WriteLine($"{id.Name} was authenticated using {id.AuthenticationType}");
                return new NetworkCredential(id.Name, string.Empty);
            }
            else
            {
                cerr.WriteLine($"{id.Name} was not authenticated");
            }

            return new NetworkCredential();
        }
    }
}
