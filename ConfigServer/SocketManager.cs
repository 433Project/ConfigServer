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
        int HEAD_SIZE = 20;

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
                        bool result = await Task.Run<bool>(() => ReceiveAsync(socket, list));
                       // if (result)
                         //   break;
                    }
                }
                catch (SocketException)
                {

                }
            }
        }


        private bool ReceiveAsync(Socket socket, List<IPAddress> list)
        {
            byte[] buffer = new byte[HEAD_SIZE];
            Console.WriteLine("\n===> Receiving from Match Server({0})....", socket.RemoteEndPoint);
            int readBytes = socket.Receive(buffer);
            
            Console.WriteLine("===> Match Server({0}) sent {1}bytes message: {2}....", socket.RemoteEndPoint, readBytes, Encoding.UTF8.GetString(buffer).Split('\0')[0]);


            //if(header의 Command가 request이면)
                byte[] buf = list.SelectMany(x => x.GetAddressBytes()).ToArray();
                socket.Send(buf);
                Console.WriteLine("===> send message to Match Server({0})....", socket.RemoteEndPoint);
                return true;

            //else 
                //return false
        }
    }
}
