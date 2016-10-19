using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ConfigServer
{

    class Connection
    {
        private Socket listenSocket;
        private int port;

        private int backlog = 10;
        public bool listening;

        Config conf;
        List<IPAddress> list;

        public Connection(int port)
        {
           this.port = port;
            listening = true;

            conf = new Config();
            list = null;

            Start();
        }

        public void Stop()
        {
            listening = false;
        }

        private void Start()
        {
            Console.WriteLine("#############################################################");
            Console.WriteLine("#####################=================#######################");
            Console.WriteLine("#####################  Config Server  #######################");
            Console.WriteLine("#####################=================#######################");
            Console.WriteLine("#############################################################");

            SetListenSocket(port);

            Console.WriteLine("Listening on {0} port\n", port);


            while (listening)
            {
                Console.WriteLine("===> Waiting for Match Server's connection....");
                Socket mServer = Accept();

                Console.WriteLine("===> New Match Server({0}) is connected....", mServer.RemoteEndPoint);

                if (mServer != null)
                {
                    if(0 != conf.GetCount())
                    {
                       list  = conf.GetAddressList();
                    }

                    //recv
                    //send

                    conf.InsertMS(mServer);
                }
            }
        }

        private void SetListenSocket(int port)
        {
            IPEndPoint localEP = new IPEndPoint(IPAddress.Any, port);

            listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(localEP);
            listenSocket.Listen(backlog);
        }

        private Socket Accept()
        {
            Socket matchS = null;
            try
            {
                matchS = listenSocket.Accept();
                Console.WriteLine("[Server][Accept]  MatchServer({0}) is Connected.", matchS.RemoteEndPoint.ToString()); 
            }
            catch (SocketException)
            {
                Console.WriteLine("[Server][Accept]  Fail.");
            }
            catch (Exception)
            {
                Console.WriteLine("[Server][Accept]  Fail.");
            }
            return matchS;
        }

    }
}
