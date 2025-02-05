using System.Net.Http;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using TcpLib;

namespace Client;


public partial class MainWindow : Window
{
    public char Symbol {  get; set; }
    private TcpClient server;
    private CancellationTokenSource cancel = new();
    Grid grid = new Grid();
    object key = new object();
    public MainWindow(TcpClient server)
    {
        InitializeComponent();
        this.server = server;
        //MessageBox.Show(this.server.Client.RemoteEndPoint.ToString());

        for (int i = 0; i < 25; i++)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
        }
        int counter = 0;
        for (int i = 0; i < 25; i++)
        {
            for (int j = 0; j < 25; j++)
            {
                counter++;  
                Button b = new Button();
                b.Name = "B" + counter;
                b.Click += Button_Click;
                b.Margin = new Thickness(2);
                b.Content = "   ";
                Grid.SetRow(b, j);
                Grid.SetColumn(b, i);
                grid.Children.Add(b);
            }
        }
        Content = grid;

        //_ = ListenToServer(cancel.Token);
    }

    private async void Button_Click(object sender, RoutedEventArgs e)
    {
        Button b  = (Button)sender;
        b.Content = Symbol;
        int row = Grid.GetRow(b);
        int col = Grid.GetColumn(b);

        await server.SendInt32Async(row);
        await server.SendInt32Async(col);
        RecreateBoard(await server.ReceiveJsonAsync<char[,]>());
    }

    private async Task ListenToServer(CancellationToken token)
    {
        Symbol = char.Parse(await server.ReceiveStringAsync());
        while (true)
        {
            if (token.IsCancellationRequested)
                break;
            RecreateBoard(await server.ReceiveJsonAsync<char[,]>());
        }

        server.Dispose();
    }

    private void RecreateBoard(char[,] board)
    {
        foreach(Button button in grid.Children)
        {
            int row = Grid.GetRow(button);
            int col = Grid.GetColumn(button);
            button.Content = board[row, col];
        }
    }
    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        Symbol = char.Parse(await server.ReceiveStringAsync());
        MessageBox.Show(Symbol.ToString());
    }
}