using System;
using System.Drawing;
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
using System.Windows.Shapes;

namespace projekt_roboty
{
    /// <summary>
    /// Logika interakcji dla klasy Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
            
        }

        private void canvas1_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Point p=Mouse.GetPosition(canvas1);
            Point pt = new Point(); //contains coords in imagined system to calculate angle
            pt.X = -p.Y + 200;
            pt.Y = -p.X + 200;
            double radians=Math.Atan2(pt.Y, pt.X);
            double angle = radians * (180/Math.PI);
            textBox1.Text = p.ToString() + "||"+ pt.ToString()+"||"+angle.ToString();
        }
        private void PID(ref Robot robot, Target target)
        {
            /*
            const double KP = 0.6;// Kp = (127 - Vr)/maxerror
            const double KI = 1/10000;
            const double KD =3/2;
            double error=0;
            double proportional, prevProportional, derivative, integral, total;
            sbyte Vr = 0; //docelowa predkosc robota
            sbyte omegaMax = 20;
            while(1)
            {
                refreshStates();
                error = Robot.angle - target.angle;
                proportional = error; //watch! Vr+omega<127
                integral = intergral+error;
                derivative = proportional - prevProportional;
                prevProportional = proportional;
                total = (KP*proportional + KI*integral + KD*derivative);

                if(total>omegaMax) //limit angular speed
                    total=omegaMax;
                if(total<-omegaMax)
                    total= -omegaMax;

                total = (sbyte)Math.Round(total);

                if(total<0)
                    robot.leftEngine = Vr + omega;
                    robot.rightEngine = Vr - omega;
                if(total>0)
                    robot.leftEngine = Vr - omega;
                    robot.rightEngine = Vr + omega;
                if(error<=5)//jezeli błąd mniejszy niz 5 stopni to zacznij jechac
                    Vr=20;
                else
                    Vr=0;
                if(close enaugh to target)
                    Vr=0;
                    break;
            }

            */
        }
    }
}
