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
            // Use HTTPS address that the server binds to in development.
            connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7148/stockpricehub")
                .WithAutomaticReconnect()
                .Build();

            // Subscribe to updates from the hub. The generic parameter is the
            // type of data sent by the server (int in our demo).
            connection.On<int>("ReceiveStockPrice", price =>
            {
                Invoke((Action)(() =>
                {
                    string messageText = $"Stock Price: {price}";
                    listBox1.Items.Add(messageText);
                    if (listBox1.Items.Count > 0)
                        listBox1.TopIndex = listBox1.Items.Count - 1;
                }));
            });

            try
            {
                Log("Starting connection...");
                await connection.StartAsync();
                Log("Connection started.");
                // Ask the server to start sending price updates.
                await connection.InvokeAsync("StartStockPriceUpdates");
                Log("Invoked StartStockPriceUpdates.");
            }
            catch (Exception ex)
            {
                Log("SignalR error: " + ex.ToString());
                MessageBox.Show($"SignalR error: {ex.Message}");
            }
        }
    }
}
