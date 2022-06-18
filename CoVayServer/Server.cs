using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Net.NetworkInformation;
using System.Web;

namespace CoVayServer
{
    public partial class Server : Form
    {
        public static List<Player> ListPlayer; //Danh sách player

        public bool listen;

        public static List<Room> ListRoom; //Danh sách Room

        public Server()
        {
            InitializeComponent();
        }

        private void Server_Load(object sender, EventArgs e)
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList) //Hiển thị các địa chỉ ip có thể connect đến server
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    string str = "Server turning on: " + ip.ToString() + ":8888";
                    AddTextOutput(str);
                }
            }
            //Tạo luồng nhận lắng nghe kết nối từ client
            Thread thread = new Thread(Listen);
            thread.Start();
        }

        /// <summary>
        /// Lắng nghe kết nối từ client
        /// </summary>
        public void Listen()
        {
            ListPlayer = new List<Player>();
            ListRoom = new List<Room>();
            listen = true;
            new Thread(Update).Start(); //Khởi chạy luồng update
            TcpListener tcpListener = new TcpListener(IPAddress.Any, 8888);
            tcpListener.Start();

            while (listen)
            {
                //Khi có 1 client kết nối đến tcpclient sẽ được tạo
                TcpClient tcpClient = tcpListener.AcceptTcpClient();

                //Tạo 1 player , player này sẽ là đóng vai trò như 1 client để Server tương tác
                Player player = new Player(tcpClient);

                //Thêm player vào danh sách
                ListPlayer.Add(player);

                new Thread(SendMsg).Start(player); //Tạo luồng gửi Msg cho client
                new Thread(ReceiveMsg).Start(player); //Tạo luồng nhận Msg client gửi
            }
        }

        /// <summary>
        /// Gửi Msg cho client
        /// </summary>
        /// <param name="obj"></param>
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
                        player.writer.Flush();
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

        /// <summary>
        /// Nhận Msg từ client
        /// </summary>
        /// <param name="obj"></param>
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
                    ProcessMsg(m, player);
                }
                catch (Exception e)
                {
                    player.tcpClient.Close();
                    Console.WriteLine(e.Message);
                    Console.WriteLine("Loi nhan Msg");
                }
                Thread.Sleep(10);
            }
        }

        /// <summary>
        /// Luồng update
        /// </summary>
        public new void Update()
        {
            while (listen)
            {
                UpdateListRoom();
                UpdateListPlayer();
                Thread.Sleep(1000); //Sau mỗi 1s các lệnh bên trên được thực hiện lại
            }
        }

        //Cập nhật danh sách player
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

        /// <summary>
        /// Gửi danh sách Room
        /// </summary>
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

        /// <summary>
        /// Hiển thị MessBox bên client bằng server
        /// </summary>
        /// <param name="mess"></param>
        /// <param name="player"></param>
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

        /// <summary>
        /// Gửi lệnh để client hiển thị form bàn cờ bên client
        /// </summary>
        /// <param name="player"></param>
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

        /// <summary>
        /// Xử lý dữ liệu từ client
        /// </summary>
        /// <param name="m"></param>
        /// <param name="player"></param>
        public void ProcessMsg(Message m, Player player)
        {
            //Dựa vào kí hiệu mà Client gửi đến sẽ có các cách xử lý khác nhau
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

                    //Nếu trận đấu chưa bắt đầu thì không thực hiện các câu lệnh bên dưới
                    if (!room4.Fight)
                    {
                        SendMessageBox("Trận đấu chưa bắt đầu", player);
                        break;
                    }

                    //Nếu player chưa đến lượt player đánh thì không thực hiện các câu lệnh bên dưới
                    if (!player.inTurn)
                    {
                        SendMessageBox("Chưa đến lượt bạn", player);
                        break;
                    }

                    int x = m.reader.ReadInt32();
                    int y = m.reader.ReadInt32();

                    //Nếu như đặt quân cờ không thỏa các điều kiện luật thì break
                    if (!room4.board.SetStone(player.Isblack, x, y))
                    {
                        break;
                    }

                    room4.CountSkipTurn = 0;

                    //Bỏ lượt đánh của player
                    player.inTurn = false;

                    //Set lượt đánh của player khác trong room
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
                    if (!room5.Fight) //Trận đấu chưa bắt đầu mà nhấn bỏ lượt thì break
                    {
                        SendMessageBox("Trận đấu chưa bắt đầu", player);
                        break;
                    }
                    if (!player.inTurn) //Nếu player không phải là người đánh mà nhấn bỏ lượt thì break
                    {
                        SendMessageBox("Chưa đến lượt bạn, không thể bỏ lượt", player);
                        break;
                    }
                    player.inTurn = false;

                    string mess3 = player.name + " ĐÃ BỎ LƯỢT";
                    room5.SendChat(mess3); //Gửi mess hiển thị player này bỏ lượt cho player khác

                    room5.CountSkipTurn++;

                    if (room5.CountSkipTurn == 2)
                    {
                        room5.SendChat("TRẬN ĐẤU KẾT THÚC do cả 2 đã bỏ lượt liên tiếp");
                        room5.FinishRound();
                        room5.SendTimeOut(true);
                        break;
                    }

                    room5.threadTimeOut.Reset();

                    //Set lại lượt đánh cho player khác
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
                        if (!CheckNumRoom(i)) //Kiểm tra số phòng có bị trùng
                        {
                            int3 = i;
                            break;
                        }
                    }
                    Room room = new Room(int3, player); //Tạo phòng mới
                    player.roomNumber = room.RoomNumber; //Set số phòng cho player

                    ListRoom.Add(room); //Thêm phòng này vào danh sách

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

                    if (player.readyFight) //player đã sẵn sàng
                    {
                        SendMessageBox("Bạn đã sẵn sàng rồi", player);
                        break;
                    }

                    player.readyFight = true;
                    string mess = player.name + " ĐÃ SẴN SÀNG";
                    room6.SendChat(mess); //Gửi tin nhắn đã sẵn sàng

                    if (room6.CountReady == 0) //Player sẵn sàng đầu tiên là quân đen, đánh lượt đầu
                    {
                        player.Isblack = true;
                        player.inTurn = true;
                        room6.CountReady++;
                    }
                    else                    //Player sẵn sàng kế là quân trắng, đánh lượt sau
                    {
                        player.Isblack = false;
                        player.inTurn = false;
                        room6.CountReady++;
                    }

                    if (room6.CountReady == 2) //Nếu 2 Player sẵn sàng thì bắt đầu ván đấu
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

                    if (room7.ListPlayerinRoom.Count >= 2) //Nếu phòng đã có 2 người hoặc nhiều hơn thì k cho vào nữa
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

        /// <summary>
        /// Kiểm tra player này có đang ở trong 1 room nào đó
        /// </summary>
        /// <param name="player"></param>
        /// <returns>true nếu player này đang ở trong 1 phòng nào đó</returns>
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

        /// <summary>
        /// Nhận về Room trong listRoom bằng số phòng
        /// </summary>
        /// <param name="RoomNum"></param>
        /// <returns>Room trong ListRoom, null nếu không tìm thấy</returns>
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

        /// <summary>
        /// Kiểm tra số phòng có bị trùng
        /// </summary>
        /// <param name="num"></param>
        /// <returns>true nếu đã sử dụng, false ngược lại</returns>
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

        /// <summary>
        /// Thêm output vào ListBoxOut
        /// </summary>
        /// <param name="cmd"></param>
        public void AddTextOutput(string cmd)
        {
            richTextBoxOutput.BeginInvoke(new Action(() =>
            {
                richTextBoxOutput.Text += "> " + cmd + "\n";
            }));
        }


        /// <summary>
        /// Đóng server
        /// </summary>
        public void StopServer()
        {
            listen = false;
            Environment.Exit(0);
        }

        // Sự kiện khi nhân nút X
        private void Server_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult dialog;
            dialog = MessageBox.Show("Bạn có muốn tắt server?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialog == DialogResult.Yes)
            {
                StopServer();
            }
            else
            {
                e.Cancel = true; //Nếu nhấn nút No thì không tắt
            }
        }
    }
}
