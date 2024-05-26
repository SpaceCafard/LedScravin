
using BadgeScreen.M2VM.Models;
using System.Windows;

namespace BadgeScreen
{
    public partial class MainWindow : Window
    {
        private MainModel mainModel;

        public MainWindow()
        {
            InitializeComponent();
            mainModel = new MainModel("127.0.0.1", 1700); // Utilisez l'adresse IP et le port corrects
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                mainModel.Connect();
                StatusTextBlock.Text = "Connected to the server!";
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"Connection failed: {ex.Message}";
            }
        }

        private void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string message = MessageTextBox.Text;
                mainModel.SendToOneDevice(message);
                StatusTextBlock.Text = "Message sent!";
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"Failed to send message: {ex.Message}";
            }
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            mainModel.Disconnect();
            StatusTextBlock.Text = "Disconnected from the server.";
        }
    }
}
