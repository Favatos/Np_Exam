using System.Net.Sockets;
using System.Text;
using System.Windows;
using TcpLib;

namespace Client;

public partial class Login : Window
{
    private LoginVM loginVM = new LoginVM();

    public Login()
    {
        InitializeComponent();

        DataContext = loginVM;
    }

    private async void ButtonConnect_Click(object sender, RoutedEventArgs e)
    {
        if (t1.Text.Length == 0 || t2.Text.Length == 0 ||t3.Text.Length==0)
        {
            MessageBox.Show("Please fill in all the fields", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        TcpClient server = new();
        try
        {
            await server.ConnectAsync(loginVM.Host, loginVM.Port);
            await server.SendStringAsync(loginVM.Name);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Sorry, wrong data or 2 players have already joined the game");
            return;
        }

        MainWindow mainWindow = new MainWindow(server, loginVM.Name);
        Hide();
        mainWindow.ShowDialog();
        Close();
    }
}
