using System;

using FlatBuffers;
using fb;
using System.Runtime.InteropServices;

namespace ConfigServer
{
    class MessageManager
    {
        int PACKET_SIZE = 100;
        int HEAD_SIZE = 20;
        public MessageManager()
        {
        }

        public byte[] MakeBody(fb.COMMAND com, fb.STATUS st, string data1, string data2)
        {
            FlatBufferBuilder builder = new FlatBufferBuilder(1);
            StringOffset s1 = builder.CreateString(data1);
            StringOffset s2 = builder.CreateString(data2);
            Body.StartBody(builder);
            Body.AddCmd(builder, com);
            Body.AddStatus(builder, st);
            Body.AddData1(builder, s1);
            Body.AddData2(builder, s2);
            builder.Finish(Body.EndBody(builder).Value);
            return builder.SizedByteArray();
        }

        public byte[] MakePacket(byte[] h, byte[] b)
        {
            byte[] p = new byte[PACKET_SIZE];
            Array.Copy(h, p, h.Length);
            Array.Copy(b, 0, p, h.Length, b.Length);
            return p;
        }

        public void ReadPacket(byte[] data, out Packet p)
        {
            
            byte[] header = new byte[HEAD_SIZE];
            Array.Copy(data, header, HEAD_SIZE);
            p.header =  (Header)ByteToStructure(header, typeof(Header));

            byte[] body = new byte[p.header.lenght];
            Array.Copy(data, HEAD_SIZE, body, 0, p.header.lenght);

            p.body = Body.GetRootAsBody(new ByteBuffer(body));
        }

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
