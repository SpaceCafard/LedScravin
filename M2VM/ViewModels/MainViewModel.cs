using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;



namespace BadgeScreen.M2VM.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private TcpClient client;
        private NetworkStream stream;
        private Dictionary<string, string> characterTrad;
        private ObservableCollection<int> ports;

        private string scanStatus;

        public string ScanStatus
        {
            get => scanStatus;
            set
            {
                scanStatus = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public MainViewModel()
        {
            ports = new ObservableCollection<int>();
            ScanStatus = "Scan pas encore lancé";
            InitMappeur();
        }

        public void ConnectToPort(int port)
        {
            ////Fermer les anciennes connexions
            //if(stream!=null && client!= null)
            //{
            //    Disconnect();
            //}

            //Créer la nouvelle connexion
            client = new TcpClient("127.0.0.1", port); //On fait que avec une connexion locale
            stream = client.GetStream();

            Trace.WriteLine("STREAM :" + stream);
        }

        public void AddPortToPortsList(int port)
        {
            ports.Add(port);
        }

        public async Task<List<int>> ScanPortsAsync()
        {
            ScanStatus = "Scan en cours...";

            var openPorts = new List<int>();

            for (int port = 1233; port <= 1236; port++)
            {
                try
                {
                    TcpClient connectTest = new("127.0.0.1", port);
                    NetworkStream ns = connectTest.GetStream();

                    string scanMsg = transformMessage("SCANTEST").ToString();
                    byte[] buffer = Encoding.ASCII.GetBytes(scanMsg);
                    await ns.WriteAsync(buffer);

                    await ns.FlushAsync();
                    ns.Close();

                    openPorts.Add(port);

                    Trace.WriteLine($"Alive connection on port {port}");
                }
                catch
                {
                    continue;
                }
            }

            ScanStatus = "Scan terminé !";

            return openPorts;
        }

        //public async Task ScanPort()
        //{
        //    string ipAddress = "127.0.0.1";
        //    Trace.WriteLine("Début du scan");

        //    ScanStatus = "Scan en cours...";

        //    var tasks = new List<Task>();
        //    var semaphore = new SemaphoreSlim(60); // Limiter à 60 connexions simultanées

        //    for (int port = 1000; port < 2000; port++)
        //    {
        //        await semaphore.WaitAsync();

        //        tasks.Add(Task.Run(async () =>
        //        {
        //            try
        //            {
        //                await CheckPortAsync(ipAddress, port);
        //            }
        //            finally
        //            {
        //                semaphore.Release();
        //            }
        //        }));
        //    }

        //    await Task.WhenAll(tasks);

        //    Trace.WriteLine("Fin du scan");

        //    ScanStatus = "Scan terminé !";

        //    // Connect to all found ports
        //    foreach (var port in ports)
        //    {
        //        ConnectToPort(port);
        //    }
        //}

        //private async Task CheckPortAsync(string ipAddress, int port)
        //{
        //    using (TcpClient tcpClient = new TcpClient())
        //    {
        //        Trace.WriteLine("Scanning port " + port);
        //        try
        //        {
        //            await tcpClient.ConnectAsync(ipAddress, port);
        //            Trace.WriteLine($"TCP listener found at {ipAddress}:{port}");

        //            ports.Add(port);
        //        }
        //        catch (SocketException)
        //        {
        //            // Silently ignore exceptions
        //        }
        //    }
        //}

        public async void Disconnect()
        {
            await stream.FlushAsync();
            stream.Close();
        }

        public void SendMessage(string message)
        {
            // Tester si le message est vide
            if (message == "")
            {
                throw new Exception("No message to send");
            }
            Debug.Print("Debut de l'envoie");

            foreach (var portHere in ports)
            {
                // Si le message n'est pas vide, on le passe d'abord en majuscules
                message = message.ToUpper();

                //Ensuite on crée la suite de 0 et 1 que l'on veut envoyer grace à la méthode transformMessage
                string messageBuilder = transformMessage(message).ToString();


                // Envoie les données via le flux (stream)
                NetworkStream stream = client.GetStream();
                
                byte[] buffer = Encoding.ASCII.GetBytes(messageBuilder);
                stream.Write(buffer, 0, buffer.Length);

                Disconnect();
                

                ////Se reconnecter après un envoi
                //ConnectToPort(portHere);
            }
            

            Debug.Print("Envoye");
        }


        private StringBuilder transformMessage(string message)
        {
            int compteur = 1;
            StringBuilder reponse = new StringBuilder();
            string debug;
            int debut = 0;
            int longueur = 5;

            // Construction de la suite de 0 et de 1 à envoyer au badge
            // On la construit ligne par ligne, il y a 11 lignes, donc on fait une boucle qui boucle 11 fois :
            //
            // Sur la première ligne, on append les 5 premiers byte (car debut=0 et longueur=5) de chaque lettre
            // Sur la deuxième ligne, on append les bytes de 5 à 10 (car debut=5 et longueur=5) de chaque lettre
            // Sur la troisième ligne, on append les bytes de 10 à 15 (car debut=10 et longueur=5) de chaque lettre
            // Sur la quatrième ligne, on append les bytes de 15 à 20 (car debut=15 et longueur=5) de chaque lettre
            // etc pour chaque ligne
            //
            // La boucle for permet donc de coder chaque ligne
            // Et dans cette boucle for, la boucle foreach nous permet de coder chaque bout de lettre présent sur cette ligne
            //
            // La variable debut est incrémenter de 5 à la fin de chaque ligne
            // Puisque quand on passe a la ligne suivante on veux chercher le morceau suivant de la lettre
            // Et vu que les lettres font 5 byte de large, le morceau du dessous se situt 5 byte plus loin

            //Pour chaque ligne a coder (11 lignes)
            for (int i = 1; i < 11; i++)
            {
                // Pour chaque lettre a coder (défini par le nombre de lettres présentes dans le message)
                foreach (char lettre in message)
                {
                    // Ajouter le petit bout de lettre à la ligne 
                    reponse.Append(this.characterTrad[lettre.ToString()].Substring(debut, longueur));
                }

                // Si le message à envoyer ne contient pas asser de lettre pour couvrir tout l'écran,
                // il faut couvrir le reste avec des 0
                // Chaque ligne fait 44 pixels

                // Tant que la ligne ne fait pas 44 pixels on ajoute un zero
                while (reponse.Length < 44 * compteur)
                {
                    reponse.Append("0");
                }
                //On multiplie 44 par un compteur qui a compté le nombre d'itération de la boucle
                //Le nombre d'itération de la boucle correspond au numéro de la ligne que l'on est entrain de coder
                //C'est pour cela que 44 est multiplier par le numéro de ligne à laquelle on est
                //C'est parcque que l'on ne veut pas couvrir les lignes du dessous de 0, mais seulement la fin de la ligne actuelle

                compteur++;
                debut += 5;

            }

            return reponse;
        }

        private void InitMappeur()
        {
            this.characterTrad = new Dictionary<string, string>();

            characterTrad.Add("A", "00000" +
                                   "00000" +
                                   "00000" +
                                   "00110" +
                                   "01001" +
                                   "01111" +
                                   "01001" +
                                   "01001" +
                                   "00000" +
                                   "00000" +
                                   "00000");
            characterTrad.Add("B", "0000000000000000111001001011100100101110000000000000000");
            characterTrad.Add("C", "0000000000000000011101000010000100000111000000000000000");
            characterTrad.Add("D", "0000000000000000111001001010010100101110000000000000000");
            characterTrad.Add("E", "0000000000000000111101000011100100001111000000000000000");
            characterTrad.Add("F", "0000000000000000111101000011100100001000000000000000000");
            characterTrad.Add("G", "0000000000000000011101000010110100100111000000000000000");
            characterTrad.Add("H", "0000000000000000100101001011110100101001000000000000000");
            characterTrad.Add("I", "0000000000000000100001000010000100001000000000000000000");
            characterTrad.Add("J", "0000000000000000000100001000010100100110000000000000000");
            characterTrad.Add("K", "0000000000000000100101010011000101001001000000000000000");
            characterTrad.Add("L", "0000000000000000100001000010000100001110000000000000000");
            characterTrad.Add("M", "0000000000000001000111011101011000110001000000000000000");
            characterTrad.Add("N", "0000000000000000100101101010110100101001000000000000000");
            characterTrad.Add("O", "0000000000000000011001001010010100100110000000000000000");
            characterTrad.Add("P", "0000000000000000111001001011100100001000000000000000000");
            characterTrad.Add("Q", "0000000000000000011001001010010101100111000000000000000");
            characterTrad.Add("R", "0000000000000000111001001011100101001001000000000000000");
            characterTrad.Add("S", "0000000000000000011101000001100000101110000000000000000");
            characterTrad.Add("T", "0000000000000000111000100001000010000100000000000000000");
            characterTrad.Add("U", "0000000000000000100101001010010100101111000000000000000");
            characterTrad.Add("V", "0000000000000000101001010010100101000100000000000000000");
            characterTrad.Add("W", "0000000000000001000110001100011010101010000000000000000");
            characterTrad.Add("X", "0000000000000000101001010001000101001010000000000000000");
            characterTrad.Add("Y", "0000000000000000101001010011100010000100000000000000000");
            characterTrad.Add("Z", "0000000000000000111100001000100010001111000000000000000");
            characterTrad.Add(" ", "0000000000000000000000000000000000000000000000000000000");
            characterTrad.Add("!", "0000000000001000010000100001000000000100000000000000000");

            characterTrad.Add("1", "0000000000000100011001010000100001000010000000000000000");
            characterTrad.Add("2", "0000000000001100100100010000100010001111000000000000000");
            characterTrad.Add("3", "0000000000011110000100111000010000101111000000000000000");
            characterTrad.Add("4", "0000000000010010100101111000010000100001000000000000000");
            characterTrad.Add("5", "0000000000011110100001111000010000101111000000000000000");
            characterTrad.Add("6", "0000000000011110100001111010010100101111000000000000000");
            characterTrad.Add("7", "0000000000011110000100010000100010000100000000000000000");
            characterTrad.Add("8", "0000000000011110100101111010010100101111000000000000000");
            characterTrad.Add("9", "0000000000011110100101111000010000101111000000000000000");
            characterTrad.Add("0", "0000000000011110100101001010010100101111000000000000000");
        }
    }
}
