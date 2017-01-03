using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projekt_roboty
{
     public class AvaibleRobots
    {
        public  List<Robot> robotList;
        public AvaibleRobots() { robotList = new List<Robot>(); }
        public AvaibleRobots(string tcpString)
        {

            if (tcpString.Length < 4 || tcpString[0] != '[' || tcpString[tcpString.Length] != ']' || tcpString.Substring(1, 2) != "49") //check formating
                throw new Exception("Parsing string failed: "+tcpString);
            tcpString = tcpString.Substring(3, tcpString.Length-1);
            int numberOfRobots = tcpString.Length/2;
            robotList = new List<Robot>(numberOfRobots);
            int i = 0;
            foreach(Robot robot in robotList)
            {
                robot.id = int.Parse(tcpString.Substring(0+i, 1+i));
                i+=2;
            }
        }
        public void UpdateFromString(string tcpString)
        {
            //TODO move some functionality to Robot class(encode data)
            //check whether string begins with 4
            if (tcpString.Length < 4 || tcpString[0] != '[' || tcpString[tcpString.Length] != ']' || tcpString.Substring(1, 2) != "04") //check formating, there could be the only 4
                throw new Exception("Parsing string failed: " + tcpString);
            tcpString = tcpString.Substring(3, tcpString.Length - 1); //so there wil be2
            int numberOfRobots = tcpString.Length / 14;
            for(int i=0,j=0;i<numberOfRobots;i++,j+=28)
            {
                Robot tempRobot = new Robot();
                string singleRobotData = tcpString.Substring(0 +j, 27);
                tempRobot.id = int.Parse(singleRobotData.Substring(0,2));
                tempRobot.flag = int.Parse(singleRobotData.Substring(2, 2));
                //<Xmm>
                byte[] temp = new byte[4];
                for (int a = 0,b=0; a < 4; a++,b+=2)
                {
                    temp[a] = byte.Parse(singleRobotData.Substring(4+b, 2), System.Globalization.NumberStyles.HexNumber); 
                }
                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(temp);
                    tempRobot.Xmm = BitConverter.ToSingle(temp,0);
                }
                else
                    tempRobot.Xmm = BitConverter.ToSingle(temp, 0);
                //</Xmm>
                //<Ymm>
                for (int a = 0, b = 0; a < 4; a++, b += 2)
                {
                    temp[a] = byte.Parse(singleRobotData.Substring(12 + b, 2), System.Globalization.NumberStyles.HexNumber); 
                }
                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(temp);
                    tempRobot.Ymm = BitConverter.ToSingle(temp, 0);
                }
                else
                    tempRobot.Ymm = BitConverter.ToSingle(temp, 0);
                //</Ymm>
                //<angle>
                for (int a = 0, b = 0; a < 4; a++, b += 2)
                {
                    temp[a] = byte.Parse(singleRobotData.Substring(20 + b, 2), System.Globalization.NumberStyles.HexNumber); 
                }
                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(temp);
                    tempRobot.angle = BitConverter.ToSingle(temp, 0);
                }
                else
                    tempRobot.angle = BitConverter.ToSingle(temp, 0);
                //</angle>
                var foo = new RobotSearch(tempRobot.id);
                int position= robotList.FindIndex(foo.equals);
                robotList[position] = tempRobot;

            }
        }
        public DataTable GetDataTable()
        { 
            DataTable table = new DataTable();
            table.Columns.Add("ID");
            table.Columns.Add("Axis X[mm]");
            table.Columns.Add("Axis Y[mm]");
            table.Columns.Add("Angle [deg]");
            table.Columns.Add("LM");
            table.Columns.Add("RM");
            foreach(Robot robot in robotList)
            {
               table.Rows.Add(robot.id.ToString(), robot.Xmm.ToString(), robot.Ymm.ToString(), robot.angle.ToString(), robot.leftEngine.ToString(), robot.rightEngine.ToString());
            }
            return table;
        }
    }
 
}
