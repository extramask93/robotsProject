using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Data;
using System.Threading;

namespace projekt_roboty
{
    
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string IP; //TODO: wpisac dane servera 192.168.2.103", 50131
        public int PORT;
        TCPConnection connection;
        AvaibleRobots robotList;
        DispatcherTimer posReqTimer;
        private delegate void Handler(string str);
        private delegate void Handler2();
        enum Tryb:byte {sterowanie,monitorSterowanie, monitor};
        enum PacketType : byte {disconnect, connectionRequest, connectionResponse,positionRequest,positionResponse, moveRequest, moveResponse};
        Tryb mode;
        Window w1;

        public MainWindow()
        {
            InitializeComponent();
            posReqTimer = new DispatcherTimer();
            posReqTimer.Tick += new EventHandler(posReqTimer_OnTick);
            posReqTimer.Interval = new TimeSpan(0, 0, 0, 0, 100); //every 0,5s
            connection = new TCPConnection();
            mode = new Tryb();
            connection.UIMessage += new EventHandler(connection_OnUIMessage);
            robotList = new AvaibleRobots(connection);
            robotList.RobotListUpdated += new EventHandler(robotList_OnRobotListUpdated);
            robotListView.ItemsSource = robotList.robotList;      
        }

        private void posReqTimer_OnTick(object sender, EventArgs e)
        {
            if(connection.Connected())
                RequestPosition();
            //label.Content = connection.outQueue.Count();
        }
        private void robotList_OnRobotListUpdated(object sender, EventArgs e)
        {
           Dispatcher.BeginInvoke(new Handler2(robotListView.Items.Refresh));    
        }

        private void connection_OnUIMessage(object sender, EventArgs e)
        {
            if (e is TCPConnectionEventArgs)
            {
                TCPConnectionEventArgs foo = e as TCPConnectionEventArgs;
                Dispatcher.BeginInvoke(new Handler(SetText), foo.info);
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            w1 = new Window1(robotList);
            w1.Show();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)//initialize
        {
            InitializeConnection();
            posReqTimer.Start();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)//connect
        {
            Connect();
            conButton.IsEnabled = false;
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)//disconnect
        {
            Disconnect();
            conButton.IsEnabled = true;     
        }

     //<functions>
        private void Connect()
        {
            IP = textBoxIP.Text;
            PORT = int.Parse(textBoxPORT.Text);
            connection.Connect(IP, PORT);
        }
        private void Disconnect()
        {
            byte[] command = { (byte)PacketType.disconnect };
            connection.Send(command);
            posReqTimer.Stop();
            connection.Close();
        }
        private void RequestPosition()
        {
            byte[] command = { (byte)PacketType.positionRequest };
            connection.Send(command);
        }
        private void InitializeConnection()
        {
            byte nrOfControlableRobots = (byte)(robotNrComboBox.SelectedIndex + 1); //get requestet nr of robots
            mode = (Tryb)modeComboBox.SelectedIndex; //get selected mode
            if (mode == Tryb.monitor)
            {
                byte[] command = { (byte)PacketType.connectionRequest,(byte)mode, 0};
                connection.Send(command);
            }
            else
            {
                byte[] command = {(byte)PacketType.connectionRequest, (byte)mode, nrOfControlableRobots };
                connection.Send(command); // zadanie polaczenia, monitor/ster, 1 robot, server should respond like:"2|49|id"
            }
        }
        private void SetText(string str)
        {
            textBox1.Text += "->" + str + '\n';
            var prevFocus = FocusManager.GetFocusedElement(this);
            textBox1.Focus();
            textBox1.CaretIndex = textBox1.Text.Length;
            textBox1.ScrollToEnd();
            FocusManager.SetFocusedElement(this, prevFocus);
        }
    }
}
