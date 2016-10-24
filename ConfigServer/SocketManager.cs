using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ConfigServer
{

    class SocketManager
    {
        private Socket listenSocket;
        private int port;

        private int backlog = 10;
        public bool listening;

        Config conf;
        List<IPAddress> list;

        public SocketManager(int port)
        {
            this.port = port;
            listening = true;

            conf = new Config();
            list = null;
        }

        public void Stop()
        {
            listening = false;
        }

        public void Start()
        {
            Console.WriteLine("#############################################################");
            Console.WriteLine("#####################=================#######################");
            Console.WriteLine("#####################  Config Server  #######################");
            Console.WriteLine("#####################=================#######################");
            Console.WriteLine("#############################################################");

            SetListenSocket(port);

            while (listening)
            {
                Accept();
            }
        }

        private void SetListenSocket(int port)
        {
            IPEndPoint localEP = new IPEndPoint(IPAddress.Any, port);

            listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(localEP);
            listenSocket.Listen(backlog);
            Console.WriteLine("Listening on {0} port\n", port);

            Console.WriteLine("===> Waiting for Match Server's connection....");
        }

        private void Accept()
        {
            try
            {
                Task<Socket> receiveTask = null;
                
                Socket mServer = listenSocket.Accept();
                Console.WriteLine("===> New Match Server({0}) is connected....", mServer.RemoteEndPoint);

                List<IPAddress> list = conf.GetAddressList();
                conf.InsertMS(mServer);
                
                Receive(mServer, receiveTask, list);   
            }
            catch (Exception e)
            {
                Console.WriteLine("[Server][Accept]  error, code : {0}", e.ToString());
            }
        }

        private async void Receive(Socket socket, Task receiveTask, List<IPAddress> list)
        {
            while (socket != null && socket.Connected)
            {
                try
                {
                    if (receiveTask == null || receiveTask.IsCompleted)
                    { 
                        // returnType xxx = await Task.Run<returnType>(() => ReceiveAsync(socket));
                    }
                }
                catch(SocketException)
                {

                }
            }
        }

      /*  
         private returnType ReceiveAsync(Socket socket)
        {
            byte[] buffer = new byte[HEAD_SIZE];

            int readBytes = socket.Receive(buffer);
        }
        */
    }
}
