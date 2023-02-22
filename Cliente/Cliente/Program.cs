using System;
using System.Net.WebSockets;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        string serverAddress = "localhost";
        int serverPort = 8080;
        var webSocket = new ClientWebSocket();

        // Verifica se o servidor está ativo.
        try
        {
            // cria o objeto TcpClient e tenta se conectar ao servidor
            using (var tcpClient = new TcpClient())
            {
                tcpClient.Connect(serverAddress, serverPort);

                // se a conexão for bem sucedida, o servidor está ativo
                Console.WriteLine("O servidor está ativo.");
                await webSocket.ConnectAsync(new Uri("ws://localhost:8080"), CancellationToken.None);

                while (webSocket.State == WebSocketState.Open)
                {
                    Console.WriteLine("Envie uma mensagem para o Servidor: ");
                    var message = Console.ReadLine();
                    var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
                    await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);

                    //Recebe responsa do servidor.
                    var bufferServidor = new ArraySegment<byte>(new byte[4096]);
                    var result = await webSocket.ReceiveAsync(bufferServidor, CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                    }
                    else if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var messageServidor = System.Text.Encoding.UTF8.GetString(bufferServidor.Array, bufferServidor.Offset, result.Count);
                        Console.WriteLine($"Menssagem recebida do servidor: {messageServidor}");
                    }
                    Console.WriteLine();
                }
            }
        }
        catch (Exception ex)
        {
            // se ocorrer uma exceção, o servidor não está ativo
            Console.WriteLine("Não foi possível conectar ao servidor. Erro: " + ex.Message);
        }
        finally
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
        }
    }
}
