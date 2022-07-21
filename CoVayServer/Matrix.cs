using System;

namespace CoVayServer
{
    public class Matrix
    {
        public int[,] matrix;

        public int this[int i, int j]
        {
            get { return matrix[i, j]; }
            set { matrix[i, j] = value; }
        }

        public Matrix()
        {
            int n = ConstNumber.linenum + 1;
            matrix = new int[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    matrix[i, j] = 0;
                }
            }
        }

        public Matrix Copy()
        {
            Matrix m = new Matrix();
            int n = ConstNumber.linenum;
            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= n; j++)
                {
                    m[i, j] = this[i, j];
                }
            }
            return m;
        }

        public bool EqualSituationWith(Matrix m)
        {
            int n = ConstNumber.linenum + 1;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    int a = this[i, j];
                    int b = m[i, j];
                    if (a != 0)
                    {
                        a = a / Math.Abs(a);
                    }

                    if (b != 0)
                    {
                        b = b / Math.Abs(b);
                    }

                    if (a != b)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
