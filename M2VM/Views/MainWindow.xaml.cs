using BadgeScreen.M2VM.ViewModels;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Windows;

namespace ProjetBadge
{
    public partial class MainWindow : Window
    {
        private TcpClient client;
        private NetworkStream stream;
        private MainViewModel mainViewModel;
        private ObservableCollection<string> messages;

        public MainWindow()
        {
            InitializeComponent();
            mainViewModel = new MainViewModel();
            messages = new ObservableCollection<string>();
            MessagesListBox.ItemsSource = messages;
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

        private void SaveMessageButton_Click(object sender, RoutedEventArgs e)
        {
            string message = SaveMessageTextBox.Text;
            if (!string.IsNullOrWhiteSpace(message))
            {
                messages.Add(message);
                SaveMessageTextBox.Clear();
            }
        }

        private void MessagesListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            bool isItemSelected = MessagesListBox.SelectedItem != null;
            EditButton.IsEnabled = isItemSelected;
            DeleteButton.IsEnabled = isItemSelected;
            SendSelectedButton.IsEnabled = isItemSelected;
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessagesListBox.SelectedItem != null)
            {
                string selectedMessage = MessagesListBox.SelectedItem as string;
                SaveMessageTextBox.Text = selectedMessage;
                messages.Remove(selectedMessage);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessagesListBox.SelectedItem != null)
            {
                string selectedMessage = MessagesListBox.SelectedItem as string;
                messages.Remove(selectedMessage);
            }
        }

        private void SendSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessagesListBox.SelectedItem != null)
            {
                try
                {
                    string selectedMessage = MessagesListBox.SelectedItem as string;
                    mainViewModel.SendToOneDevice(selectedMessage);
                    StatusTextBlock.Text = "Message sent!";
                }
                catch (Exception ex)
                {
                    StatusTextBlock.Text = $"Failed to send message: {ex.Message}";
                }
            }
        }
    }
}