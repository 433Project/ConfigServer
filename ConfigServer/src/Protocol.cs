using System;
using System.Runtime.InteropServices;

namespace ConfigServer
{
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
    };

    class Protocol
    {
        public object ByteToStructure(byte[] data, Type type)
        {
            IntPtr buff = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, buff, data.Length);
            object obj = Marshal.PtrToStructure(buff, type);
            Marshal.FreeHGlobal(buff);

            if (Marshal.SizeOf(obj) != data.Length)
            {
                return null;
            }

            return obj;
        }
        public byte[] StructureToByte(object obj)
        {
            int datasize = Marshal.SizeOf(obj);
            IntPtr buff = Marshal.AllocHGlobal(datasize);
            Marshal.StructureToPtr(obj, buff, false);
            byte[] data = new byte[datasize];
            Marshal.Copy(buff, data, 0, datasize);
            Marshal.FreeHGlobal(buff);
            return data;
        }
    }
}
