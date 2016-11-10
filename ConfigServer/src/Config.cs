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
        private Dictionary<Socket, int> msList ;
        private int id;

        public Config()
        {
            msList = new Dictionary<Socket, int>();
            id = 0;
        }
        
        public bool InsertMS(Socket socket)
        {
            try
            {
                msList.Add(socket, id);
                id++;
                return true;
            }
            catch
            {
                return false;
            }
            
        }

        public bool DeleteMS(Socket socket)
        {
            try
            {
                msList.Remove(socket);
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
            return msList;
        }
        
        public int GetID(Socket s)
        {
            int value;

            if (msList.TryGetValue(s, out value))
                return value;
            else
                return -1;
        }


    }
}
