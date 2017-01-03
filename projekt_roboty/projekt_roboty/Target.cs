using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace projekt_roboty
{
    class Target
    {
        public double X { set; get; }
        public double Y { set; get; }
        public double Xb; //TODO: set methods updating angle
        public double Yb;
        public double angle { set; get; }
        double angleB;
        Target(Point a)
        {
            X = a.X;
            Y = a.Y;
            Xb = -a.Y + 200;
            Yb = -a.X + 200;
            angleB = SetAngleB();
        }
        private double SetAngleB()
        {
            double radians = Math.Atan2(Y, X);
            double angle = radians * (180 / Math.PI);
            return angle;
        }
    }
}
