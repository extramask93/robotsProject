using System;
using System.Windows;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows.Threading;


namespace projekt_roboty
{
     public class AvaibleRobots
    {
        public  List<Robot> robotList;
        enum State { idc, waitingForIDs, waitingForLoc };
        private delegate void Handler(string str);
        public event EventHandler RobotListUpdated;
        TCPConnection connection;
        State currentState;
        
        public AvaibleRobots(TCPConnection connection)
        {
            this.connection = connection;
            currentState = State.idc;
            connection.MessageReceived += new EventHandler(queue_OnNewelementInQueue);
            robotList = new List<Robot>(10);
             for (int i = 0; i < 10; i++)
            {
                robotList.Add(new Robot(i+1));
            }
        }

        private void queue_OnNewelementInQueue(object sender, EventArgs e)
        {
                Decode(connection.ReadIncomeQueue());
        }
        
        private void OnRobotListUpdated(EventArgs e)
        {
            RobotListUpdated?.Invoke(this, e);
        }
        public int SetMotorsOffline(int id, float leftMotor, float rightMotor)
        {
            if(id==-1)
            {
                foreach(Robot robot in robotList)
                {
                    if (robot.controlable == 1)
                    {
                        robot.leftEngine = leftMotor;
                        robot.rightEngine = rightMotor;
                        OnRobotListUpdated(new EventArgs());
                        return 0;
                    }
                }
            }
            if (id > 0 && id <= 10)
            {
                if (robotList[id].controlable == 0)
                    return -1;
                robotList[id].leftEngine = leftMotor;
                robotList[id].rightEngine = rightMotor;
                OnRobotListUpdated(new EventArgs());
                return 0;
            }
            else
                return -1;
        }
        public int SetMotorsOnline(int id, float leftMotor, float rightMotor) //TODO check it
        {
            if (SetMotorsOffline(id, leftMotor, rightMotor) == 0)
            {
                List<byte> command = new List<byte>();
                command.Add(5);
                foreach (Robot robot in robotList)
                {
                    if (robot.controlable == 1)
                    {
                        command.AddRange(robot.encodeData());
                    }
                }
                byte[] commandb = command.ToArray();
                //connection.outQueue.Clear();
                connection.Send(commandb);
                return 0;
            }
            else return -1;
        }
        private void UpdateFromString(byte[] btcp)
        {
            //

            if (btcp[0] == 4)
            {
                for (int i = 0, j = 0; i < robotList.Count; i++, j += 14)
                {
                    ArraySegment<byte> segm = new ArraySegment<byte>(btcp, 1 + j, 14);
                    robotList[i].decodeData(segm);

                }
            }
            else {
                for (int i = 0, j = 0; i < robotList.Count; i++, j += 14)
                {
                    ArraySegment<byte> segm = new ArraySegment<byte>(btcp, 0 + j, 14);
                    robotList[i].decodeData(segm);

                }
            }
            OnRobotListUpdated(new EventArgs());
        }
        private void SetControlableRobotsIDs(byte[] btcp)
        {
            int i = 1;
            if (btcp[0] == 2)
                i = 2;
            for(;i<btcp.Length;i++)
            {
                robotList[btcp[i]-1].controlable=1;
            }
            OnRobotListUpdated(new EventArgs());
        }
        public void Decode(byte[] data)
        {    
            {
                if (data.Length == 1 && currentState == State.idc)
                {
                    switch (data[0])
                    {
                        case 2: currentState = State.waitingForIDs; break;
                        case 4: currentState = State.waitingForLoc; break;
                        case 6: currentState = State.idc; break;
                        default: currentState = State.idc; throw (new Exception("Nierozpoznane ID w pakiecie!"));
                    }
                }
                else
                {
                    switch (currentState)
                    {
                        case State.waitingForIDs: SetControlableRobotsIDs(data); currentState = State.idc; break;
                        case State.waitingForLoc: UpdateFromString(data); currentState = State.idc; break;
                        case State.idc:
                            {
                                if (data[0] == 4)
                                    UpdateFromString(data);
                                else if (data[0] == 2)
                                    SetControlableRobotsIDs(data);
                                else
                                    throw (new Exception("Nierozpoznano pakietu z danymi debug"));
                                currentState = State.idc;
                                break;
                            }
                        default:
                            {
                                currentState = State.idc; throw (new Exception("Nierozpoznano pakietu z danymi"));
                            }
                    }
                }

            }
        }
    }
 
}
