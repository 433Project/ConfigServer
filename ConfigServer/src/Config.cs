using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace ConfigServer
{
    class Config
    {
        private int id;
        private Dictionary<Socket, int> serverIDs; //socket, id
        private Dictionary<int, string> serverList; // id, ip:port

        public Config()
        {
            serverIDs = new Dictionary<Socket, int>();
            serverList = new Dictionary<int, string>();
            id = 1;
        }
        
        public bool InsertServer(Socket socket)
        {
            try
            {
                serverIDs.Add(socket, id);
                id++;
                return true;
            }
            catch
            {
                return false;
            }
            
        }

        public bool InsertPort(Socket s, string port)
        {
            try
            {

                serverList.Add(serverIDs[s], s.RemoteEndPoint.ToString().Split(':')[0] + ":" + port);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteServer(Socket socket)
        {
            try
            {
                serverList.Remove(serverIDs[socket]);
                serverIDs.Remove(socket);
                return true;
            }
            catch
            {
                return false;
            }

        }

        /// <summary>
        /// return EndPoint(ip:port) list
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, string> GetAddressList()
        {
            return serverList;
        }
        
        public int GetID(Socket s)
        {
            if (serverIDs.ContainsKey(s))
                return serverIDs[s];
            else
                return -1;
        }


    }
}
