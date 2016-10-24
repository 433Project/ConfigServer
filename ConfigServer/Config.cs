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
        private List<IPAddress> msIPList;

        public Config()
        {
            msList = new Dictionary<Socket, IPAddress>();
            msIPList = new List<IPAddress>();
        }
        
        public bool InsertMS(Socket socket)
        {
            try
            {
                IPAddress ip = IPAddress.Parse(socket.RemoteEndPoint.ToString().Split(':')[0]);
                msList.Add(socket, ip);
                msIPList.Add(ip);
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
                msIPList.Remove(IPAddress.Parse(socket.RemoteEndPoint.ToString().Split(':')[0]));
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
            return msIPList;
        }

        public int GetCount()
        {
            return msList.Count;
        }


    }
}
