using BadgeScreen.M2VM.ViewModels;
using System;
using System.Net.Sockets;
using System.Text;
using System.Windows;

namespace ProjetBadge
{
    public partial class MainWindow : Window
    {
        private TcpClient client;
        private NetworkStream stream;
        private MainViewModel mainViewModel;

        public MainWindow()
        {
            InitializeComponent();
            mainViewModel = new MainViewModel();
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(this.PortTextBox.Text, out int port))
            {
                try
                {
                    mainViewModel.Connect(port);
                    StatusTextBlock.Text = "Connecté !";
                }
                catch (Exception ex)
                {
                    StatusTextBlock.Text = $"La connexion a échouée: {ex.Message}";
                }
            }
            else
            {
                StatusTextBlock.Text = "Numéro de port non valide.";
            }
        }

        private void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string message = MessageTextBox.Text;
                mainViewModel.SendToOneDevice(message);
                StatusTextBlock.Text = "Message sent!";
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"Failed to send message: {ex.Message}";
            }
        }
    }
}
