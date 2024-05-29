using BadgeScreen.M2VM.ViewModels;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        private ObservableCollection<int> ports;
        private ObservableCollection<string> filteredMessages; // New collection for filtered messages

        public MainWindow()
        {
            InitializeComponent();
            
            mainViewModel = new MainViewModel();
            messages = mainViewModel.InitMessages();
            filteredMessages = new ObservableCollection<string>(messages);
            ports = new ObservableCollection<int>();
            PortConnectedListBox.ItemsSource = ports;
            MessagesListBox.ItemsSource = filteredMessages;
            this.DataContext = mainViewModel;
        }

        private void AjouterPortButton_Click(object sender, RoutedEventArgs e)
        {
            //int port = Int32.Parse(PortTextBox.Text);

            if (int.TryParse(this.PortTextBox.Text, out int port))
            {
                try{
                    mainViewModel.ConnectToPort(port);
                    StatusTextBlock.Text = "Port ajouté !";

                    mainViewModel.AddPortToPortsList(port);
                    ports.Add(port);
                    PortTextBox.Clear();
                }
                catch (Exception ex)
                {
                    StatusTextBlock.Text = $"La connexion a échouée: {ex.Message}";

                    Trace.WriteLine($"La connexion a échouée: {ex.Message}");
                }
            }
            else
            {
                StatusTextBlock.Text = "Numéro de port non valide.";
            }
        }

        private void PortConnectedListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            bool isItemSelected = PortConnectedListBox.SelectedItem != null;
            EditPortButton.IsEnabled = isItemSelected;
            DeletePortButton.IsEnabled = isItemSelected;
        }

        private void EditPortButton_Click(object sender, RoutedEventArgs e)
        {
            if (PortConnectedListBox.SelectedItem != null)
            {
                if (PortConnectedListBox.SelectedItem is int selectedPort)
                {
                    Trace.WriteLine("Port : " + selectedPort);
                    PortTextBox.Text = selectedPort.ToString();
                    ports.Remove(selectedPort);
                    mainViewModel.RemovePortFromList(selectedPort);
                }
            }
        }

        private void DeletePortButton_Click(object sender, RoutedEventArgs e)
        {
            if (PortConnectedListBox.SelectedItem != null)
            {
                if (PortConnectedListBox.SelectedItem is int selectedPort)
                {
                    Trace.WriteLine("Port : " + selectedPort);
                    ports.Remove(selectedPort);
                    mainViewModel.RemovePortFromList(selectedPort);
                }
            }
        }

        public async void ScanButton_Click(object sender, RoutedEventArgs e)
        {
            //await mainViewModel.ScanPort();

            var openPorts = await mainViewModel.ScanPortsAsync();
            foreach (var port in openPorts)
            {
                PortListBox.Items.Add($"Port {port} is open");
            }
        }

        private void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string message = MessageTextBox.Text;
                mainViewModel.SendMessage(message);
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
                    mainViewModel.SendMessage(selectedMessage);
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

        //Importer des messages depuis un fichier csv
        private void ImportCsvButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                mainViewModel.ImportCsv(openFileDialog.FileName, messages);
            }

            RefreshMessagesList();
        }

        //Exporter la liste des messages dans un fichier csv
        private void ExportCsvButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            if (saveFileDialog.ShowDialog() == true)
            {
                mainViewModel.ExportCsv(saveFileDialog.FileName, messages);
            }
        }

        //Importer des messages depuis un fichier excel
        private void ImportExcelButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                mainViewModel.ImportExcel(openFileDialog.FileName, messages);
            }

            RefreshMessagesList();
        }

        //Exporter la liste des messages dans un fichier excel
        private void ExportExcelButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
            if (saveFileDialog.ShowDialog() == true)
            {
                mainViewModel.ExportExcel(saveFileDialog.FileName, messages);
            }
        }

        //Rafraichir la liste des message après un import
        private void RefreshMessagesList()
        {
            filteredMessages.Clear();
            foreach (var message in messages)
            {
                filteredMessages.Add(message);
            }
        }
    }
}
