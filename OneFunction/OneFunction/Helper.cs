using System;
using System.Collections.Generic;
using System.Text;

namespace OneFunction
{
    static class Helper
    {
        public static void ShowMatrix(double[][] M)
        {
            for (int i = 0; i < M.GetLength(0); i++)
            {
                for (int j = 0; j < M[i].Length; j++)
                {
                    Console.Write("{0:0.0000} ", M[i][j]);
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        public static double[][] GetProduct(double[][] Left, double[][] Right)
        {
            int N = Left.GetLength(0);
            double[][] P = new double[N][];
            for (int k = 0; k < N; ++k)
            {
                P[k] = new double[N];
            }

            for (int i = 0; i < N; ++i)
            {
                for (int j = 0; j < N; ++j)
                {
                    P[i][j] = 0.0;
                    for (int k = 0; k < N; ++k)
                    {
                        P[i][j] += Left[i][k] * Right[k][j];
                    }
                }
            }
            return P;
        }

        public static double[][] GetXTX(double[][] X)
        {
            int rows = X.GetLength(0);
            int cols = X[0].Length;
            if (rows < cols)
            {
                Console.WriteLine("Fatal: matrix misformatted");
                Environment.Exit(0);
            }
            double[][] XTX = new double[cols][];
            for (int i = 0; i < cols; ++i)
            {
                XTX[i] = new double[cols];
            }

            for (int i = 0; i < cols; ++i)
            {
                for (int j = 0; j < cols; ++j)
                {
                    XTX[i][j] = 0.0;
                    for (int k = 0; k < rows; ++k)
                    {
                        XTX[i][j] += X[k][i] * X[k][j];
                    }
                }
            }
            return XTX;
        }

        public static double[][] Pseudo(double[][] X, double[][] Inv)
        {
            int rows = X.GetLength(0);
            int cols = X[0].Length;
            if (rows < cols)
            {
                Console.WriteLine("Fatal: matrix misformatted");
                Environment.Exit(0);
            }
            if (cols != Inv.GetLength(0))
            {
                Console.WriteLine("Fatal: matrix misformatted");
                Environment.Exit(0);
            }
            if (cols != Inv[0].Length)
            {
                Console.WriteLine("Fatal: matrix misformatted");
                Environment.Exit(0);
            }

            double[][] P = new double[rows][];
            for (int i = 0; i < rows; ++i)
            {
                P[i] = new double[cols];
            }
            for (int i = 0; i < rows; ++i)
            {
                for (int j = 0; j < cols; ++j)
                {
                    P[i][j] = 0.0;
                    for (int k = 0; k < cols; ++k)
                    {
                        P[i][j] += X[i][k] * Inv[j][k];
                    }
                }
            }
            return P;
        }

        public static double[][] TwoRectangular(double[][] X, double[][] Y)
        {
            int rows = X.GetLength(0);
            int cols = X[0].Length;
            if (rows < cols)
            {
                Console.WriteLine("Fatal: matrix misformatted");
                Environment.Exit(0);
            }
            if (rows != Y.GetLength(0))
            {
                Console.WriteLine("Fatal: matrix misformatted");
                Environment.Exit(0);
            }
            if (cols != Y[0].Length)
            {
                Console.WriteLine("Fatal: matrix misformatted");
                Environment.Exit(0);
            }

            double[][] Res = new double[cols][];
            for (int i = 0; i < cols; ++i)
            {
                Res[i] = new double[cols];
            }
            for (int i = 0; i < cols; ++i)
            {
                for (int j = 0; j < cols; ++j)
                {
                    Res[i][j] = 0.0;
                    for (int k = 0; k < rows; ++k)
                    {
                        Res[i][j] += X[k][i] * Y[k][j];
                    }
                }
            }
            return Res;
        }

        public static double[][] PseudoInverse(double[][] X)
        {
            double[][] XTX = GetXTX(X);
            SplineGenerator sg = new SplineGenerator();
            double[][] Inv = sg.MatInverseQR(XTX);
            double[][] P = Pseudo(X, Inv);
            return P;
        }

        public static void ShowBasisValues(Basis basis)
        {
            int N = 10;
            for (int i = 0; i < basis.splines.Count - 1; i++)
            {
                for (int j = 0; j < N; ++j)
                {
                    double dist = j * 1.0 / N;
                    double point = basis.GetValue(i, dist);

                    Console.WriteLine("{0:0.0000} ", point);
                }
            }
        }
    }
}
