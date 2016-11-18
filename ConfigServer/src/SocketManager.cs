
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
                Console.WriteLine("Listening on {0} port\n", port);
            }
            catch (SocketException se)
            {
                Console.WriteLine(" {0}", se.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(" {0}", e.ToString());
            }

        }

        public bool Accept()
        {
            try
            {
                Task<Socket> receiveTask = null;
                Console.WriteLine("===> Waiting for Match Server's connection....");
                Socket sock = listen.Accept();
                Console.WriteLine("===> New Match Server({0}) is connected....", sock.RemoteEndPoint);
                heartBeatList.Add(sock, 0);
                process.ProcessAccept(sock, serverType);
                Receive(sock, receiveTask);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("[Server][Accept]  error, code : {0}", e.ToString());
                return false;
            }
        }

        private async void Receive(Socket socket, Task receiveTask)
        {
            while (socket != null && socket.Connected)
            {
                try
                {
                    if (receiveTask == null || receiveTask.IsCompleted)
                    {
                        byte[] packet = await Task.Run<byte[]>(() => ReceiveAsync(socket));
                        if(packet != null)
                            process.Process(socket, packet);
                    }
                }
                catch (Exception)
                {
                    if (socket != null)
                    {
                        Close(socket);
                        socket = null;
                    }
                }
            }
        }

        private byte[] ReceiveAsync(Socket socket)
        {
            try
            {
                Console.WriteLine("\n===> Receiving from Match Server({0})....", socket.RemoteEndPoint);
                byte[] packet = new byte[PACKET_SIZE];

                socket.ReceiveTimeout = 3000;
                int readBytes = socket.Receive(packet);

                if (readBytes == 0)
                {
                    Close(socket);
                    return null;
                }

                heartBeatList[socket] = 0;

                return packet;
            }
            catch (SocketException se)
            {
                if (socket.Connected)
                {
                    if (se.ErrorCode == 10060)
                    {
                        if (++heartBeatList[socket] > 3)
                            throw;
                        else
                        {
                            process.SendHeartBeat(socket);
                            return null;
                        } 
                    }
                }
                Console.WriteLine("[Server][Receive] {0}", se.ToString());
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine("[Server][Receive] {0}", e.ToString());
                throw;
            }
        }


        private void Close(Socket s)
        {
            Console.WriteLine("===> close socket {0}\n", s.RemoteEndPoint.ToString());
            process.close(s);
            heartBeatList.Remove(s);
            s.Close();
            
        }

        

    }
}
