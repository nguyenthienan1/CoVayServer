using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CoVayServer
{
    public class Server
    {
        public static List<Player> ListPlayer;

        public bool listen;

        public static List<Room> ListRoom;

        public static void Main()
        {
            new Server().Run();
        }

        public void Run()
        {
            ListPlayer = new List<Player>();
            ListRoom = new List<Room>();
            listen = true;
            new Thread(UpdateServer).Start();
            TcpListener tcpListener = new TcpListener(IPAddress.Any, 8888);
            tcpListener.Start();

            while (listen)
            {
                TcpClient tcpClient = tcpListener.AcceptTcpClient();

                Player player = new Player(tcpClient);

                ListPlayer.Add(player);

                new Thread(SendMsg).Start(player);
                new Thread(ReceiveMsg).Start(player);
                new Thread(ProcessMessage).Start(player);
            }
        }

        public void ProcessMessage(object obj)
        {
            Player player = (Player)obj;
            while (player.tcpClient.Connected)
            {
                if (player.ListMessageProcess.Count > 0)
                {
                    try
                    {
                        Message m = player.ListMessageProcess[0];
                        player.ListMessageProcess.RemoveAt(0);
                        ProcessMsg(m, player);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        Console.WriteLine("Loi gui Msg");
                    }
                }
                Thread.Sleep(10);
            }
        }

        public void SendMsg(object obj)
        {
            Player player = (Player)obj;
            while (player.tcpClient.Connected)
            {
                if (player.ListMessage.Count > 0)
                {
                    try
                    {
                        Message m = player.ListMessage[0];
                        player.ListMessage.RemoveAt(0);
                        byte[] data = m.GetData();
                        player.writer.Write(data.Length);
                        player.writer.Write(m.cmd);
                        player.writer.Write(data);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        Console.WriteLine("Loi gui Msg");
                    }
                }
                Thread.Sleep(10);
            }
        }

        public void ReceiveMsg(object obj)
        {
            Player player = (Player)obj;
            while (player.tcpClient.Connected)
            {
                try
                {
                    int length = player.reader.ReadInt32();
                    int cmd = player.reader.ReadInt32();
                    byte[] data = player.reader.ReadBytes(length);
                    Message m = new Message(cmd, data);
                    player.ListMessageProcess.Add(m);
                }
                catch (Exception e)
                {
                    player.tcpClient.Close();
                    Console.WriteLine(e.Message);
                    Console.WriteLine("Loi nhan Msg");
                }
            }
        }

        public void UpdateServer()
        {
            while (listen)
            {
                UpdateListRoom();
                UpdateListPlayer();
                Thread.Sleep(100);
            }
        }

        public void UpdateListPlayer()
        {
            lock (ListPlayer)
            {
                //Nếu player nào bị mất kết nối thì xóa khỏi ListPlayer
                for (int i = 0; i < ListPlayer.Count; i++)
                {
                    if (!ListPlayer[i].tcpClient.Connected)
                    {
                        AddTextOutput(ListPlayer[i].name + " has been disconnected");
                        ListPlayer.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        //Cập nhật danh sách phòng
        public void UpdateListRoom()
        {
            lock (ListRoom)
            {
                for (int i = 0; i < ListRoom.Count; i++)
                {
                    //Nếu player nào bị mất kết nối thì xóa khỏi ListPlayerinRoom
                    for (int j = 0; j < ListRoom[i].ListPlayerinRoom.Count; j++)
                    {
                        if (!ListRoom[i].ListPlayerinRoom[j].tcpClient.Connected)
                        {
                            ListRoom[i].ListPlayerinRoom.RemoveAt(j);
                            j--;
                            if (ListRoom[i].Fight)
                            {
                                ListRoom[i].ResetRoom();
                            }
                        }
                    }
                    //Nếu Room không còn player nào thì bị xóa
                    if (ListRoom[i].ListPlayerinRoom.Count <= 0)
                    {
                        ListRoom.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        public void SendListRoom(Player player)
        {
            try
            {
                Message m = new Message(3);
                m.writer.Write(ListRoom.Count);
                foreach (Room room in ListRoom)
                {
                    m.writer.Write(room.RoomNumber);
                    m.writer.Write(room.ListPlayerinRoom[0].name);
                    m.writer.Write(room.ListPlayerinRoom.Count);
                }
                player.AddMessage(m);
            }
            catch { }
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

        public void SendShowBoard(Player player)
        {
            try
            {
                Message m = new Message(6);
                m.writer.Write(player.roomNumber);
                player.AddMessage(m);
            }
            catch { }
        }

        public void ProcessMsg(Message m, Player player)
        {
            switch (m.cmd)
            {
                case 0: //Set tên player

                    player.name = m.reader.ReadString();
                    string str = player.name + " has been connected";
                    AddTextOutput(str);
                    SendListRoom(player);
                    break;

                case 1: // Nhận tọa độ quân cờ từ Client
                    Room room4 = GetRoom(player.roomNumber); //Room của player
                    if (room4 == null)
                    {
                        break;
                    }

                    if (!room4.Fight)
                    {
                        SendMessageBox("Trận đấu chưa bắt đầu", player);
                        break;
                    }

                    if (!player.inTurn)
                    {
                        SendMessageBox("Chưa đến lượt bạn", player);
                        break;
                    }

                    int x = m.reader.ReadInt32();
                    int y = m.reader.ReadInt32();

                    if (!room4.board.SetStone(player.Isblack, x, y))
                    {
                        break;
                    }

                    room4.CountSkipTurn = 0;

                    player.inTurn = false;

                    foreach (Player player1 in room4.ListPlayerinRoom)
                    {
                        if (player1 != player)
                        {
                            player1.inTurn = true;
                        }
                    }

                    room4.threadTimeOut.Reset();

                    room4.SendTimeOut(false);

                    room4.SendBoard();

                    break;

                case 2: //Nhận chat từ player
                    string str33 = m.reader.ReadString();
                    str33 = str33.Trim();
                    if (str33 != "")
                    {
                        string mess2 = player.name + ": " + str33;
                        Room room2 = GetRoom(player.roomNumber);
                        if (room2 != null)
                        {
                            room2.SendChat(mess2); //Gửi lại cho các player trong room
                        }
                    }
                    break;

                case 3: //  Nhận tín hiệu bỏ lượt từ player
                    Room room5 = GetRoom(player.roomNumber);
                    if (room5 == null)
                    {
                        break;
                    }
                    if (!room5.Fight) 
                    {
                        SendMessageBox("Trận đấu chưa bắt đầu", player);
                        break;
                    }
                    if (!player.inTurn)
                    {
                        SendMessageBox("Chưa đến lượt bạn, không thể bỏ lượt", player);
                        break;
                    }
                    player.inTurn = false;

                    string mess3 = player.name + " ĐÃ BỎ LƯỢT";
                    room5.SendChat(mess3); 

                    room5.CountSkipTurn++;

                    if (room5.CountSkipTurn == 2)
                    {
                        room5.SendChat("TRẬN ĐẤU KẾT THÚC do cả 2 đã bỏ lượt liên tiếp");
                        room5.FinishRound();
                        room5.SendTimeOut(true);
                        break;
                    }

                    room5.threadTimeOut.Reset();

                    
                    foreach (Player player1 in room5.ListPlayerinRoom)
                    {
                        if (player1 != player)
                        {
                            player1.inTurn = true;
                        }
                    }

                    room5.SendTimeOut(false);
                    break;

                case 4: //Nhận lệnh tạo room
                    if (CheckPlayerinOtherRoom(player))
                    {
                        SendMessageBox("Bạn đang ở trong phòng khác!", player);
                        break;
                    }
                    int int3 = 1;
                    for (int i = 1; i < 100; i++)
                    {
                        if (!CheckNumRoom(i))
                        {
                            int3 = i;
                            break;
                        }
                    }
                    Room room = new Room(int3, player);
                    player.roomNumber = room.RoomNumber; 

                    ListRoom.Add(room); 

                    SendShowBoard(player);

                    break;

                case 5: //Nhận lệnh khi nhấn nút sẵn sàng từ client
                    Room room6 = GetRoom(player.roomNumber);
                    if (room6 == null)
                    {
                        break;
                    }

                    if (room6.Fight)
                    {
                        SendMessageBox("Trận đấu đang diễn ra", player);
                        break;
                    }

                    if (player.readyFight)
                    {
                        SendMessageBox("Bạn đã sẵn sàng rồi", player);
                        break;
                    }

                    player.readyFight = true;
                    string mess = player.name + " ĐÃ SẴN SÀNG";
                    room6.SendChat(mess);

                    if (room6.CountReady == 0)
                    {
                        player.Isblack = true;
                        player.inTurn = true;
                        room6.CountReady++;
                    }
                    else                    
                    {
                        player.Isblack = false;
                        player.inTurn = false;
                        room6.CountReady++;
                    }

                    if (room6.CountReady == 2)
                    {
                        room6.Fight = true;
                        room6.board = new Board(); //Tạo bàn cờ
                        room6.SendResetBoard();
                        room6.SendChat("Trận đấu bắt đầu!!!!");
                        room6.SendTimeOut(false);
                        room6.threadTimeOut = new ThreadTime(room6);
                        room6.threadTimeOut.Start();
                    }
                    break;

                case 6: //Nhận lệnh join room
                    if (CheckPlayerinOtherRoom(player))  //Kiểm tra player có ở trong room khác
                    {
                        SendMessageBox("Bạn đang ở trong phòng khác!", player);
                        break;
                    }

                    int roomnum = m.reader.ReadInt32();

                    Room room7 = GetRoom(roomnum);
                    if (room7 == null)
                    {
                        SendMessageBox("Không tìm thấy phòng đã chọn, hãy cập nhật danh sách phòng", player);
                        break;
                    }

                    if (room7.ListPlayerinRoom.Count >= 2)
                    {
                        SendMessageBox("Phòng đã đầy", player);
                        break;
                    }
                    //Set số phòng cho player
                    player.roomNumber = roomnum;

                    room7.ListPlayerinRoom.Add(player);

                    SendShowBoard(player);

                    break;
                case 7: //Nhận lệnh rời phòng, set lại các thuộc tính khi player chưa vào phòng nào
                    Room room8 = GetRoom(player.roomNumber);

                    if (room8 == null)
                    {
                        break;
                    }

                    room8.ResetRoom();

                    room8.ListPlayerinRoom.Remove(player); //Xóa player trong phòng

                    player.roomNumber = -1;

                    if (room8.ListPlayerinRoom.Count <= 0)
                    {
                        ListRoom.Remove(room8);
                        break;
                    }

                    foreach (Player player1 in room8.ListPlayerinRoom)
                    {
                        if (player1 != player)
                        {
                            SendMessageBox("Người chơi " + player.name + " đã rời phòng", player1);
                        }
                    }
                    room8.SendTimeOut(true);

                    break;
                case 8:
                    //Nhận lệnh cập nhật ListRoom cho player
                    SendListRoom(player);
                    break;

            }
        }

        public bool CheckPlayerinOtherRoom(Player player)
        {
            foreach (Room room1 in ListRoom)
            {
                foreach (Player player1 in room1.ListPlayerinRoom)
                {
                    if (player1 == player)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static Room GetRoom(int RoomNum)
        {
            foreach (Room room in ListRoom)
            {
                if (room.RoomNumber == RoomNum)
                {
                    return room;
                }
            }
            return null;
        }

        public bool CheckNumRoom(int num)
        {
            foreach (Room room in ListRoom)
            {
                if (room.RoomNumber == num)
                {
                    return true;
                }
            }
            return false;
        }

        public void AddTextOutput(string cmd)
        {
            Console.Out.WriteLine(cmd);
        }
    }
}
