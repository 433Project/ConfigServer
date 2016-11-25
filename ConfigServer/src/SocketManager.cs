
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;


namespace ConfigServer
{
    class SocketManager
    {
        private string serverType;
        private Socket listen;
        private int port;
        private int backlog = 10;

        int PACKET_SIZE = 100;

        private ILog logger = Logger.GetLoggerInstance();
        Dictionary<Socket, int> heartBeatList;
        ProcessManager process;

        public SocketManager(string serverType)
        {
            this.serverType = serverType;
            setSocket(this.serverType);

            process = new ProcessManager();
            heartBeatList = new Dictionary<Socket, int>();
        }

        private void setSocket(string serverType)
        {
            if (int.TryParse(ConfigurationManager.AppSettings.Get(serverType), out this.port))
                SetListenSocket(this.port);
        }

        private void SetListenSocket(int port)
        {
            try
            {
                IPEndPoint localEP = new IPEndPoint(IPAddress.Any, port);

                listen = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listen.Bind(localEP);
                listen.Listen(backlog);
                logger.Info("Listening Port : " + port);
            }
            catch (SocketException se)
            {
                logger.Error("SocketException : " + se.ToString());
            }
            catch (Exception e)
            {
                logger.Error("Exception : "+ e.ToString());
            }

        }

        public bool Accept()
        {
            try
            {
                //logger.Info("===> Waiting for Match Server's connection....");
                Socket sock = listen.Accept();
                logger.Info("===> New Match Server is connected : "+ sock.RemoteEndPoint);
                heartBeatList.Add(sock, 0);
                process.ProcessAccept(sock, serverType);
                Receive(sock);
                return true;
            }
            catch (Exception e)
            {
                logger.Error("[Server][Accept]  error, code : "+ e.ToString());
                return false;
            }
        }

        private async void Receive(Socket socket)
        {
            Task<bool> receiveTask = null;
            while (socket != null && socket.Connected)
            {
                try
                {
                    if (receiveTask == null || receiveTask.IsCompleted)
                    {
                        receiveTask = ReceiveAsync(socket);
                        bool result  = await receiveTask;
                    }
                }
                catch (Exception e )
                {
                    Console.WriteLine(e.ToString());
                    if (socket != null)
                    {
                        Close(socket);
                    }
                }
            }
        }

        private Task<bool> ReceiveAsync(Socket socket)
        {
            Task<bool>  task = Task.Run(() =>
            {
                try
                {
                    //Console.WriteLine("\n===> Receiving from Match Server({0})....", socket.RemoteEndPoint);
                    byte[] bytes = new byte[PACKET_SIZE];
                    socket.ReceiveTimeout = 3 * 1000;
                    int readBytes = socket.Receive(bytes);

                    if (readBytes == 0)
                    {
                        Close(socket);
                        return false;
                    }

                    process.Process(socket, bytes);
                    heartBeatList[socket] = 0;

                    return true;
                }
                catch (SocketException se)
                {
                    if (socket.Connected)
                    {
                        //receive timeout
                        if (se.ErrorCode == 10060)
                        {
                            if (++heartBeatList[socket] <= 3)
                            {
                                process.SendHeartBeat(socket);
                                return true;
                            }
                            Close(socket);
                            return false;
                        }
                    }
                    logger.Error("===>recieve socket error : " + se.ToString());
                    Close(socket);
                    return false; 
                }
                catch (Exception e)
                {
                    logger.Error("===>recieve socket error : " + e.ToString());
                    Close(socket);
                    return false;
                }
            });
            return task;
            
        }


        private void Close(Socket s)
        {
            logger.Info("===> close socket : "+ s.RemoteEndPoint.ToString());
            process.close(s);
            heartBeatList.Remove(s);
            s.Close();
        }

        

    }
}
