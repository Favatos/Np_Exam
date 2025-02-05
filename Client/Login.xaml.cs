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
        TcpClient server = new();
        await server.ConnectAsync(loginVM.Host, loginVM.Port);
        await server.SendStringAsync(loginVM.Name);

        MainWindow mainWindow = new MainWindow(server);
        Hide();
        mainWindow.ShowDialog();
        Close();
    }
}
