using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.IO;
using System.Windows.Forms;

namespace CoVayServer
{
    public class Room
    {
        public int RoomNumber;

        public Board board;

        public List<Player> ListPlayerinRoom;

        public int CountSkipTurn;

        public int CountReady;

        public int TimeOut;

        public bool Fight;

        public ThreadTime threadTimeOut;

        public Room(int roomNumber, Player player)
        {
            ListPlayerinRoom = new List<Player>();
            RoomNumber = roomNumber;
            ListPlayerinRoom.Add(player);
            TimeOut = 60;
            CountReady = 0;
            Fight = false;
        }

        public void SendMessageBox(string mess, Player player)
        {
            try
            {
                Message m = new Message(0);
                m.writer.Write(mess);
                player.AddMessage(m);
            }
            catch { }
        }

        public void SendTimeOut(bool dostop)
        {
            foreach (Player player in ListPlayerinRoom)
            {
                try
                {
                    Message m = new Message(7);
                    m.writer.Write(player.inTurn);
                    m.writer.Write(dostop);
                    player.AddMessage(m);
                }
                catch { }
            }
        }

        /// <summary>
        /// Gửi Message của 1 client cho các client trong room
        /// </summary>
        /// <param name="mess"></param>
        public void SendChat(string mess)
        {
            foreach (Player player1 in ListPlayerinRoom)
            {
                try
                {
                    Message m = new Message(2);
                    m.writer.Write(mess);
                    player1.AddMessage(m);
                }
                catch { }
            }
        }

        /// <summary>
        /// Gửi vị trí các quân cờ cho các client trong bàn cờ
        /// </summary>
        public void SendBoard()
        {
            int n = ConstNumber.linenum + 1;
            foreach (Player player1 in ListPlayerinRoom)
            {
                Message m = new Message(1);
                m.writer.Write(board.StoneNum);
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        m.writer.Write((short)board.matrix[j, i]);
                    }
                }
                player1.AddMessage(m);
            }

        }

        public void SendResetBoard()
        {
            foreach (Player player in ListPlayerinRoom)
            {
                try
                {
                    Message m = new Message(5);
                    player.AddMessage(m);
                }
                catch { }
            }
        }

        public void FinishRound()
        {
            if (board.white_killed > board.black_killed)
            {
                foreach (Player player1 in ListPlayerinRoom)
                {
                    if (player1.Isblack)
                    {
                        SendMessageBox("Chúc mừng, bạn đã thắng!!!", player1);
                    }
                    else
                    {
                        SendMessageBox("Bạn đã thua!!!", player1);
                    }
                }
            }
            else if (board.white_killed == board.black_killed)
            {
                foreach (Player player1 in ListPlayerinRoom)
                {
                    SendMessageBox("HÒA !!!!!!!!!!!!!!!", player1);
                }
            }
            else
            {
                foreach (Player player1 in ListPlayerinRoom)
                {
                    if (player1.Isblack)
                    {
                        SendMessageBox("Bạn đã thua!!!", player1);
                    }
                    else
                    {
                        SendMessageBox("Chúc mừng, bạn đã thắng!!!", player1);
                    }
                }
            }
            ResetRoom();
        }

        public void ResetRoom()
        {
            CountReady = 0;
            Fight = false;
            CountSkipTurn = 0;
            TimeOut = 60;
            if (threadTimeOut != null)
            {
                threadTimeOut.Stop();
            }
            board = new Board();
            foreach (Player player1 in ListPlayerinRoom)
            {
                player1.readyFight = false;
                player1.inTurn = false;
            }
        }
    }
}
