using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace projekt_roboty
{
    public class TCPConnection
    {
        string server;
        Int32 port;
        ConcurrentQueue<string> outQueue;
        public ConcurrentQueue<string> inQueue;
        TcpClient client;
        NetworkStream nwStream;
        Thread writeThread;
        Thread readThread;
        bool _quit;
        //event1
        public event EventHandler UIMessage;
        public void OnUIMessage(TCPConnectionEventArgs e)
        {
            EventHandler uiMessage = UIMessage;
            if (uiMessage != null)
            {
                uiMessage(this, e);
            }
        }
        //event2
        public event EventHandler MessageReceived;
        public void OnMessageReceived(TCPConnectionEventArgs e)
        {
            
            EventHandler messageReceived = MessageReceived;
            if(messageReceived != null)
            {
                messageReceived(this, e);
            }
        }
        //
        public TCPConnection()
        {
            inQueue = new ConcurrentQueue<string>(30);
            outQueue = new ConcurrentQueue<string>(30);
            _quit = false;

        }
        public void Send(string toSend)
        {
            outQueue.Add(toSend);
        }
        public void Close()
        {
            _quit = true;
            
        }

        public void Connect(string server, Int32 port)
        {
            try
            {
                this.server = server;
                this.port = port;
                _quit = false;
                OnUIMessage(new TCPConnectionEventArgs("Connecting..."));
                client = new TcpClient(server, port);
                nwStream = client.GetStream();
                OnUIMessage(new TCPConnectionEventArgs("Połączono z " + server + "port: " + port.ToString() + "    "));
                writeThread = new Thread(() => Write());
                writeThread.Start();
                readThread = new Thread(() => Read());
                readThread.Start();
            }
            catch (Exception e)
            {
                OnUIMessage(new TCPConnectionEventArgs("Błąd w wątku TCP" + e.ToString()));
            }
        }
        public void Read()
        {
            while (true && _quit != true)
            {
                int i = 0;
                int count = 0;
                int N = 1;
                byte[] bytesToRead = new byte[client.ReceiveBufferSize];
                //int id = nwStream.Read(bytesToRead, 0, 1);
                //switch (bytesToRead[0])
                //{
                //    case 0:
                //    case 6:
                //    case 17: count = 0; break;
                //    case 49:
                //    case 33: count = 1 * N; break;
                //    case 4: count = 14 * N; break;
                //    default: break;
                //}
                while ((i = nwStream.Read(bytesToRead, 0, client.ReceiveBufferSize-1)) != 0)
                {
                    // Translate data bytes to a ASCII string.
                    string data = System.Text.Encoding.ASCII.GetString(bytesToRead, 0, i);
                    //throw 
                    //byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);
                    string temp="";
                    for (int j = 0; j < i; j++)
                        temp += bytesToRead[j].ToString()+"|";
                    OnUIMessage(new TCPConnectionEventArgs("Receieved: " + temp));
                    inQueue.Add(data);
                }
            }
        }
        public void Write()
        {
            try
            {
                while (true && _quit!=true)
                {
                    if (outQueue._queue.Count() > 0)
                    {
                        string toSend = outQueue.Remove();
                        byte[] bytesToSend = new byte[toSend.Length];
                        bytesToSend=Encoding.ASCII.GetBytes(toSend);
                        toSend = "";
                        foreach (byte b in bytesToSend)
                            toSend += b.ToString();
                        OnUIMessage(new TCPConnectionEventArgs("Sending: "+toSend));
                        nwStream.Write(bytesToSend, 0, bytesToSend.Length);
                    }
                    Thread.Sleep(200);
                }
                
            }
            catch (Exception e)
            {
                OnUIMessage(new TCPConnectionEventArgs("Błąd w wątku TCP"+e.ToString()));
            }
            finally
            {
                Close();
                if(client!=null) client.Close();
            }
        }
    }
}
