using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;



namespace BadgeScreen.M2VM.ViewModels
{
    public class MainViewModel
    {
        private TcpClient client;
        private NetworkStream stream;
        private Dictionary<string, string> characterTrad;

        public MainViewModel()
        {
            InitMappeur();
        }

        public void Connect(int port)
        {
            client = new TcpClient("127.0.0.1", port); //On fait que avec une connexion locale
            stream = client.GetStream();
        }

        public void SendMessage(string message)
        {
            if (client == null || !client.Connected)
                throw new InvalidOperationException("Client is not connected.");

            byte[] data = Encoding.ASCII.GetBytes(message + Environment.NewLine);
            stream.Write(data, 0, data.Length);
        }

        public void Disconnect()
        {
            stream.Close();
            client.Close();
        }

        public void SendToOneDevice(string messages)
        {

            if (messages == "")
            {
                throw new Exception("No message to send");
            }
            Debug.Print("Debut de l'envoie");

            
            List<byte[]> contents = new List<byte[]>();


            string messageBuilder = transformMessage(messages).ToString();


            // Envoie les données via le flux (stream)
            using (NetworkStream stream = client.GetStream())
            {
                byte[] buffer = Encoding.ASCII.GetBytes(messageBuilder);
                stream.Write(buffer, 0, buffer.Length);
            }

            Debug.Print("Envoye");


        }


        private StringBuilder transformMessage(string messages)
        {
            int compteur = 1;
            StringBuilder reponse = new StringBuilder();
            string debug;
            int debut = 0;
            int longueur = 5;

            for (int i = 1; i < 11; i++)
            {

                foreach (char lettre in messages)
                {

                    debug = this.characterTrad[lettre.ToString()].Substring(debut, longueur);

                    reponse.Append(this.characterTrad[lettre.ToString()].Substring(debut, longueur));

                }

                while (reponse.Length < 44 * compteur)
                {
                    reponse.Append("0");
                }

                compteur++;
                debut += 5;

            }


            return reponse;
        }

        private byte[] GetSecondEnteteToSendContents(string messages)
        {
            var bArr = new byte[16];

            // Transform the message length into 2 bytes
            bArr[0] = (byte)(messages.Length >> 8);  // MSB
            bArr[1] = (byte)messages.Length;         // LSB
            

            return bArr;
        }

        private void InitMappeur()
        {
            this.characterTrad = new Dictionary<string, string>();

            characterTrad.Add("A", "0000000000000000011001001011110100101001000000000000000");
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
        }

    }
}
