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

        public Config()
        {
            msList = new Dictionary<Socket, int>();
        }
        
        public bool InsertMS(Socket socket)
        {
            try
            {
                msList.Add(socket, msList.Count);
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

        public List<string> GetAddressList()
        {
            List<string> addrList = new List<string>();
            for (int i = 0; i < msList.Count; i++)
            {
                addrList.Add(msList)
            }
            return 
        }

    }
}
