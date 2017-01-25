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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace projekt_roboty
{
    /// <summary>
    /// Logika interakcji dla klasy Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        BitmapImage controlable, detected;
        private delegate void Handler(AvaibleRobots a);
        AvaibleRobots robotList;
        List<Robot> controlableRobots;
        int id;
        Action emptyDelegate = delegate { };
        const sbyte MotorMax = 127;
        const sbyte tick = 1;
        sbyte[] Times;
        sbyte MaxMultiSteer = (sbyte)Math.Round(MotorMax / (float)(80));//maximum nr. of steps for steering
        sbyte MotorMulti = (sbyte)Math.Round(MotorMax / (float)30);//#multiplier per step of acceleration/deceleration
        sbyte MotorMultiSteer = (sbyte)Math.Round(MotorMax / (float)12);//maximum nr. of steps for acc/dec
        sbyte MaxMulti;
        sbyte Motor1Speed;
        sbyte Motor2Speed;
        sbyte SteerSpeed;
        float MotorPercentage;
        DispatcherTimer keyboardTimer1;
        public Window1(AvaibleRobots robotList)
        {
            Times = new sbyte[9];
            MaxMulti = (sbyte)Math.Floor(MotorMax / (float)MotorMulti);//#calculate the maximum nr. of steps for acc/dec =30
            keyboardTimer1 = new DispatcherTimer();
            keyboardTimer1.Tick += new EventHandler(keyboardTimer_OnTick);
            keyboardTimer1.Interval = new TimeSpan(0, 0, 0, 0, 100); //every 0,1s
            keyboardTimer1.Start();
            //
            this.robotList = robotList;
            InitializeComponent();
            controlable = new BitmapImage(new Uri("robot_controlable.png", UriKind.Relative));
            detected = new BitmapImage(new Uri("robot_detected.png", UriKind.Relative));
            robotList.RobotListUpdated += new EventHandler(robotList_OnRobotListUpdated);
            DrawRobots(robotList);
        }
       

        //private void canvas1_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        //{
        //    Point p=Mouse.GetPosition(canvas1);
        //    Point pt = new Point(); //contains coords in imagined system to calculate angle
        //    pt.X = -p.Y + 400;
        //    pt.Y = -p.X + 400;
        //    double radians=Math.Atan2(pt.Y, pt.X);
        //    double angle = radians * (180/Math.PI);
        //    angle = -90;
        //    textBox1.Text = p.ToString() + "||"+ pt.ToString()+"||"+angle.ToString();
        //    //DrawElipise(p);
        //    DrawElipse(p,angle,detected);

        //}
        private void robotList_OnRobotListUpdated(object sender, EventArgs e)
        {
            AvaibleRobots a = sender as AvaibleRobots;
            Dispatcher.BeginInvoke(new Handler(DrawRobots),a);
        }
        private void DrawElipse(Point p,double angle,BitmapImage bp)
        {
            Ellipse el = new Ellipse();
            el.Width = 40;
            el.Height = 40;
            ImageBrush brush = new ImageBrush(bp);
            el.Fill=(brush);
            RotateTransform rt = new RotateTransform(180-angle,20,20);
            el.RenderTransform = rt;
            Canvas.SetLeft(el,p.X-(el.Width/2));
            Canvas.SetTop(el,p.Y-(el.Height/2));
            canvas1.Children.Add(el);
            canvas1.Dispatcher.Invoke(emptyDelegate, DispatcherPriority.Render);

        }

        private void DrawRobots(AvaibleRobots a)
        {
            canvas1.Children.Clear();
            foreach (Robot robot in a.robotList)
            {
                if (robot.detected == 1)
                {
                    Point p = ToVisCoord(new Point(robot.Xmm,robot.Ymm));
                    if (robot.controlable == 1)
                    {
                        DrawElipse(p, robot.angle, controlable);
                        id = robot.id;
                    }
                    else
                        DrawElipse(p, robot.angle, detected);
                }
            }

        }
        private Point ToVisCoord(Point realCoord)
        {
            Point temp = new Point(0,0);
            temp.X = Math.Round(realCoord.Y*0.4);
            temp.Y = Math.Round(realCoord.X*0.4);
            return temp;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            canvas1.Children.Clear();
        }

        //////////////sterowanie
        private void Window_KeyDown_1(object sender, KeyEventArgs e)
        {
            //TODO
            if (e.Key == Key.Up)
                Times[4] = 1;
            else if (e.Key == Key.Down)
                Times[7] = 1;
            else if (e.Key == Key.Left)
                Times[5] = 1;
            else if (e.Key == Key.Right)
                Times[6] = 1;
            else if (e.Key == Key.Space) //breaks
                Times[8] = 1;

        }

        private void Window_KeyUp_1(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Up)
                Times[4] = 0;
            else if (e.Key == Key.Down)
                Times[7] = 0;
            else if (e.Key == Key.Left)
                Times[5] = 0;
            else if (e.Key == Key.Right)
                Times[6] = 0;
            else if (e.Key == Key.Space)
                Times[8] = 0;
        }


        private void keyboardTimer_OnTick(object sender, EventArgs e)
        {
            if (Times[4] == 0 && Times[0] > 0)      //#if flag for <Up Arrow> is cleared,
                Times[0] -= 1;  						//#decrement counter for <Up Arrow> if not already zero	
            if (Times[5] == 0 && Times[1] > 0)
                Times[1] -= 1;
            if (Times[6] == 0 && Times[2] > 0)
                Times[2] -= 1;
            if (Times[7] == 0 && Times[3] > 0)
                Times[3] -= 1;

            if (Times[4] == 1 && Times[0] < MaxMulti)		//flag of <Up Arrow> is set -> counter increment
                Times[0] += 1;  						//if not already at maximum
            if (Times[5] == 1 && Times[1] < MaxMultiSteer)
                Times[1] += 1;
            if (Times[6] == 1 && Times[2] < MaxMultiSteer)
                Times[2] += 1;
            if (Times[7] == 1 && Times[3] < MaxMulti)
                Times[3] += 1;
            if (Times[8] == 1)
                Times[0] = Times[1] = Times[2] = Times[3] = 0;

            Motor1Speed = (sbyte)((Times[0] - Times[3]) * MotorMulti);//calculating the motor speed for linear movement
            MotorPercentage = Motor1Speed / MotorMax;//calc temp var its not needed?
            SteerSpeed = (sbyte)((Times[2] - Times[1]) * MotorMultiSteer);//calculating the current steering movement ye deleted motor percentage n works
            if (Motor1Speed + Math.Abs(SteerSpeed) >= MotorMax)//limit the accumulated motor speeds so that steering can take effect
                Motor1Speed = (sbyte)(MotorMax - Math.Abs(SteerSpeed));
            if (Motor1Speed - Math.Abs(SteerSpeed) <= -MotorMax)
                Motor1Speed = (sbyte)(-MotorMax + Math.Abs(SteerSpeed));
            Motor2Speed = Motor1Speed;//linear movement --> both motors running at the same speed
            Motor1Speed += SteerSpeed;//adding the steering speed to the both motors
            Motor2Speed += (sbyte)-SteerSpeed;
            robotList.SetMotorsOnline(id-1, Motor1Speed,Motor2Speed);
            labell.Content = Motor1Speed;
            labelr.Content = Motor2Speed;

        }
    }
}
