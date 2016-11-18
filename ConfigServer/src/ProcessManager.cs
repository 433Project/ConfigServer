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
        Dictionary<Socket, int> serverList;

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

            int id = config.GetID(s);

            if (id == -1)
            {
                return;
            }

            if (serverType == "MS")
            {
                byte[] buf = msg.MakeBody(COMMAND.MS_ID, STATUS.NONE, id.ToString(), "");

                Header h = new Header(buf.Length, TERMINALTYPE.CONFIG_SERVER, 0, TERMINALTYPE.MATCHING_SERVER, 0);
                byte[] head = msg.StructureToByte(h);
                s.Send(msg.MakePacket(head, buf));
            }
        }

        public void Process(Socket s, byte[] data)
        {
            Packet p = new Packet();
            msg.ReadPacket(data, out p);
            if (p.body.Cmd == COMMAND.HEALTH_CHECK)
            {
            }
            else if (p.body.Cmd == fb.COMMAND.MSLIST_REQUEST)
            {

                if (serverList.Count != 0)
                {
                    Console.WriteLine("-------------------------");
                    Console.Write("List : ");
                    foreach (Socket soc in serverList.Keys)
                    {
                        if (soc == s)
                            break;
                        Console.Write(serverList[soc] + " ");
                        byte[] buf = msg.MakeBody(COMMAND.MSLIST_RESPONSE, STATUS.SUCCESS, serverList[s].ToString(), s.RemoteEndPoint.ToString().Split(':')[0]);
                        Header h = new Header(buf.Length, TERMINALTYPE.CONFIG_SERVER, 0, TERMINALTYPE.MATCHING_SERVER, 0);
                        byte[] head = msg.StructureToByte(h);
                        s.Send(msg.MakePacket(head, buf));
                    }
                    Console.WriteLine("\n-------------------------");
                }
                else
                {
                    byte[] buf = msg.MakeBody(COMMAND.MSLIST_RESPONSE, STATUS.FAIL, "", "");
                    Header h = new Header(buf.Length, TERMINALTYPE.CONFIG_SERVER, 0, TERMINALTYPE.MATCHING_SERVER, 0);
                    byte[] head = msg.StructureToByte(h);
                    s.Send(msg.MakePacket(head, buf));
                }
                Console.WriteLine("===> send message to Match Server({0})....", s.RemoteEndPoint);
            }
            else
            {
                Console.WriteLine("===> recv unkwon command : ({0})....", p.body.Cmd);
            }
        }

        public void SendHeartBeat(Socket s)
        {
            byte[] buf = msg.MakeBody(COMMAND.HEALTH_CHECK, STATUS.NONE, "", "");
            Header h = new Header(buf.Length, TERMINALTYPE.CONFIG_SERVER, 0, TERMINALTYPE.MATCHING_SERVER, 0);
            byte[] head = msg.StructureToByte(h);
            s.Send(msg.MakePacket(head, buf));
        }

        public void close(Socket s)
        {
            //config.DeleteServer(s);
        }
    }
}
