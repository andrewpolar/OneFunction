//This is iterative identification of the function represented by values.
//This method is valid for identification of discrete Urysohn operators and
//Kolmogorov-Arnold representation.
//Developed by Andrew Polar and Mike Poluektov.
//Published:
//https://www.sciencedirect.com/science/article/abs/pii/S0016003220301149
//https://www.sciencedirect.com/science/article/abs/pii/S0952197620303742
//https://arxiv.org/abs/2305.08194

//Usual execution print out for Option 1
//epoch 0, error 0.066522
//epoch 1, error 0.007795
//epoch 2, error 0.001350
//epoch 3, error 0.000562
//epoch 4, error 0.000528
//epoch 5, error 0.000528
//epoch 6, error 0.000528
//epoch 7, error 0.000528
//Relative error for unseen data 0.000428

using System;
using System.Linq;
using System.Collections.Generic;

namespace OneFunction
{
    internal class Program
    {
        static double Function(double x)
        {
            const double pi = 3.14;
            return Math.Sin(pi * x) + Math.Sin(pi * (2.0 * x + 2.0)); 
        }

        static (double[] x, double[] y) GetSample(int N, double xmin, double xmax)
        {
            Random rand = new Random();
            double[] x = new double[N];
            double[] y = new double[N];
            for (int i = 0; i < N; ++i)
            {
                x[i] = rand.Next(10, 1000) / 1000.0 * (xmax - xmin) + xmin;
                y[i] = Function(x[i]);
            }
            return (x, y);
        }

        static void Main(string[] args)
        {
            //Generate data
            (double[] x, double[] y) = GetSample(2000, 0.5, 1.5);
            double xmin = x.Min();
            double xmax = x.Max();
            double ymin = y.Min();
            double ymax = y.Max();
            int nRecords = x.Length;

            //Identification
            Univariate uv = new Univariate(xmin, xmax, ymin, ymax, 10);

            //Option 1: iterative Kaczmarz
            for (int step = 0; step < 8; ++step)
            {
                double error = 0.0;
                int cnt = 0;
                for (int i = 0; i < x.Length; ++i)
                {
                    double v = uv.GetFunctionValue(x[i]);
                    double diff = Function(x[i]) - v;
                    uv.Update(x[i], diff, 0.05);
                    error += diff * diff;
                    ++cnt;
                }
                error /= cnt;
                error = Math.Sqrt(error);
                error /= (ymax - ymin);
                Console.WriteLine("epoch {0}, error {1:0.000000}", step, error);
            }

            //Option 2
            //Build residual vector
            //double residual_error = 0.0;
            //double[] residual = new double[nRecords];
            //for (int i = 0; i < nRecords; ++i)
            //{
            //    double m = uv.GetFunctionValue(x[i]);
            //    double diff = Function(x[i]) - m;
            //    residual[i] = diff;
            //    residual_error += diff * diff;
            //}
            //residual_error /= nRecords;
            //residual_error = Math.Sqrt(residual_error);
            //residual_error /= (ymax - ymin);
            //Console.WriteLine("Relative error for initial approximation {0:0.0000}", residual_error);

            //uv.UpdateModelByResidual(residual, x);

            //Validation
            (double[] x_test, double[] y_test) = GetSample(100, 0.5, 1.5);
            double error_test = 0.0;
            int cnt_test = 0;
            for (int i = 0; i < x_test.Length; ++i)
            {
                double v = uv.GetFunctionValue(x[i]);
                double diff = Function(x[i]) - v;
                error_test += diff * diff;
                ++cnt_test;
            }
            error_test /= cnt_test;
            error_test = Math.Sqrt(error_test);
            error_test /= (ymax - ymin);
            Console.WriteLine("Relative error for unseen data after identification {0:0.000000}", error_test);
        }
    }
}
