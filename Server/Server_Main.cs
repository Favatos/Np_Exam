using ChatServer;
using System.Net;
using System.Net.Sockets;
using TcpLib;

namespace Server;

internal class Server_Main
{
    private char[,] board = new char[25, 25];
    private bool isXTurn = true; 
    private bool gameEnded = false;
    private List<Player> players = new List<Player>();
    static async Task Main(string[] args) => await new Server_Main().RunAsync();
    private async Task RunAsync()
    {
        PrepareBoard();

        using TcpListener listener = new(IPAddress.Any, 2025);
        listener.Start();
        Console.WriteLine("Сервер запущен и слушает на порту 2025");
        int counter = 0;
        while (counter!=3)
        {
            counter++;
            TcpClient client = await listener.AcceptTcpClientAsync();
            Player player = new(client, counter == 1 ? 'X' : 'O');

            _ = ListenToClient(player);

            lock (players)
                players.Add(player);

            //if (counter == 2) counter = 0;
        }
    }

    private async Task ListenToClient(Player player)
    {
        player.Name = await player.TcpClient.ReceiveStringAsync();
        await player.TcpClient.SendStringAsync(player.Symbol.ToString());
        Console.WriteLine($" Member {player} has connected");

        while (true)
        {
            int row = await player.TcpClient.ReceiveInt32Async();
            int col = await player.TcpClient.ReceiveInt32Async();
            
            if (board[row, col] == '0' && (isXTurn ? 'X' : 'O') == player.Symbol && players.Count==2)
            {
                board[row, col] = player.Symbol;
                isXTurn = !isXTurn;
            }
            if (CheckWin(row, col, player.Symbol))
            {
               Console.WriteLine($"{player.Symbol} won!");
               await SendAll($"{player.Symbol} won!");
               break;
            }
            if (IsFull())
            {
                Console.WriteLine("It's a draw!");
                await SendAll("It's a draw!");
                break;
            }

            await SendAll(ConvertBoard());
        }
        DisposeAll();
    }

    public void PrintBoard()
    {
        for (int i = 0; i < 25; i++)
        {
            for (int j = 0; j < 25; j++)
            {
                Console.Write(board[i, j] + " ");
            }
            Console.WriteLine();
        }
    }
    public string ConvertBoard()
    {
        string @string = "";
        for (int i = 0; i < 25; i++)
        {
            for (int j = 0; j < 25; j++)
            {
                @string += board[i, j];
            }
            @string += '\n';
        }
        return @string;
    }

    private async Task SendAll(string board)
    {
        IReadOnlyList<Player> playersCopy; 
        lock (players)
            playersCopy = players.ToArray();

        List<Task> tasks = [];
        foreach (Player player in playersCopy) 
            tasks.Add(player.TcpClient.SendStringAsync(board));
        await Task.WhenAll(tasks);
    }

    private void DisposeAll()
    {
        IReadOnlyList<Player> playersCopy;
        lock (players)
            playersCopy = players.ToArray();

        foreach (Player player in playersCopy)
           player.TcpClient.Dispose();
    }

    private void PrepareBoard()
    {
        for (int i = 0; i < 25; i++)
        {
            for (int j = 0; j < 25; j++)
            {
                board[i, j] = '0';
            }
        }
    }

    private bool CheckWin(int row, int col, char symbol)
    {
        int counter = 0;
        for (int i = 0;i<25;i++)
        {
            counter = (board[row, i] == symbol) ? counter + 1 : 0;
            if (counter == 5) return true;
        }

        counter = 0;
        for(int i = 0; i < 25; i++)
        {
            counter = (board[i, col] == symbol) ? counter + 1 : 0;
            if (counter == 5) return true;
        }

        counter = 0;
        for( int i = -5; i < 5; i++)
        {
            int r = row + i, c = col + i;
            if (r >= 0 && r < 25 && c >= 0 && c < 25)
            {
                counter = (board[r, c] == symbol) ? counter + 1 : 0;
                if (counter == 5) return true;
            }
        }

        counter = 0;
        for (int i = -5; i < 5; i++)
        {
            int r = row + i, c = col - i;
            if (r >= 0 && r < 25 && c >= 0 && c < 25)
            {
                counter = (board[r, c] == symbol) ? counter + 1 : 0;
                if (counter == 5) return true;
            }
        }

        return false;
    }

    public bool IsFull()
    {
        bool isBoardFull = true;
        for (int i = 0; i < 25; i++)
        {
            for (int j = 0; j < 25; j++)
            {
                if (board[i, j] == '0')
                {
                    isBoardFull = false;
                    break;
                }
            }
            if (!isBoardFull) break;
        }
        return isBoardFull;
    }
}
