using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;

namespace CoVayServer
{
    public class Player
    {
        public string name;

        public TcpClient tcpClient;

        public NetworkStream stream;

        public BinaryReader reader;

        public BinaryWriter writer;

        public List<Message> ListMessage;

        public bool Isblack;

        public bool readyFight;

        public bool inTurn;

        public int roomNumber;

        public Player(TcpClient tcp)
        {
            tcpClient = tcp;
            stream = tcpClient.GetStream();
            reader = new BinaryReader(stream, new UTF8Encoding());
            writer = new BinaryWriter(stream, new UTF8Encoding());
            roomNumber = -1;
            ListMessage = new List<Message>();
        }

        public void AddMessage(Message m)
        {
            ListMessage.Add(m);
        }
    }
}
