using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace projekt_roboty
{
    /// <summary>
    /// Logika interakcji dla klasy App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static MainWindow mw;
        public  Window1 w1;
        public TCPConnection connection;//shared between windows
        public AvaibleRobots robotList;// shared between windows
    private void Application_Startup(object sender, StartupEventArgs e)
        {
            mw = new MainWindow();
            w1 = new Window1();
            mw.Show();
        }


    }
}
