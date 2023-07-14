using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
namespace LANCom
{
    public partial class Form1 : Form

    {
        private UdpClient udpClient;
        private Thread receiveThread;
        private const int Port = 8888;
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
                // Append the received message to the TextBox or ListBox control
                textBox4.AppendText(message + Environment.NewLine);
            }
        }
        private void send_Click(object sender, EventArgs e)
        {
            string message = textBox1.Text;
            byte[] data = Encoding.ASCII.GetBytes(message);
            udpClient.Send(data, data.Length, new IPEndPoint(IPAddress.Broadcast, Port));
            textBox1.Clear();
        }
        private void Form1_Load(object sender, EventArgs e)
        {

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
    }
}