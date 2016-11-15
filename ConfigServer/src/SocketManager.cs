using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using FlatBuffers;
using fb;
using Newtonsoft.Json;
using System.IO;

namespace ConfigServer
{
    class SocketManager
    {
        private Socket listenSocket;
        private int port;

        private int backlog = 10;
        public bool listening;

        Protocol p = new Protocol();
        Config conf;

        Dictionary<Socket, int> list;

        int HEAD_SIZE = 20;
        int PACKET_SIZE = 100;

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
                list = conf.GetAddressList();
                conf.InsertMS(mServer);

                int id = conf.GetID(mServer);
                byte[] buf = MakeBody(fb.Command.MS_ID, fb.Status.NONE, id.ToString(), "");
                 
                Header h = new Header(buf.Length, SrcDstType.CONFIG_SERVER, 0, SrcDstType.MATCHING_SERVER, 0);
                byte[] head = p.StructureToByte(h);
                mServer.Send(MakePacket(head, buf));

                
                Receive(mServer, receiveTask, list);
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
                        bool result = await Task.Run<bool>(() => ReceiveAsync(socket, list));
                    }
                }
                catch (SocketException)
                {

                }
            }
        }

        private bool ReceiveAsync(Socket socket, Dictionary<Socket, int> list)
        {
            Console.WriteLine("\n===> Receiving from Match Server({0})....", socket.RemoteEndPoint);
            byte[] packet = new byte[PACKET_SIZE];
            int readBytes = socket.Receive(packet);
            if (readBytes == 0)
            {
                conf.DeleteMS(socket);
                socket.Close();
                return false;
            }

            byte[] header = new byte[HEAD_SIZE];
            Array.Copy(packet, header, HEAD_SIZE);            
            Header h = (Header)p.ByteToStructure(header, typeof(Header));            
            Console.WriteLine(h.lenght + " " + h.srcType + " " + h.srcCode + " " + h.dstType + " " + h.dstCode);

            if (h.lenght != 0)
            {
                byte[] body = new byte[h.lenght];
                Array.Copy(packet, HEAD_SIZE, body, 0, h.lenght);

                var b = Body.GetRootAsBody(new ByteBuffer(body));
                if (b.Cmd == fb.Command.MSLIST_REQUEST)
                {
                    if (list.Count != 0)
                    {
                        Console.WriteLine("======================================");
                        Console.Write("MS List : ");
                        foreach(Socket s in list.Keys)
                        {
                            Console.Write(list[s] + " ");
                            if (s == socket)
                                continue;
                            
                            byte[] buf = MakeBody(fb.Command.MSLIST_RESPONSE, fb.Status.SUCCESS, list[s].ToString(), s.RemoteEndPoint.ToString().Split(':')[0]);
                            h = new Header(buf.Length, SrcDstType.CONFIG_SERVER, 0, SrcDstType.MATCHING_SERVER, 0);
                            byte[] head = p.StructureToByte(h);
                            socket.Send(MakePacket(head, buf));
                        }
                        Console.WriteLine("\n======================================");
                    }
                    else
                    {
                        byte[] buf = MakeBody(fb.Command.MSLIST_RESPONSE, fb.Status.FAIL, "", "");
                        h = new Header(buf.Length, SrcDstType.CONFIG_SERVER, 0, SrcDstType.MATCHING_SERVER, 0);
                        byte[] head = p.StructureToByte(h);
                        socket.Send(MakePacket(head, buf));
                    }


                    Console.WriteLine("===> send message to Match Server({0})....", socket.RemoteEndPoint);
                    return true;
                }
                else if (b.Cmd == fb.Command.HEALTH_CHECK)
                {
                    return true;
                }
            }
            
            return false;
        }

        private byte[] MakeBody(fb.Command com, fb.Status st, string data1, string data2)
        {
            FlatBufferBuilder builder = new FlatBufferBuilder(1);
            StringOffset s1 = builder.CreateString(data1);
            StringOffset s2 = builder.CreateString(data2);
            Body.StartBody(builder);
            Body.AddCmd(builder, com);
            Body.AddStatus(builder, st);
            Body.AddData1(builder, s1);
            Body.AddData2(builder, s2);
            builder.Finish(Body.EndBody(builder).Value);
           return builder.SizedByteArray();
        }

        private byte[] MakePacket(byte[] h, byte[] b)
        {
            byte[] p = new byte[PACKET_SIZE];
            Array.Copy(h, p, h.Length);
            Array.Copy(b, 0, p, h.Length, b.Length);
            return p;
        }

    }
}
