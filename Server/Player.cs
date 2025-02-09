using System.Net;
using System.Net.Sockets;

namespace ChatServer;

public class Player
{
    public string? Name { get; set; }
    public char Symbol { get; set; }
    public TcpClient TcpClient { get; }
    public IPEndPoint IPEndPoint => (IPEndPoint)TcpClient.Client.RemoteEndPoint!;

    public Player(TcpClient tcpClient, char symbol)
    {
        TcpClient = tcpClient;
        Symbol = symbol;    
    }

    public override string ToString() => $"{Name} {Symbol} {IPEndPoint}";
}
