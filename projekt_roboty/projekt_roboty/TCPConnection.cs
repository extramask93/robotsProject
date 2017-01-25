using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace projekt_roboty
{
    public class TCPConnection
    { 
        string server;
        Int32 port;
        public ConcurrentQueue<byte[]> outQueue;
        public ConcurrentQueue<byte[]> inQueue;
        EventWaitHandle sendHandle;
        EventWaitHandle exitHandle;
        WaitHandle[] handles;
        TcpClient client;
        NetworkStream nwStream;
        Thread writeThread;
        Thread readThread;
        bool _quit;
        int flag;
        //event1
        public event EventHandler UIMessage;
        public void OnUIMessage(TCPConnectionEventArgs e)
        {
            UIMessage?.Invoke(this, e);
        }
        //event2
        public event EventHandler MessageReceived;
        public void OnMessageReceived(TCPConnectionEventArgs e)
        {
            MessageReceived?.Invoke(this, e);
        }
        public TCPConnection()
        {
            sendHandle = new AutoResetEvent(false);
            inQueue = new ConcurrentQueue<byte[]>(30,new AutoResetEvent(false)); 
            outQueue =new ConcurrentQueue<byte[]>(30,sendHandle);
            exitHandle = new AutoResetEvent(false);
            _quit = false;
            handles = new WaitHandle[] {sendHandle, exitHandle};

        }
        public void Send(byte[] toSend)
        {
            outQueue.Add(toSend);
        }
        public byte[] ReadIncomeQueue()
        {
            if (inQueue.Count() > 0)
                return inQueue.Remove();
            else 
            return new byte[] {30}; 
        }
        public bool Connected()
        {
            if (client != null)
                return client.Connected;
            else
                return false;
        }
        public void Close()
        {
            exitHandle.Set();
            inQueue.Clear();
            outQueue.Clear();
            _quit = true;
            if (readThread != null && Thread.CurrentThread.Name!="WriteThread")
                readThread.Join();
            if (client != null)
                client.Close();
            if(Thread.CurrentThread.Name!="WriteThread")
            OnUIMessage(new TCPConnectionEventArgs("Disconnected"));
        }

        public void Connect(string server, Int32 port)
        {
            try
            {
                this.server = server;
                this.port = port;
                flag = 0;
                _quit = false;
                outQueue.Clear();
                inQueue.Clear();
                OnUIMessage(new TCPConnectionEventArgs("Connecting..."));
                client = new TcpClient(server, port);
                nwStream = client.GetStream();
                OnUIMessage(new TCPConnectionEventArgs("Connected to " + server + " at port: " + port.ToString()));
                try
                {
                    writeThread = new Thread(() => Write());
                    writeThread.IsBackground = true;
                    writeThread.Start();
                }
                catch(Exception)
                {
                    OnUIMessage(new TCPConnectionEventArgs("Cannot create TCP threads"));
                    client.Close();
                }
            }
            catch (SocketException e)
            {
                OnUIMessage(new TCPConnectionEventArgs("Server denied the connection, error code: " + e.ErrorCode.ToString()));
            }
            catch (Exception)
            {
                OnUIMessage(new TCPConnectionEventArgs("Wrong ip/port format"));
            }
        }
        public void Read()
        {
            try
            {
                nwStream.ReadTimeout = 1000;
                while (!_quit)
                {
                    int i = 0;
                    byte[] bytesToRead = new byte[client.ReceiveBufferSize];
                    string temp;
                    try
                    {
                        while ((i = nwStream.Read(bytesToRead, 0, client.ReceiveBufferSize - 1)) != 0 && !_quit)
                        {
                            temp = "";
                            for (int j = 0; j < i; j++)
                                temp += bytesToRead[j].ToString() + "|";
                            if (flag == 0)
                            {
                                OnUIMessage(new TCPConnectionEventArgs("Receieved: " + temp));
                                flag = 1;
                            }
                            Array.Resize(ref bytesToRead, i);
                            inQueue.Add(bytesToRead);
                            Array.Resize(ref bytesToRead, client.ReceiveBufferSize);
                            OnMessageReceived(new TCPConnectionEventArgs(""));
                        }
                    }
                    catch(IOException)//just readtimeout nothing to worry about
                    { }
                }
            }
            catch(Exception e)
            {
                OnUIMessage(new TCPConnectionEventArgs(e.Message));
                _quit = true;
                exitHandle.Set();
            }

        }
        public void Write()
        {
            try
            {
                if (Thread.CurrentThread.Name == null)
                {
                    Thread.CurrentThread.Name = "WriteThread";
                }
                readThread = new Thread(() => Read());
                readThread.IsBackground = true;
                readThread.Start();
                while (_quit!=true)
                {
                    sendHandle.WaitOne();
                    if (outQueue._queue.Count() > 0)
                    {
                        string toSendS="";
                        byte[] toSend = outQueue.Remove();
                        foreach (byte b in toSend)
                            toSendS += b.ToString();
                        //OnUIMessage(new TCPConnectionEventArgs("Sending: "+toSendS));
                        nwStream.WriteTimeout=1000;
                        nwStream.Write(toSend, 0, toSend.Length);
                        if (toSend.Length == 1 && toSend[0] == 0)
                            return;
                    }
                }
                
            }
            catch(SocketException e)
            {
                OnUIMessage(new TCPConnectionEventArgs("Cannot Write to the socket, error code: " + e.Message));
            }
            catch (Exception)
            {
                OnUIMessage(new TCPConnectionEventArgs("Unknown error occured during socket writing"));
            }
            finally
            {
                _quit = true;
                readThread.Join();
                Close();
            }
        }
    }
}
