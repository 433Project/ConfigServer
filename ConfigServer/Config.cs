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
        private Dictionary<Socket, IPAddress> msList ;

        public Config()
        {
            msList = new Dictionary<Socket, IPAddress>();
        }
        
        public bool InsertMS(Socket socket)
        {
            try
            {
                msList.Add(socket, IPAddress.Parse(socket.RemoteEndPoint.ToString().Split(':')[0]));
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
        public List<IPAddress> GetAddressList()
        {
            List<IPAddress> addrList = new List<IPAddress>();
            foreach(var item in msList)
            {
                addrList.Add(item.Value);
            }
            return addrList;
        }

        public int GetCount()
        {
            return msList.Count;
        }


    }
}
