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

            switch (p.body.Cmd)
            {
                case Command.HealthCheckResponse:
                    SocketManager.heartBeatList[s] = 0;
                    break;

                case Command.MatchingServerIDRequest:
                    config.InsertPort(s, p.body.Data1);
                    int id = config.GetID(s);

                    if (id == -1)
                    {
                        logger.Error("===> Invalid id (-1) for MatchingServer: " + s.RemoteEndPoint);
                        return;
                    }

                    logger.Info("===> Sending MatchingServerIDResponse (ID = " + id + ") to " + s.RemoteEndPoint);
                    byte[] buf = msg.MakeBody(Command.MatchingServerIDResponse, Status.None, id.ToString(), "");
                    Header h = new Header(buf.Length, TerminalType.ConfigServer, 0, TerminalType.MatchingServer, 0);
                    byte[] head = msg.StructureToByte(h);
                    s.Send(msg.MakePacket(head, buf));
                    break;

                case Command.MatchingServerListRequest:
                    SendMatchingServerListResponse(s, p);
                    break;

                default:
                    logger.Error("===> Received unknown Command : " + p.body.Cmd);
                    break;
            }
        }

        private void SendMatchingServerListResponse (Socket s, Packet p)
        {
            if (serverList.Count != 0)
            {
                foreach (int id in serverList.Keys)
                {
                    if (id >= p.header.srcCode)
                        continue;
                    logger.Info("===> Sending MatchingServerListResponse (ID = " + id + " IP = " + serverList[id] +") to " + s.RemoteEndPoint);
                    byte[] buf = msg.MakeBody(Command.MatchingServerListResponse, Status.Success, id.ToString(), serverList[id]);
                    Header h = new Header(buf.Length, TerminalType.ConfigServer, 0, TerminalType.MatchingServer, 0);
                    byte[] head = msg.StructureToByte(h);
                    s.Send(msg.MakePacket(head, buf));
                }
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
