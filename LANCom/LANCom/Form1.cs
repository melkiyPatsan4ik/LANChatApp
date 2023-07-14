using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace LANCom
{
    public partial class Form1 : Form

    {
        private UdpClient udpClient;
        private Thread receiveThread;
        private const int Port = 8888;
        private IPEndPoint remoteEP;
        public Form1()
        {
            InitializeComponent();
            udpClient = new UdpClient();
            udpClient.EnableBroadcast = true;
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            receiveThread = new Thread(ReceiveMessages);
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }
        private void ReceiveMessages()
        {
            try
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, Port);
                while (true)
                {
                    byte[] data = udpClient.Receive(ref remoteEP);
                    string message = Encoding.ASCII.GetString(data);
                    DisplayReceivedMessage(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        private void DisplayReceivedMessage(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(DisplayReceivedMessage), message);
            }
            else
            {

                textBox4.AppendText(message + Environment.NewLine);
                listBox1.Items.Add(message);
                label2.Text = "New Message: " + message;
                label2.Visible = true;
                WaitForAcknowledgment();
            }
            SendAcknowledgment(remoteEP);
        }
        private void WaitForAcknowledgment()
        {
            try
            {
                udpClient.Client.ReceiveTimeout = 5000; // 5 seconds
                byte[] data = udpClient.Receive(ref remoteEP);
                string acknowledgment = Encoding.ASCII.GetString(data);
                if (acknowledgment == "ACK")
                {
                    MessageBox.Show("Message delivered successfully!");
                }
                else
                {
                    MessageBox.Show("Failed to deliver the message!");
                }
            }
            catch (SocketException ex)
            {
                MessageBox.Show("Error receiving acknowledgment: " + ex.Message);
            }
        }
        private void send_Click(object sender, EventArgs e)
        {
            string message = textBox1.Text;
            SendMessage(message);
            WaitForAcknowledgment();
        }
        private void SendMessage(string message)
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            udpClient.Send(data, data.Length, new IPEndPoint(IPAddress.Broadcast, Port));
            udpClient.Send(data, data.Length, new IPEndPoint(IPAddress.Parse("127.0.0.1"), Port));
            textBox1.Clear();
        }
        private void SendAcknowledgment(IPEndPoint senderEP)
        {

            byte[] acknowledgmentData = Encoding.ASCII.GetBytes("ACK");
            udpClient.Send(acknowledgmentData, acknowledgmentData.Length, senderEP);
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            string[] ipAddresses = DiscoverDevices();

            foreach (string ipAddress in ipAddresses)
            {
                listBox1.Items.Add(ipAddress);
            }
        }
        private string[] DiscoverDevices()
        {
            List<string> ipAddresses = new List<string>();
            using (UdpClient udpClient = new UdpClient())
            {
                byte[] discoveryData = Encoding.ASCII.GetBytes("DISCOVER");
                udpClient.Send(discoveryData, discoveryData.Length, new IPEndPoint(IPAddress.Broadcast, Port));
                udpClient.Client.ReceiveTimeout = 3000; 
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, Port);
                while (true)
                {
                    try
                    {
                        byte[] data = udpClient.Receive(ref remoteEP);
                        string response = Encoding.ASCII.GetString(data);
                        ipAddresses.Add(remoteEP.Address.ToString());
                    }
                    catch (SocketException ex)
                    {
                        break;
                    }
                }
            }

            return ipAddresses.ToArray();
        }
        private string[] GetLocalIPAddresses()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            List<string> ipAddresses = new List<string>();
            foreach (IPAddress ipAddress in host.AddressList)
            {
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    ipAddresses.Add(ipAddress.ToString());
                }
            }
            return ipAddresses.ToArray();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            udpClient.Close();
            // receiveThread.Abort();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}