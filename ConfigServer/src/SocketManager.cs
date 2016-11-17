using fb;
using FlatBuffers;
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
        
        int PACKET_SIZE = 100;
        
        Config conf;
        MessageManager mm;

        Dictionary<Socket, int> list;
        Dictionary<Socket, int> heartBeatList;

        public SocketManager(int port)
        {
            this.port = port;
            listening = true;

            conf = new Config();
            list = null;

            mm = new MessageManager();
            heartBeatList = new Dictionary<Socket, int>();
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
            try
            {
                IPEndPoint localEP = new IPEndPoint(IPAddress.Any, port);

                listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listenSocket.Bind(localEP);
                listenSocket.Listen(backlog);
                Console.WriteLine("Listening on {0} port\n", port);

                Console.WriteLine("===> Waiting for Match Server's connection....");
            }
            catch (SocketException se)
            {
                Console.WriteLine(" {0}", se.ToString());
            }
            catch(Exception e)
            {
                Console.WriteLine(" {0}", e.ToString());
            }
            
        }

        private void Accept()
        {
            try
            {
                Task<Socket> receiveTask = null;

                Socket ms = listenSocket.Accept();
                Console.WriteLine("===> New Match Server({0}) is connected....", ms.RemoteEndPoint);
                heartBeatList.Add(ms, 0);

                list = conf.GetAddressList();
                conf.InsertMS(ms);

                int id = conf.GetID(ms);
                byte[] buf = mm.MakeBody(fb.COMMAND.MS_ID, fb.STATUS.NONE, id.ToString(), "");
                 
                Header h = new Header(buf.Length, SrcDstType.CONFIG_SERVER, 0, SrcDstType.MATCHING_SERVER, 0);
                byte[] head = mm.StructureToByte(h);
                ms.Send(mm.MakePacket(head, buf));

                
                Receive(ms, receiveTask, list);
            }
            catch (Exception e)
            {
                
                Console.WriteLine("[Server][Accept]  error, code : {0}", e.ToString());


            }
        }

        private async void Receive(Socket socket, Task receiveTask, Dictionary<Socket, int> list)
        {
            while (socket != null && socket.Connected)
            {
                try
                {
                    if (receiveTask == null || receiveTask.IsCompleted)
                    {
                        Packet? packet = await Task.Run<Packet?>(() => ReceiveAsync(socket));

                        if (packet != null)
                            Process(socket, packet.Value.body, list);
                    }
                }
                catch (Exception)
                {

                }
            }
        }

        private Packet? ReceiveAsync(Socket socket)
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

                Packet p = new Packet();

                mm.ReadPacket(packet, out p);

                return p;
            }
            catch(SocketException se)
            {
                if (!socket.Connected)
                {
                    if (se.ErrorCode == 10060)
                    {
                        if (++heartBeatList[socket] > 3)
                        {
                            Close(socket);
                        }
                        else
                        {
                            SendHeartBeat(socket);
                        }
                    }
                    else
                    {
                        Console.WriteLine("[Server][Receive] {0}", se.ToString());
                        Close(socket);
                    }
                }

                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine("[Server][Receive] ", e.ToString());
                Close(socket);
                return null;
            }
            
        }

        private void Close(Socket s)
        {
            conf.DeleteMS(s);
            heartBeatList.Remove(s);
            s.Close();
        }

        private void SendHeartBeat(Socket s)
        {
            try
            {
                byte[] buf = mm.MakeBody(fb.COMMAND.HEALTH_CHECK, fb.STATUS.NONE, "", "");
                Header h = new Header(buf.Length, SrcDstType.CONFIG_SERVER, 0, SrcDstType.MATCHING_SERVER, 0);
                byte[] head = mm.StructureToByte(h);
                s.Send(mm.MakePacket(head, buf));
                Console.WriteLine("send heartbeat");

            }
            catch(Exception)
            {
                conf.DeleteMS(s);
                heartBeatList.Remove(s);
                s.Close();
                Console.WriteLine("delete server");
            }


        }

        private void Process(Socket s, Body b, Dictionary<Socket, int> list)
        {
            if (b.Cmd == fb.COMMAND.MSLIST_REQUEST)
            {
                if (list.Count != 0)
                {
                    Console.WriteLine("-------------------------");
                    Console.Write("List : ");
                    foreach (Socket soc in list.Keys)
                    {
                        Console.Write(list[soc] + " ");
                        if (soc == s)
                            continue;

                        byte[] buf = mm.MakeBody(fb.COMMAND.MSLIST_RESPONSE, fb.STATUS.SUCCESS, list[s].ToString(), s.RemoteEndPoint.ToString().Split(':')[0]);
                        Header h = new Header(buf.Length, SrcDstType.CONFIG_SERVER, 0, SrcDstType.MATCHING_SERVER, 0);
                        byte[] head = mm.StructureToByte(h);
                        s.Send(mm.MakePacket(head, buf));
                    }
                    Console.WriteLine("\n-------------------------");
                }
                else
                {
                    byte[] buf = mm.MakeBody(fb.COMMAND.MSLIST_RESPONSE, fb.STATUS.FAIL, "", "");
                    Header h = new Header(buf.Length, SrcDstType.CONFIG_SERVER, 0, SrcDstType.MATCHING_SERVER, 0);
                    byte[] head = mm.StructureToByte(h);
                    s.Send(mm.MakePacket(head, buf));
                }
                Console.WriteLine("===> send message to Match Server({0})....", s.RemoteEndPoint);
            }
            else if (b.Cmd == fb.COMMAND.HEALTH_CHECK)
            {
                heartBeatList[s] = 0;
                Console.WriteLine("recv heartbeat");
            }
        }

    }

    
}
