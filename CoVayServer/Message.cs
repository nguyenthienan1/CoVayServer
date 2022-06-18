using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoVayServer
{
    public class Message
    {
        public MemoryStream m;

        public BinaryReader reader;

        public BinaryWriter writer;

        public int cmd;

        public Message(int _cmd)
        {
            cmd = _cmd;
            m = new MemoryStream();
            writer = new BinaryWriter(m);
        }

        public byte[] GetData()
        {
            byte[] data = m.ToArray();
            return data;
        }

        public Message(int _cmd, byte[] data)
        {
            cmd = _cmd;
            MemoryStream m = new MemoryStream(data);
            reader = new BinaryReader(m);
        }
    }
}
