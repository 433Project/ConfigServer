using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ConfigServer
{
    class Config
    {
        private int id;
        private Dictionary<Socket, int> serverList ; //socket, id

        public Config()
        {
            serverList = new Dictionary<Socket, int>();
            id = 0;
        }
        
        public bool InsertServer(Socket socket)
        {
            try
            {
                serverList.Add(socket, id);
                id++;
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
                serverList.Remove(socket);
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
        public Dictionary<Socket, int> GetAddressList()
        {
            return serverList;
        }
        
        public int GetID(Socket s)
        {
            int value;
            try
            {
                if (serverList.TryGetValue(s, out value))
                    return value;
                else
                    return -1;
            }
            catch (Exception)
            {
                return -1;
            }
        }


    }
}
