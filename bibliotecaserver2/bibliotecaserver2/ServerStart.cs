using System.Net;
using System.Net.Sockets;
using System.Text;

namespace bibliotecaserver2;

public sealed class ServerStart
{
    public const int Port = 5000;

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        var listener = new TcpListener(IPAddress.Any, Port);
        listener.Start();

        Console.WriteLine($"Server biblioteca in ascolto sulla porta {Port}.");

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                TcpClient client = await listener.AcceptTcpClientAsync(cancellationToken);
                _ = GestisciClientAsync(client);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Server arrestato.");
        }
        finally
        {
            listener.Stop();
        }
    }

    private static async Task GestisciClientAsync(TcpClient client)
    {
        using (client)
        using (NetworkStream stream = client.GetStream())
        {
            byte[] messaggio = Encoding.UTF8.GetBytes(
                "Connessione al server biblioteca riuscita.\r\n");

            await stream.WriteAsync(messaggio);
        }
    }
}
