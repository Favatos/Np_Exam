using ChatServer;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using TcpLib;

namespace Server;

internal class Server_Main
{
    private char[,] board = new char[25, 25];
    private bool isXTurn = true; 
    private bool gameEnded = false;
    static async Task Main(string[] args) => await new Server_Main().RunAsync();
    private async Task RunAsync()
    {
        for (int i = 0; i < 25; i++)
        {
            for (int j = 0; j < 25; j++)
            {
                board[i, j] = '0';
            }
        }

        using TcpListener listener = new(IPAddress.Any, 2025);
        listener.Start();
        Console.WriteLine("Сервер запущен и слушает на порту 2025");
        int counter = 0;
        while (true)
        {
            counter++;
            using TcpClient client = await listener.AcceptTcpClientAsync();
            //await client.SendStringAsync(counter == 1 ? "X" : "0");
            Player player = new(client, counter == 1 ? 'X' : 'O');
            _ = ListenToClient(player);
            if (counter == 2) counter = 0;
        }
    }

    private async Task ListenToClient(Player player)
    {
        player.Name = await player.TcpClient.ReceiveStringAsync();
        await player.TcpClient.SendStringAsync(player.Symbol.ToString());
        Console.WriteLine($" Member {player} has connected");

        while (true)
        {
            Console.WriteLine('1');
            int row = await player.TcpClient.ReceiveInt32Async();
            Console.WriteLine(row);
            int col = await player.TcpClient.ReceiveInt32Async();
            
            if (board[row, col] == '0' && (isXTurn ? 'X' : 'O') == player.Symbol)
            {
                board[row, col] = player.Symbol;
            }
            string str_board = JsonSerializer.Serialize(board);
            Console.WriteLine(str_board);
            await player.TcpClient.SendJsonAsync(str_board);
            isXTurn = !isXTurn;
        }
    }
}
