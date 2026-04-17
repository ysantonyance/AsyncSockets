using System.Net;
using System.Net.Sockets;
using System.Text;

class Client
{
    private const int DEFAULT_BUFLEN = 512;
    private const int DEFAULT_PORT = 27015;
    private static bool isRunning = true;

    static async Task Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.Title = "CLIENT SIDE";
        Console.WriteLine("=== CLIENT STARTED ===");

        try
        {
            var ipAddress = IPAddress.Loopback;
            var remoteEndPoint = new IPEndPoint(ipAddress, DEFAULT_PORT);

            var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await clientSocket.ConnectAsync(remoteEndPoint);
            Console.WriteLine("Connected to server!");

            var receivingTask = Task.Run(async () =>
            {
                var buffer = new byte[DEFAULT_BUFLEN];
                while (isRunning)
                {
                    int bytesReceived = await clientSocket.ReceiveAsync(buffer);
                    if (bytesReceived > 0)
                    {
                        var response = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
                        Console.WriteLine($"\nServer response: {response}");
                    }
                    else
                    {
                        Console.WriteLine("Server disconnected.");
                        break;
                    }
                }
            });

            while (isRunning)
            {
                Console.Write("\nCommands: \n1.time\n2.date\n3.weather [city]\n4.euro\n5.bitcoin\n6.stop\n");
                var message = Console.ReadLine();
                if (message?.ToLower() == "stop")
                {
                    await clientSocket.SendAsync(Encoding.UTF8.GetBytes(message));
                    isRunning = false;
                    break;
                }

                var messageBytes = Encoding.UTF8.GetBytes(message!);
                await clientSocket.SendAsync(messageBytes);
                Console.WriteLine($"Sent: {message}");
            }

            await Task.Delay(500); 
            clientSocket.Close();
            Console.WriteLine("Client shutting down.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}