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
            int id = config.GetID(s);

            switch (p.body.Cmd)
            {
                case Command.HealthCheckResponse:
                    SocketManager.heartBeatList[s] = 0;
                    break;

                case Command.MatchingServerIDRequest:
                    config.InsertPort(s, p.body.Data1);

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

                case Command.MatchingServerIDVerify:
                    bool isVerified = config.VerifyServerID(p.body.Data1);

#if DEBUG
                    // In the DEBUG version, make the verification random at 50% for TESTING PURPOSES
                    Random random = new Random();
                    isVerified = (random.NextDouble() <= 0.5f) ? true : false;
                    //Console.WriteLine(isVerified);
#endif

                    Status status;
                    if (isVerified)
                    {
                        status = Status.Success;
                    }
                    else
                    {
                        status = Status.Fail;
                    }
                    logger.Info("===> Sending MatchingServerIDVerifyResponse (ID = " + id + ") to " + s.RemoteEndPoint);
                    buf = msg.MakeBody(Command.MatchingServerIDVerifyResponse, status, id.ToString(), "");
                    h = new Header(buf.Length, TerminalType.ConfigServer, 0, TerminalType.MatchingServer, 0);
                    head = msg.StructureToByte(h);
                    s.Send(msg.MakePacket(head, buf));

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
