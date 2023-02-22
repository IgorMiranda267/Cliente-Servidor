using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:8080/");
        listener.Start();

        while (true)
        {
            var context = await listener.GetContextAsync();
            if (IsWebSocketRequest(context))
            {
                try
                {
                    var webSocketContext = await context.AcceptWebSocketAsync(null);
                    var webSocket = webSocketContext.WebSocket;
                    await Program.HandleWebSocketConnectionAsync(webSocket);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"WebSocket error: {ex.Message}");
                }
            }

        }
    }

    static bool IsWebSocketRequest(HttpListenerContext context)
    {
        return context.Request.Headers["Upgrade"] == "websocket"
            && context.Request.Headers["Connection"]?.Contains("Upgrade") == true;
    }

    static async Task HandleWebSocketConnectionAsync(WebSocket webSocket)
    {
        try
        {
            while (webSocket.State == WebSocketState.Open)
            {
                var buffer = new ArraySegment<byte>(new byte[4096]);
                var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = System.Text.Encoding.UTF8.GetString(buffer.Array, buffer.Offset, result.Count);

                    var responseMessage = "Entregue!";
                    // converte a mensagem de resposta em um array de bytes
                    var responseBytes = Encoding.UTF8.GetBytes(responseMessage);

                    // envia a mensagem de resposta de volta para o cliente
                    await webSocket.SendAsync(responseBytes, WebSocketMessageType.Text, true, CancellationToken.None);
                    Console.WriteLine($"Menssagem recebida do Cliente: {message}");

                    if (message == "close WebSocket")
                        break;
                }
            }
        }
        catch (WebSocketException ex)
        {
            Console.WriteLine($"WebSocket error: {ex.Message}");
        }
        finally
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
        }
    }
}
