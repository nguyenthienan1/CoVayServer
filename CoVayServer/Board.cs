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
        public Matrix matrix;
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

        public bool SetStone(bool isblack, int x, int y)
        {
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

            if (!KillStone(m, x, y) && CountLiberty(m, x, y) == 0)
            {
                return false;
            }
            if (StoneNum - 1 >= 0 && m.EqualSituationWith(matrix))
            {
                return false;
            }

            StoneNum++;
            matrix = m;
            return true;
        }

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
