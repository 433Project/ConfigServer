using fb;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Sockets;

namespace ConfigServer
{
    class ProcessManager
    {
        MessageManager msg;
        Config config;
        Dictionary<int, string> serverList;

        public ProcessManager()
        {
            msg = new MessageManager();
            config = new Config();
            serverList = config.GetAddressList();
        }

        public void ProcessAccept(Socket s, string serverType)
        {
            if (!config.InsertServer(s))
            {
                return;
            }
        }

        public void Process(Socket s, byte[] data)
        {
            Packet p = new Packet();
            msg.ReadPacket(data, out p);
            if (p.body.Cmd == COMMAND.HEALTH_CHECK_RESPONSE)
            {
                ;
            }
            else if (p.body.Cmd == COMMAND.MS_ID_REQUEST)
            {
                config.InsertPort(s, p.body.Data1);
                int id = config.GetID(s);

                if (id == -1)
                {
                    return;
                }
               
                byte[] buf = msg.MakeBody(COMMAND.MS_ID_RESPONSE, STATUS.NONE, id.ToString(), "");
                Header h = new Header(buf.Length, TERMINALTYPE.CONFIG_SERVER, 0, TERMINALTYPE.MATCHING_SERVER, 0);
                byte[] head = msg.StructureToByte(h);
                s.Send(msg.MakePacket(head, buf));
            }
            else if (p.body.Cmd == fb.COMMAND.MSLIST_REQUEST)
            {

                if (serverList.Count != 0)
                {
                    Console.WriteLine("-------------------------");
                    Console.Write("List : ");
                    foreach (int id in serverList.Keys)
                    {
                        Console.Write(id + " ");
                        if(id >= p.header.srcCode)
                            continue;
                        byte[] buf = msg.MakeBody(COMMAND.MSLIST_RESPONSE, STATUS.SUCCESS, id.ToString(), serverList[id]);
                        Header h = new Header(buf.Length, TERMINALTYPE.CONFIG_SERVER, 0, TERMINALTYPE.MATCHING_SERVER, 0);
                        byte[] head = msg.StructureToByte(h);
                        s.Send(msg.MakePacket(head, buf));
                    }
                    Console.WriteLine("\n-------------------------");
                    Console.WriteLine("===> send message to Match Server({0})....", s.RemoteEndPoint);
                }
                
            }
            else
            {
                Console.WriteLine("===> recv unkwon command : ({0})....", p.body.Cmd);
            }
        }

        public void SendHeartBeat(Socket s)
        {
            byte[] buf = msg.MakeBody(COMMAND.HEALTH_CHECK_REQUEST, STATUS.NONE, "", "");
            Header h = new Header(buf.Length, TERMINALTYPE.CONFIG_SERVER, 0, TERMINALTYPE.MATCHING_SERVER, 0);
            byte[] head = msg.StructureToByte(h);
            s.Send(msg.MakePacket(head, buf));
        }

        public void close(Socket s)
        {
            config.DeleteServer(s);
        }
    }
}
