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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SocketUDP
{
    public partial class Form1 : Form
    {
        private Socket udpSocket;
        private IPEndPoint localEndPoint;
        private IPEndPoint remoteEndPoint;
        private Thread receiveThread;
        private bool isListening = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Valeurs par défaut pour faciliter les tests
            textBox1.Text = "127.0.0.1"; // IP locale
            textBox2.Text = "8080";      // Port local
            textBox3.Text = "127.0.0.1"; // IP distante
            textBox4.Text = "8081";      // Port distant
        }

        private void button5_Click(object sender, EventArgs e)
        {
            StopListening();
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                StopListening();

                // Recréer le socket pour pouvoir le réutiliser
                udpSocket?.Close();
                udpSocket = null;

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

                // Démarrer l'écoute en arrière-plan
                StartListening();

                MessageBox.Show($"Socket créé et bindé sur {textBox1.Text}:{textBox2.Text} !\nÉcoute démarrée.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la création du socket : {ex.Message}");
            }
        }

        private void StartListening()
        {
            if (!isListening)
            {
                isListening = true;
                receiveThread = new Thread(ListenForMessages);
                receiveThread.IsBackground = true;
                receiveThread.Start();
            }
        }

        private void StopListening()
        {
            isListening = false;

            // Fermer le socket pour débloquer ReceiveFrom
            if (udpSocket != null)
            {
                try
                {
                    udpSocket.Close();
                }
                catch { }
            }

            // Attendre que le thread se termine
            if (receiveThread != null && receiveThread.IsAlive)
            {
                receiveThread.Join(2000); // Attendre max 2 secondes
            }
        }

        private void ListenForMessages()
        {
            byte[] buffer = new byte[1024];
            EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);

            while (isListening)
            {
                try
                {
                    if (udpSocket != null && udpSocket.IsBound)
                    {
                        // Définir un timeout pour éviter les blocages
                        udpSocket.ReceiveTimeout = 1000; // 1 seconde

                        // Utiliser ReceiveFrom pour obtenir l'adresse de l'expéditeur
                        int bytesReceived = udpSocket.ReceiveFrom(buffer, ref remoteEP);

                        if (bytesReceived > 0)
                        {
                            string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesReceived);

                            // Mise à jour thread-safe de l'interface utilisateur
                            if (this.InvokeRequired)
                            {
                                this.BeginInvoke(new Action(() =>
                                {
                                    string timestamp = DateTime.Now.ToString("HH:mm:ss");
                                    richTextBox2.AppendText($"[{timestamp}] Reçu de {remoteEP}: {receivedMessage}\r\n");
                                    richTextBox2.ScrollToCaret();
                                }));
                            }
                        }
                    }
                }
                catch (SocketException ex)
                {
                    // Ignorer les timeouts, continuer l'écoute
                    if (ex.SocketErrorCode == SocketError.TimedOut)
                    {
                        continue;
                    }

                    // Ignorer les erreurs de connexion fermée si on arrête l'écoute
                    if (!isListening || ex.SocketErrorCode == SocketError.Interrupted)
                    {
                        break;
                    }

                    // Log autres erreurs mais continuer
                    if (this.InvokeRequired)
                    {
                        this.BeginInvoke(new Action(() =>
                        {
                            richTextBox2.AppendText($"[INFO] Erreur réseau temporaire: {ex.SocketErrorCode}\r\n");
                            richTextBox2.ScrollToCaret();
                        }));
                    }

                    // Petite pause avant de réessayer
                    Thread.Sleep(100);
                }
                catch (ObjectDisposedException)
                {
                    // Socket fermé, arrêter l'écoute proprement
                    break;
                }
                catch (Exception ex)
                {
                    // Autres exceptions
                    if (isListening && this.InvokeRequired)
                    {
                        this.BeginInvoke(new Action(() =>
                        {
                            richTextBox2.AppendText($"[ERREUR] Exception inattendue: {ex.Message}\r\n");
                        }));
                    }
                    Thread.Sleep(100);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                if (udpSocket == null || !udpSocket.IsBound)
                {
                    MessageBox.Show("Veuillez d'abord créer et lier le socket !");
                    return;
                }

                if (string.IsNullOrWhiteSpace(richTextBox1.Text))
                {
                    MessageBox.Show("Veuillez saisir un message !");
                    return;
                }

                string message = richTextBox1.Text;
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);

                // Créer un point de terminaison pour la destination
                remoteEndPoint = new IPEndPoint(IPAddress.Parse(textBox3.Text), int.Parse(textBox4.Text));

                // Envoyer le message
                udpSocket.SendTo(messageBytes, remoteEndPoint);

                // Afficher le message envoyé dans la zone de réception
                string timestamp = DateTime.Now.ToString("HH:mm:ss");
                richTextBox2.AppendText($"[{timestamp}] Envoyé vers {remoteEndPoint}: {message}\r\n");
                richTextBox2.ScrollToCaret();

                // Vider la zone de saisie
                richTextBox1.Clear();
                richTextBox1.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'envoi du message : {ex.Message}");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // Cette fonction n'est plus nécessaire car l'écoute se fait automatiquement
            // Mais on peut l'utiliser pour effacer l'historique des messages
            if (MessageBox.Show("Voulez-vous effacer l'historique des messages ?",
                               "Effacer l'historique",
                               MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                richTextBox2.Clear();
            }
        }

        private void richTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            // Permettre d'envoyer avec Entrée
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true;
                button3_Click(sender, e);
            }
        }

        // Gestionnaires d'événements existants (non modifiés)
        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            // Vous pouvez supprimer cette ligne car elle n'est pas utilisée
            // var msg = Encoding.ASCII.GetBytes("Texte à envoyer");
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

        private void richTextBox2_TextChanged(object sender, EventArgs e)
        {
            // Supprimé le code problématique qui était ici
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopListening();
            udpSocket?.Close();
        }
    }
}