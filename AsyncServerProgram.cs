using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Concurrent;

class Server
{
    private const int DEFAULT_BUFLEN = 512;
    private const int DEFAULT_PORT = 27015;
    private static ConcurrentQueue<(Socket client, byte[] data, int length)> messageQueue = new();
    private static bool isRunning = true;

    static async Task Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.Title = "SERVER SIDE";
        Console.WriteLine("=== SERVER STARTED ===");

        try
        {
            var ipAddress = IPAddress.Any;
            var localEndPoint = new IPEndPoint(ipAddress, DEFAULT_PORT);

            var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(localEndPoint);
            listener.Listen(10);
            Console.WriteLine("Waiting for client connection...");

            var clientSocket = await listener.AcceptAsync();
            Console.WriteLine("Client connected!");
            listener.Close();

            _ = ProcessMessages();

            var buffer = new byte[DEFAULT_BUFLEN];

            while (isRunning)
            {
                int bytesReceived = await clientSocket.ReceiveAsync(buffer);
                if (bytesReceived > 0)
                {
                    var messageBytes = new byte[bytesReceived];
                    Buffer.BlockCopy(buffer, 0, messageBytes, 0, bytesReceived);
                    messageQueue.Enqueue((clientSocket, messageBytes, bytesReceived));

                    var message = Encoding.UTF8.GetString(messageBytes);
                    Console.WriteLine($"\nClient: {message}");

                    if (message.Trim().ToLower() == "stop")
                    {
                        Console.WriteLine("Stop command received. Shutting down...");
                        break;
                    }
                }
                else
                {
                    Console.WriteLine("Client disconnected.");
                    break;
                }
            }

            clientSocket.Close();
            isRunning = false;
            Console.WriteLine("Server shutting down.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    static async Task ProcessMessages()
    {
        while (isRunning)
        {
            if (messageQueue.TryDequeue(out var item))
            {
                var (clientSocket, data, length) = item;
                var message = Encoding.UTF8.GetString(data, 0, length);

                string response = ProcessCommand(message);
                byte[] responseBytes = Encoding.UTF8.GetBytes(response);

                await clientSocket.SendAsync(responseBytes);
                Console.WriteLine($"Server: {response}");
            }

            await Task.Delay(100);
        }
    }

    static string ProcessCommand(string command)
    {
        command = command.Trim().ToLower();

        if (command.StartsWith("weather"))
        {
            var parts = command.Split(' ');
            if (parts.Length > 1)
            {
                string city = parts[1].ToLower();
                return GetWeatherByCity(city);
            }
            else
            {
                return "Please specify a city. Example: weather Odesa";
            }
        }

        switch (command)
        {
            case "time":
                return DateTime.Now.ToString("HH:mm:ss");
            case "date":
                return DateTime.Now.ToString("dd.MM.yyyy");
            case "euro":
                return "EUR/USD: 1.09, EUR/UAH: 44.50";
            case "bitcoin":
                return "BTC: 45,200 USD";
            case "stop":
                return "Server is shutting down...";
            default:
                return $"Unknown command: '{command}'. Available: time, date, weather [city], euro, bitcoin, stop";
        }
    }

    static string GetWeatherByCity(string city)
    {
        switch (city)
        {
            case "odesa":
                return "Weather in Odesa: +14°C, sunny, humidity 65%";
            case "kyiv":
                return "Weather in Kyiv: +12°C, partly cloudy, humidity 70%";
            case "lviv":
                return "Weather in Lviv: +10°C, rainy, humidity 85%";
            case "kharkiv":
                return "Weather in Kharkiv: +11°C, cloudy, humidity 75%";
            case "dnipro":
                return "Weather in Dnipro: +13°C, sunny, humidity 60%";
            case "london":
                return "Weather in London: +15°C, cloudy, humidity 80%";
            case "new york":
            case "ny":
                return "Weather in New York: +18°C, sunny, humidity 55%";
            case "tokyo":
                return "Weather in Tokyo: +20°C, clear sky, humidity 50%";
            case "paris":
                return "Weather in Paris: +16°C, partly cloudy, humidity 68%";
            case "berlin":
                return "Weather in Berlin: +13°C, light rain, humidity 78%";
            default:
                return "WE don't have that city in our data";
                
        }
    }
}
