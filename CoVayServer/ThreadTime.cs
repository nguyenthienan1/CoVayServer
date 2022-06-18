using System;
using System.Timers;

namespace CoVayServer
{
    public class ThreadTime
    {
        Room room;
        Timer timer;

		public ThreadTime(Room r)
		{
			room = r;
			timer = new Timer(1000);
            timer.Elapsed += timer_Tick;
            timer.AutoReset = true;
        }
		
		void timer_Tick(object sender, EventArgs e)
        {
            if (room.TimeOut <= 0)
            {
                foreach (Player player in room.ListPlayerinRoom)
                {
                    if (player.inTurn)
                    {
                        room.SendChat(player.name + " hết thời gian chờ, TRẬN ĐẤU KẾT THÚC!!!");
                    }
                }
                room.FinishRound();
                room.SendTimeOut(true);
                return;
            }
            room.TimeOut--;
        }

        public void Reset()
        {
            timer.Stop();
            room.TimeOut = 60;
            timer.Start();
        }

        public void Start()
        {
            timer.Start();
        }

		public void Stop()
        {
			timer.Stop();
        }
	}
}
