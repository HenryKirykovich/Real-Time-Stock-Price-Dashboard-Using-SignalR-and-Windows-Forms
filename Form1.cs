using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Windows.Forms;

namespace ClientSideChat
{
    /// <summary>
    /// Simple WinForms dashboard that connects to the SignalR `StockPriceHub` and
    /// displays incoming `ReceiveStockPrice` messages in a ListBox.
    /// Students: the client creates a `HubConnection`, subscribes to events with
    /// `connection.On<T>(...)`, then starts the connection with `StartAsync()`.
    /// </summary>
    public partial class Form1 : Form
    {
        // HubConnection is the client-side object that maintains the connection
        // to the server-side SignalR hub.
        private HubConnection connection;

        public Form1()
        {
            InitializeComponent();
            InitializeSignalR();
        }

        // Simple file-based logger to help students inspect events when debugging.
        private void Log(string message)
        {
            try
            {
                var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "client_log.txt");
                System.IO.File.AppendAllText(path, DateTime.Now.ToString("o") + " - " + message + Environment.NewLine);
            }
            catch { }
        }

        /// <summary>
        /// Initialize and start the SignalR connection. The client subscribes to
        /// the "ReceiveStockPrice" event and invokes the server method
        /// "StartStockPriceUpdates" to request simulated updates.
        /// </summary>
        private async void InitializeSignalR()
        {
            // Development endpoints (server binds to these by default).
            var httpsUrl = "https://localhost:7148/stockpricehub";
            var httpUrl = "http://localhost:5209/stockpricehub";

            // Factory that accepts untrusted dev certificates (for local dev only).
            System.Func<HttpMessageHandler> handlerFactory = () =>
            {
                var handler = new System.Net.Http.HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                return handler;
            };

            // Helper to create a connection for a given url and optionally use the handler.
            HubConnection CreateConnection(string url, bool acceptAnyCert)
            {
                var builder = new HubConnectionBuilder();
                if (acceptAnyCert)
                {
                    builder = builder.WithUrl(url, options =>
                    {
                        options.HttpMessageHandlerFactory = _ => handlerFactory();
                    });
                }
                else
                {
                    builder = builder.WithUrl(url);
                }

                return builder.WithAutomaticReconnect().Build();
            }

            // Shared receiver registration
            void RegisterReceiver(HubConnection conn)
            {
                conn.On<int>("ReceiveStockPrice", price =>
                {
                    Invoke((Action)(() =>
                    {
                        string messageText = $"Stock Price: {price}";
                        listBox1.Items.Add(messageText);
                        if (listBox1.Items.Count > 0)
                            listBox1.TopIndex = listBox1.Items.Count - 1;
                    }));
                });
            }

            Exception httpsException = null;

            // Try HTTPS first (accept dev certs). If it fails, fallback to HTTP.
            try
            {
                connection = CreateConnection(httpsUrl, acceptAnyCert: true);
                RegisterReceiver(connection);
                Log($"Starting HTTPS connection to {httpsUrl}...");
                await connection.StartAsync();
                Log("Connection started (HTTPS).");
                await connection.InvokeAsync("StartStockPriceUpdates");
                Log("Invoked StartStockPriceUpdates (HTTPS).");
                return;
            }
            catch (Exception ex)
            {
                httpsException = ex;
                Log("HTTPS connection failed: " + ex.ToString());
            }

            // Fallback to HTTP
            try
            {
                connection = CreateConnection(httpUrl, acceptAnyCert: false);
                RegisterReceiver(connection);
                Log($"Starting HTTP connection to {httpUrl}...");
                await connection.StartAsync();
                Log("Connection started (HTTP).");
                await connection.InvokeAsync("StartStockPriceUpdates");
                Log("Invoked StartStockPriceUpdates (HTTP).");
                return;
            }
            catch (Exception exHttp)
            {
                Log("HTTP connection failed: " + exHttp.ToString());
                // Show full details to help debugging
                var msg = $"Cannot connect to SignalR hub.\n\nHTTPS error:\n{httpsException}\n\nHTTP error:\n{exHttp}";
                MessageBox.Show(msg, "SignalR connection error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
