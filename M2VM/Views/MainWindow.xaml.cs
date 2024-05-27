using BadgeScreen.M2VM.ViewModels;
using System.Collections.ObjectModel;
using System.Linq;
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
        private ObservableCollection<string> filteredMessages; // New collection for filtered messages

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
            filteredMessages = new ObservableCollection<string>(messages);
            MessagesListBox.ItemsSource = filteredMessages;
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
                message = message.ToUpper();
                messages.Add(message);
                SaveMessageTextBox.Clear();
                ApplyFilter(); // Apply the current filter to include the new message if necessary
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
                ApplyFilter(); // Apply the current filter after removing the message
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessagesListBox.SelectedItem != null)
            {
                string selectedMessage = MessagesListBox.SelectedItem as string;
                messages.Remove(selectedMessage);
                ApplyFilter(); // Apply the current filter after removing the message
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
            var sortedMessages = filteredMessages.OrderBy(m => m).ToList();
            UpdateFilteredMessages(sortedMessages);
        }

        private void SortSizeAscendingButton_Click(object sender, RoutedEventArgs e)
        {
            var sortedMessages = filteredMessages.OrderBy(m => m.Length).ToList();
            UpdateFilteredMessages(sortedMessages);
        }

        private void SortSizeDescendingButton_Click(object sender, RoutedEventArgs e)
        {
            var sortedMessages = filteredMessages.OrderByDescending(m => m.Length).ToList();
            UpdateFilteredMessages(sortedMessages);
        }

        private void SearchTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            string searchText = SearchTextBox.Text.ToLower();
            if (string.IsNullOrEmpty(searchText))
            {
                UpdateFilteredMessages(messages);
            }
            else
            {
                var filtered = messages.Where(m => m.ToLower().Contains(searchText)).ToList();
                UpdateFilteredMessages(filtered);
            }
        }

        private void UpdateFilteredMessages(IEnumerable<string> updatedMessages)
        {
            filteredMessages.Clear();
            foreach (var message in updatedMessages)
            {
                filteredMessages.Add(message);
            }
        }
    }
}
