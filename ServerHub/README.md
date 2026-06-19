# Real-Time Stock Price Dashboard (SignalR + WinForms)

Repository: https://github.com/HenryKirykovich/Real-Time-Stock-Price-Dashboard-Using-SignalR-and-Windows-Forms.git

This solution contains:

- ServerHub/ — ASP.NET Core SignalR server (project: ServerHub.csproj).
- ClientSideChat/ — WinForms client that connects to the StockPriceHub and displays prices.
- TempSignalRClient/ — small console client for quick verification.

Goals:

- Start the server so it listens on https://localhost:7148 (development HTTPS).
- Run the WinForms client and verify it receives ReceiveStockPrice updates.

Prerequisites:

- Windows with .NET SDK / runtimes installed.
- Recommended: install .NET 8 runtime and SDK if you want the server to target net8.0.
  Download from: https://dotnet.microsoft.com/en-us/download/dotnet/8.0
  After installing, run: dotnet --list-runtimes
- Trust the ASP.NET Core dev certificate for HTTPS (once):
  dotnet dev-certs https --trust

Run the server (from repository root):

dotnet run --project ServerHub/ServerHub/ServerHub.csproj

The server is configured to bind to both https://localhost:7148 and http://localhost:5209.

Run the WinForms client (Release):

dotnet build ClientSideChat/ClientSideChat.csproj -c Release
Start-Process .\ClientSideChat\bin\Release\ClientSideChat.exe

Or run the Debug executable (already built during development):

Start-Process .\ClientSideChat\bin\Debug\ClientSideChat.exe

Client logs are written to: ClientSideChat\bin\Debug\client_log.txt (or Release folder).

Quick verification (negotiate):

Invoke-RestMethod -Uri https://localhost:7148/stockpricehub/negotiate -Method Post | ConvertTo-Json

If the HTTPS certificate is not trusted, use HTTP:

Invoke-RestMethod -Uri http://localhost:5209/stockpricehub/negotiate -Method Post | ConvertTo-Json

Use the temporary console client:

dotnet run --project TempSignalRClient/TempSignalRClient.csproj

Notes for instructors / students:

- The server hub is implemented in ServerHub/ServerHub/StockPriceHub.cs and
  exposes StartStockPriceUpdates(). It broadcasts ReceiveStockPrice messages every second.
- The WinForms client is ClientSideChat/Form1.cs and uses HubConnection to subscribe to ReceiveStockPrice
  and invoke StartStockPriceUpdates.

If you want me to produce screenshots of the running server console and the WinForms client window,
I can provide commands/instructions to capture them.
