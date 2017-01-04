using fb;

namespace ConfigServer
{
    struct Packet
    {
        public Header header;
        public Body body;
    }

    struct Header
    {
        public int length;
        public TERMINALTYPE srcType;    
        public int srcCode;   
        public TERMINALTYPE dstType;    
        public int dstCode;
        public Header(int len, TERMINALTYPE srcType, int srcCode, TERMINALTYPE dstType, int dstCode)
        {
            this.length = len;
            this.srcType = srcType;
            this.srcCode = srcCode;
            this.dstType = dstType;
            this.dstCode = dstCode;
        }    
    }

    enum TERMINALTYPE : int
    {
        MATCHING_SERVER = 0,
        MATCHING_CLIENT,
        ROOM_SERVER,
        PACKET_GENERATOR,
        MONITORING_SERVER,
        CONFIG_SERVER,
        CONNECTION_SERVER
    }
}
