using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace bibliotecaclient2
{
    public sealed class BibliotecaClient : IDisposable
    {
        private readonly string host;
        private readonly int port;
        private TcpClient client;
        private StreamReader reader;
        private StreamWriter writer;
        private readonly SemaphoreSlim communicationLock = new SemaphoreSlim(1, 1);

        public BibliotecaClient(string host, int port)
        {
            this.host = host;
            this.port = port;
        }

        public bool IsConnected
        {
            get { return client != null && client.Connected; }
        }

        public async Task ConnectAsync()
        {
            if (IsConnected)
                return;

            client = new TcpClient();
            await client.ConnectAsync(host, port);

            NetworkStream stream = client.GetStream();
            reader = new StreamReader(stream, Encoding.UTF8, false, 4096, true);
            writer = new StreamWriter(stream, new UTF8Encoding(false), 4096, true)
            {
                AutoFlush = true
            };

            await reader.ReadLineAsync();
            await reader.ReadLineAsync();
        }

        public async Task<string> SendAsync(string request)
        {
            if (!IsConnected)
                throw new InvalidOperationException("Il client non è connesso al server.");

            await communicationLock.WaitAsync();
            try
            {
                await writer.WriteLineAsync(request);
                string response = await reader.ReadLineAsync();

                if (response == null)
                    throw new IOException("Il server ha chiuso la connessione.");

                return response;
            }
            finally
            {
                communicationLock.Release();
            }
        }

        public void Disconnect()
        {
            writer?.Dispose();
            reader?.Dispose();
            client?.Close();
            writer = null;
            reader = null;
            client = null;
        }

        public void Dispose()
        {
            Disconnect();
            communicationLock.Dispose();
        }
    }
}
