using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SocketUDP
{
    public partial class Form1 : Form
    {
        private Socket udpSocket;
        private IPEndPoint localEndPoint;
        private IPEndPoint remoteEndPoint;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                udpSocket.Close();
                MessageBox.Show("Socket fermée !");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la fermeture du socket : {ex.Message}");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // Créer un socket UDP
                udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                // Lier le socket à l'IP locale et au port
                localEndPoint = new IPEndPoint(IPAddress.Parse(textBox1.Text), int.Parse(textBox2.Text));
                udpSocket.Bind(localEndPoint);

                MessageBox.Show("Socket créé et bindé avec succès !");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la création du socket : {ex.Message}");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                string message = richTextBox1.Text;
                byte[] messageBytes = Encoding.ASCII.GetBytes(message);

                // Créer un point de terminaison pour la destination
                remoteEndPoint = new IPEndPoint(IPAddress.Parse(textBox3.Text), int.Parse(textBox4.Text));

                // Envoyer le message
                udpSocket.SendTo(messageBytes, remoteEndPoint);

                MessageBox.Show("Message envoyé !");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'envoi du message : {ex.Message}");
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            var msg = Encoding.ASCII.GetBytes("Texte à envoyer");
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] buffer = new byte[1024];
                udpSocket.ReceiveTimeout = 5000; // Timeout de 5 secondes

                // Recevoir les données (blocage jusqu'à réception ou timeout)
                int bytesReceived = udpSocket.Receive(buffer);
                string receivedMessage = Encoding.ASCII.GetString(buffer, 0, bytesReceived);

                // Afficher le message reçu dans la TextBox
                richTextBox2.Text = receivedMessage;

                MessageBox.Show("Message reçu !");
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.TimedOut)
                {
                    MessageBox.Show("Timeout: Aucun message reçu.");
                }
                else
                {
                    MessageBox.Show($"Erreur lors de la réception du message : {ex.Message}");
                }
            }
        }

        private void richTextBox2_TextChanged(object sender, EventArgs e)
        {
            var buffer = new byte[1024];
            this.textBox1.Text += Encoding.ASCII.GetString(buffer, 0, buffer.Length);
        }
    }
}
 