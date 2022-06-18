using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace CoVayServer
{
    public class Board
    {
        public Matrix matrix; //Mảng 2 chiều lưu vị trí quân cờ
        public int StoneNum;

        public int black_killed;
        public int white_killed;

        public Board()
        {
            StoneNum = 0;
            black_killed = 0;
            white_killed = 0;
            matrix = new Matrix();
        }


        public void DrawBoard(Graphics gr)
        {
            //Vẽ background bàn cờ
            Pen yellowPen = new Pen(Color.SandyBrown, 3);
            Rectangle rect = new Rectangle(0, 0, 430, 430);
            Brush yelloBrush = new SolidBrush(Color.SandyBrown);
            gr.DrawRectangle(yellowPen, rect);
            RectangleF rectF = new RectangleF(0, 0, 430, 430);
            gr.FillRectangle(yelloBrush, rectF);

            //Vẽ đường kẻ dọc
            for (int i = 1; i <= ConstNumber.linenum; i++)
            {
                Point start = new Point(ConstNumber.cellSize, i * ConstNumber.cellSize);
                Point end = new Point(ConstNumber.linenum * ConstNumber.cellSize, i * ConstNumber.cellSize);
                gr.DrawLine(Pens.Black, start, end);
            }
            //Vẽ đường kẻ ngang
            for (int i = 1; i <= ConstNumber.linenum; i++)
            {
                Point start = new Point(i * ConstNumber.cellSize, ConstNumber.cellSize);
                Point end = new Point(i * ConstNumber.cellSize, ConstNumber.linenum * ConstNumber.cellSize);
                gr.DrawLine(Pens.Black, start, end);
            }
            //Vẽ số hàng ngang
            for (int i = 1; i <= ConstNumber.linenum; i++)
            {
                string drawstr = i.ToString();
                Font drawfont = new Font("Arial", 10);
                SolidBrush drawbrush = new SolidBrush(Color.Black);
                PointF drawpointf = new PointF((float)(i * ConstNumber.cellSize) - 10, 0);
                gr.DrawString(drawstr, drawfont, drawbrush, drawpointf);
            }
            //Vẽ số hàng dọc
            for (int i = 1; i <= ConstNumber.linenum; i++)
            {
                string drawstr = i.ToString();
                Font drawfont = new Font("Arial", 10);
                SolidBrush drawbrush = new SolidBrush(Color.Black);
                PointF drawpointf = new PointF(0, (float)(i * ConstNumber.cellSize) - 10);
                gr.DrawString(drawstr, drawfont, drawbrush, drawpointf);
            }
        }

        protected void DrawStone(Graphics gr, bool isblack, int x, int y)
        {
            int r = ConstNumber.cellSize / 2;
            Color color = isblack == true ? Color.Black : Color.White;
            Brush mybrush = new SolidBrush(color);

            int rectX = x * 2 * r - r;
            int rectY = y * 2 * r - r;

            gr.FillEllipse(mybrush, rectX, rectY, 2 * r, 2 * r);
            gr.DrawEllipse(Pens.Black, rectX, rectY, 2 * r, 2 * r);
        }

        public void DrawStones(Graphics gr)
        {
            int n = ConstNumber.linenum + 1;
            for (int i = 1; i < n; i++)
            {
                for (int j = 1; j < n; j++)
                {
                    if (matrix[i, j] != 0)
                    {
                        bool isblack = true;
                        if (matrix[i, j] < 0)
                        {
                            isblack = false;
                        }
                        DrawStone(gr, isblack, i, j);
                        if (Math.Abs(matrix[i, j]) == StoneNum)
                        {
                            DrawNewFlag(gr, i, j);
                        }
                    }
                }
            }
        }

        protected void DrawNewFlag(Graphics gr, int x, int y)
        {
            int r = ConstNumber.cellSize / 2;
            int rectX = x * 2 * r - r / 2;
            int rectY = y * 2 * r - r / 2;
            gr.FillEllipse(Brushes.Green, rectX, rectY, r - 2, r - 2);
        }

        /// <summary>
        /// Set quân cờ vào bàn cờ
        /// </summary>
        /// <param name="isblack"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool SetStone(bool isblack, int x, int y)
        {
            // Nếu tọa độ điểm này đã có quân cờ thì trả về false
            if (matrix[x, y] != 0)
            {
                return false;
            }

            Matrix m = matrix.Copy();
            int flag = 1;
            if (!isblack)
            {
                flag = -1;
            }
            m[x, y] = flag * (StoneNum + 1);

            //Nếu vị trí quân cờ này đặt không có khí thì trả về false
            if (!KillStone(m, x, y) && CountLiberty(m, x, y) == 0)
            {
                return false;
            }

            //Nếu tìm thấy 1 quân cờ bàn cờ mới không cớ với quân cờ bàn cờ cũ thì return
            if (StoneNum - 1 >= 0 && m.EqualSituationWith(matrix))
            {
                return false;
            }

            StoneNum++;
            matrix = m;
            return true;
        }

        /// <summary>
        /// Đếm khí quân cờ
        /// </summary>
        /// <param name="m"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int CountLiberty(Matrix m, int x, int y)
        {
            int liberty = 0;
            bool[,] array = new bool[20, 20];
            bool flag = false;
            if (m[x, y] > 0)
            {
                flag = true;
            }
            array[x, y] = true;
            if (flag)
            {
                CountBlackLiberty(m, x, y, array, ref liberty);
            }
            else
            {
                CountWhiteLiberty(m, x, y, array, ref liberty);
            }
            return liberty;
        }

        protected void CountBlackLiberty(Matrix m, int x, int y, bool[,] visited, ref int liberty)
        {
            if (x - 1 >= 1 && !visited[x - 1, y])
            {
                visited[x - 1, y] = true;
                int num = m[x - 1, y];
                if (num == 0)
                {
                    liberty++;
                }
                else if (num >= 0 && num > 0)
                {
                    CountBlackLiberty(m, x - 1, y, visited, ref liberty);
                }
            }
            if (y - 1 >= 1 && !visited[x, y - 1])
            {
                visited[x, y - 1] = true;
                int num2 = m[x, y - 1];
                if (num2 == 0)
                {
                    liberty++;
                }
                else if (num2 >= 0 && num2 > 0)
                {
                    CountBlackLiberty(m, x, y - 1, visited, ref liberty);
                }
            }
            if (x + 1 <= 13 && !visited[x + 1, y])
            {
                visited[x + 1, y] = true;
                int num3 = m[x + 1, y];
                if (num3 == 0)
                {
                    liberty++;
                }
                else if (num3 >= 0 && num3 > 0)
                {
                    CountBlackLiberty(m, x + 1, y, visited, ref liberty);
                }
            }
            if (y + 1 <= 13 && !visited[x, y + 1])
            {
                visited[x, y + 1] = true;
                int num4 = m[x, y + 1];
                if (num4 == 0)
                {
                    liberty++;
                }
                else if (num4 >= 0 && num4 > 0)
                {
                    CountBlackLiberty(m, x, y + 1, visited, ref liberty);
                }
            }
        }

        protected void CountWhiteLiberty(Matrix m, int x, int y, bool[,] visited, ref int liberty)
        {
            if (x - 1 >= 1 && !visited[x - 1, y])
            {
                visited[x - 1, y] = true;
                int num = m[x - 1, y];
                if (num == 0)
                {
                    liberty++;
                }
                else if (num <= 0 && num < 0)
                {
                    CountWhiteLiberty(m, x - 1, y, visited, ref liberty);
                }
            }
            if (y - 1 >= 1 && !visited[x, y - 1])
            {
                visited[x, y - 1] = true;
                int num2 = m[x, y - 1];
                if (num2 == 0)
                {
                    liberty++;
                }
                else if (num2 <= 0 && num2 < 0)
                {
                    CountWhiteLiberty(m, x, y - 1, visited, ref liberty);
                }
            }
            if (x + 1 <= 13 && !visited[x + 1, y])
            {
                visited[x + 1, y] = true;
                int num3 = m[x + 1, y];
                if (num3 == 0)
                {
                    liberty++;
                }
                else if (num3 <= 0 && num3 < 0)
                {
                    CountWhiteLiberty(m, x + 1, y, visited, ref liberty);
                }
            }
            if (y + 1 <= 13 && !visited[x, y + 1])
            {
                visited[x, y + 1] = true;
                int num4 = m[x, y + 1];
                if (num4 == 0)
                {
                    liberty++;
                }
                else if (num4 <= 0 && num4 < 0)
                {
                    CountWhiteLiberty(m, x, y + 1, visited, ref liberty);
                }
            }
        }

        public bool KillStone(Matrix m, int x, int y)
        {
            bool flag2 = false;
            if (m[x, y] > 0)
            {
                flag2 = true;
            }
            if (flag2)
            {
                return KillWhiteSearch(m, x, y);
            }
            return KillBlackSearch(m, x, y);
        }

        protected bool KillBlackSearch(Matrix m, int x, int y)
        {
            bool result = false;
            if (x - 1 >= 1 && m[x - 1, y] > 0 && CountLiberty(m, x - 1, y) == 0)
            {
                Kill(m, x - 1, y);
                result = true;
            }
            if (y - 1 >= 1 && m[x, y - 1] > 0 && CountLiberty(m, x, y - 1) == 0)
            {
                Kill(m, x, y - 1);
                result = true;
            }
            if (x + 1 <= 13 && m[x + 1, y] > 0 && CountLiberty(m, x + 1, y) == 0)
            {
                Kill(m, x + 1, y);
                result = true;
            }
            if (y + 1 <= 13 && m[x, y + 1] > 0 && CountLiberty(m, x, y + 1) == 0)
            {
                Kill(m, x, y + 1);
                result = true;
            }
            return result;
        }

        protected bool KillWhiteSearch(Matrix m, int x, int y)
        {
            bool result = false;
            if (x - 1 >= 1 && m[x - 1, y] < 0 && CountLiberty(m, x - 1, y) == 0)
            {
                Kill(m, x - 1, y);
                result = true;
            }
            if (y - 1 >= 1 && m[x, y - 1] < 0 && CountLiberty(m, x, y - 1) == 0)
            {
                Kill(m, x, y - 1);
                result = true;
            }
            if (x + 1 <= 13 && m[x + 1, y] < 0 && CountLiberty(m, x + 1, y) == 0)
            {
                Kill(m, x + 1, y);
                result = true;
            }
            if (y + 1 <= 13 && m[x, y + 1] < 0 && CountLiberty(m, x, y + 1) == 0)
            {
                Kill(m, x, y + 1);
                result = true;
            }
            return result;
        }

        protected void Kill(Matrix m, int x, int y)
        {
            bool flag = false;
            if (m[x, y] > 0)
            {
                flag = true;
            }
            bool[,] array = new bool[20, 20];
            array[x, y] = true;
            if (flag)
            {
                KillBlackStones(m, x, y, array);
            }
            else
            {
                KillWhiteStones(m, x, y, array);
            }
        }

        protected void KillBlackStones(Matrix m, int x, int y, bool[,] visited)
        {
            visited[x, y] = true;
            if (x - 1 >= 1 && !visited[x - 1, y] && m[x - 1, y] > 0)
            {
                KillBlackStones(m, x - 1, y, visited);
            }
            if (y - 1 >= 1 && !visited[x, y - 1] && m[x, y - 1] > 0)
            {
                KillBlackStones(m, x, y - 1, visited);
            }
            if (x + 1 <= 13 && !visited[x + 1, y] && m[x + 1, y] > 0)
            {
                KillBlackStones(m, x + 1, y, visited);
            }
            if (y + 1 <= 13 && !visited[x, y + 1] && m[x, y + 1] > 0)
            {
                KillBlackStones(m, x, y + 1, visited);
            }
            m[x, y] = 0;
            black_killed++;
        }

        protected void KillWhiteStones(Matrix m, int x, int y, bool[,] visited)
        {
            visited[x, y] = true;
            if (x - 1 >= 1 && !visited[x - 1, y] && m[x - 1, y] < 0)
            {
                KillWhiteStones(m, x - 1, y, visited);
            }
            if (y - 1 >= 1 && !visited[x, y - 1] && m[x, y - 1] < 0)
            {
                KillWhiteStones(m, x, y - 1, visited);
            }
            if (x + 1 <= 13 && !visited[x + 1, y] && m[x + 1, y] < 0)
            {
                KillWhiteStones(m, x + 1, y, visited);
            }
            if (y + 1 <= 13 && !visited[x, y + 1] && m[x, y + 1] < 0)
            {
                KillWhiteStones(m, x, y + 1, visited);
            }
            m[x, y] = 0;
            white_killed++;
        }
    }
}
