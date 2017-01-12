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

        /// <summary>
        /// Checks if the server ID is a valid key in the serverList dictionary. Written by Mik.
        /// </summary>
        /// <param name="id">A string value of the server's ID.</param>
        /// <returns>True if the key exists, false if not.</returns>
        public bool VerifyServerID (string id)
        {
            int idCode;
            if(!int.TryParse(id, out idCode))
            {
                return false;
            }
            return serverList.ContainsKey(idCode);
        }

        public bool InsertPort(Socket s, string port)
        {
            try
            {

                serverList.Add(serverIDs[s], s.RemoteEndPoint.ToString().Split(':')[0]); //Removed:  + ":" + port
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
