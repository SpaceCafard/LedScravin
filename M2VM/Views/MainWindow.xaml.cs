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
            messages = new ObservableCollection<string>()
            {
                "SALUT",
                "YANN",
                "MOUSS",
                "ABC",
                "ZYX",
                "ZZZZZ"
            };
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

        private void SortAlphabeticallyButton_Click(object sender, RoutedEventArgs e)
        {
            var sortedMessages = messages.OrderBy(m => m).ToList();
            UpdateMessages(sortedMessages);
        }

        private void SortSizeAscendingButton_Click(object sender, RoutedEventArgs e)
        {
            var sortedMessages = messages.OrderBy(m => m.Length).ToList();
            UpdateMessages(sortedMessages);
        }

        private void SortSizeDescendingButton_Click(object sender, RoutedEventArgs e)
        {
            var sortedMessages = messages.OrderByDescending(m => m.Length).ToList();
            UpdateMessages(sortedMessages);
        }
        
        private void SearchTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string searchText = SearchTextBox.Text.ToLower();
            if (string.IsNullOrEmpty(searchText))
            {
                // If search text is empty, reset to original messages
                UpdateMessages(messages);
            }
            else
            {
                // Filter messages based on search text
                var filtered = messages.Where(m => m.ToLower().Contains(searchText)).ToList();
                UpdateMessages(filtered);
            }
        }

        private void UpdateMessages(IEnumerable<string> sortedMessages)
        {
            messages.Clear();
            foreach (var message in sortedMessages)
            {
                messages.Add(message);
            }
        }
    }
}