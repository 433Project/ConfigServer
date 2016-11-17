using System;
using System.Runtime.InteropServices;

namespace ConfigServer
{
    struct Packet
    {
        public Header header;
        public fb.Body body;
    }

    struct Header
    {
        public int lenght;
        public SrcDstType srcType;    
        public int srcCode;   
        public SrcDstType dstType;    
        public int dstCode;
        public Header(int len, SrcDstType srcType, int srcCode, SrcDstType dstType, int dstCode)
        {
            this.lenght = len;
            this.srcType = srcType;
            this.srcCode = srcCode;
            this.dstType = dstType;
            this.dstCode = dstCode;
        }    
    }

    enum SrcDstType : int
    {
        MATCHING_SERVER = 0,
        MATCHING_CLIENT,
        ROOM_SERVER,
        PACKET_GENERATOR,
        MONITORING_SERVER,
        CONFIG_SERVER,
        CONNECTION_SERVER
    }

    class Protocol
    {
        
    }
}
