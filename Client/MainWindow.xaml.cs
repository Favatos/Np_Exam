using System.Net.Http;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TcpLib;

namespace Client;


public partial class MainWindow : Window
{
    public char Symbol {  get; set; }
    private TcpClient server;
    private string name;
    private CancellationTokenSource cancel = new();
    Grid grid = new Grid();
    private List<SolidColorBrush> brushes = [new SolidColorBrush(Colors.Red), new SolidColorBrush(Colors.Blue)];
    public MainWindow(TcpClient server, string name)
    {
        InitializeComponent();
        this.server = server;
        this.name = name;

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
                b.Background = new SolidColorBrush(Colors.LightBlue);
                b.MouseEnter += (s, e) => b.Background = new SolidColorBrush(Colors.LightGreen);
                b.MouseLeave += (s, e) => b.Background = new SolidColorBrush(Colors.LightBlue);
                Grid.SetRow(b, j);
                Grid.SetColumn(b, i);
                grid.Children.Add(b);
            }
        }
        Content = grid;

        _ = ListenToServer(cancel.Token);
    }

    private async void Button_Click(object sender, RoutedEventArgs e)
    {
        Button b  = (Button)sender;
        b.Content = Symbol;
        b.Foreground = Symbol == 'X' ? brushes[0] : brushes[1];
        int row = Grid.GetRow(b);
        int col = Grid.GetColumn(b);

        if (server.Connected)
        {
            await server.SendInt32Async(row);
            await server.SendInt32Async(col);
        }
        else
        {
            MessageBox.Show("Sorry, 2 players have already joined the game");
            Close();
        }
    }

    private async Task ListenToServer(CancellationToken token)
    { 
        while (true)
        {
            if (token.IsCancellationRequested)
                break;

            string str = await server.ReceiveStringAsync();
            if (str == "X" || str == "O")
            {
                Symbol = char.Parse(str);
                Title = str;
                MessageBox.Show($"Hi, {name}! You are {Symbol.ToString()}!");
            }
            else if(str == "X won!" || str=="O won!")
            {
                MessageBox.Show(str, "Victory", MessageBoxButton.OK);
                Close();
            }
            else if(str == "It's a draw!")
            {
                MessageBox.Show(str, "Draw");
                Close();
            }
            else
                RecreateBoard(str);
        }

        server.Dispose();
    }

    private void RecreateBoard(string @string)
    {
        string[] str = @string.Split('\n');
        foreach(Button button in grid.Children)
        {
            int row = Grid.GetRow(button);
            int col = Grid.GetColumn(button);
            button.Content = str[row][col]=='0'? '\0' : str[row][col];
            if (str[row][col] == 'X') button.Foreground = brushes[0];
            else if (str[row][col] =='O') button.Foreground = brushes[1];
        }
    }

}