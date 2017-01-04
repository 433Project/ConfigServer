using fb;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Sockets;

namespace ConfigServer
{
    class ProcessManager
    {
        private static ILog logger = Logger.GetLoggerInstance();
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
            if (p.body.Cmd == Command.HealthCheckResponse)
            {
                ;
            }
            else if (p.body.Cmd == Command.MatchingServerListRequest)
            {
                config.InsertPort(s, p.body.Data1);
                int id = config.GetID(s);

                if (id == -1)
                {
                    return;
                }
               
                byte[] buf = msg.MakeBody(Command.MatchingServerIDResponse, Status.None, id.ToString(), "");
                Header h = new Header(buf.Length, TerminalType.ConfigServer, 0, TerminalType.MatchingServer, 0);
                byte[] head = msg.StructureToByte(h);
                s.Send(msg.MakePacket(head, buf));
            }
            else if (p.body.Cmd == fb.Command.MatchingServerListRequest)
            {

                if (serverList.Count != 0)
                {
                    foreach (int id in serverList.Keys)
                    { 
                        if(id >= p.header.srcCode)
                            continue;
                        byte[] buf = msg.MakeBody(Command.MatchingServerListResponse, Status.Success, id.ToString(), serverList[id]);
                        Header h = new Header(buf.Length, TerminalType.ConfigServer, 0, TerminalType.MatchingServer, 0);
                        byte[] head = msg.StructureToByte(h);
                        s.Send(msg.MakePacket(head, buf));
                    }
                }
                
            }
            else
            {
                logger.Error("===> recv unkwon Command : " + p.body.Cmd);
            }
        }

        public void SendHeartBeat(Socket s)
        {
            byte[] buf = msg.MakeBody(Command.HealthCheckRequest, Status.None, "", "");
            Header h = new Header(buf.Length, TerminalType.ConfigServer, 0, TerminalType.MatchingServer, 0);
            byte[] head = msg.StructureToByte(h);
            s.Send(msg.MakePacket(head, buf));
        }

        public void close(Socket s)
        {
            config.DeleteServer(s);
        }
    }
}
