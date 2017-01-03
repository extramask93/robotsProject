using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace projekt_roboty
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const string IP = "127.0.0.1"; //TODO: wpisac dane servera 192.168.2.103", 50131
        public const int PORT = 55056;
        public const byte robotNr = 1;
        private delegate void Handler(string str);
        enum Tryb:byte {sterowanie,monitorSterowanie, monitor};
        enum PacketType : byte {disconnect, connectionRequest, connectionResponse,positionRequest,positionResponse, moveRequest, moveResponse};
        public MainWindow()
        {
            InitializeComponent();
            ((App)Application.Current).connection = new TCPConnection();
            ((App)Application.Current).connection.MessageReceived += new EventHandler(connection_OnMessageReceived);
            ((App)Application.Current).connection.UIMessage += new EventHandler(connection_OnUIMessage);
            ((App)Application.Current).robotList = new AvaibleRobots();
            dataGrid1.ItemsSource= ((App)Application.Current).robotList.GetDataTable().DefaultView;
       
            //textBox1.Text = "asdad";
        }

   
        private void connection_OnUIMessage(object sender, EventArgs e)
        {
            if (e is TCPConnectionEventArgs)
            {
                TCPConnectionEventArgs foo = e as TCPConnectionEventArgs;
                Dispatcher.BeginInvoke(new Handler(SetText), foo.info);
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
 
        private void connection_OnMessageReceived(object sender, EventArgs e)
        {
            //SetText(((App)Application.Current).connection.inQueue.Remove());
            if (e is TCPConnectionEventArgs)
            {
                TCPConnectionEventArgs foo = e as TCPConnectionEventArgs;
                Dispatcher.BeginInvoke(new Handler(SetText), ((App)Application.Current).connection.inQueue.Remove());
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            ((App)Application.Current).w1.Show();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {                                                                                                        //   tryb    dunno    id   ava_flag    
            byte[] command = { (byte)Tryb.monitorSterowanie, (byte)PacketType.connectionRequest, robotNr };
            ((App)Application.Current).connection.Send(Encoding.ASCII.GetString(command)); // zadanie polaczenia, monitor/ster, 1 robot, server should respond like:"2|49|id"
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {

            byte[] command = { (byte)PacketType.positionRequest};
            ((App)Application.Current).connection.Send(Encoding.ASCII.GetString(command));
        } 

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ((App)Application.Current).connection.Connect(IP, PORT);
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            byte[] command = { (byte)PacketType.disconnect };
            ((App)Application.Current).connection.Send(Encoding.ASCII.GetString(command));
        }
    }
}
