using System;
using System.Collections.Generic;
using System.Text;

namespace OneFunction
{
    internal class Spline
    {
        private double a, b, c, d;

        public double GetValue(double x)
        {
            return a + b * x + c * x * x + d * x * x * x;
        }

        public double GetDerivative(double x)
        {
            return b + 2.0 * c * x + 3.0 * d * x * x;
        }

        public Spline(double A, double B, double C, double D)
        {
            this.a = A; this.b = B; this.c = C; this.d = D;
        }
    }

    internal class Basis
    {
        public List<Spline> splines = new List<Spline>();

        public void AddSpline(double A, double B, double C, double D)
        {
            splines.Add(new Spline(A, B, C, D));
        }

        public double GetDerivative(int spline, double relativeDistance)
        {
            return splines[spline].GetDerivative(relativeDistance);
        }

        public double GetValue(int spline, double relativeDistance)
        {
            return splines[spline].GetValue(relativeDistance);
        }
    }

    internal class Univariate
    {
        private int _points;
        List<Basis> _basisList = new List<Basis>();
        double[] _coefficients = null;
        double _xmin;
        double _xmax;
        double _ymin;
        double _ymax;
        double _deltax;
        Random _rnd = new Random();

        public Univariate(double xmin, double xmax, double ymin, double ymax, int points)
        {
            _points = points;
            _xmin = xmin;
            _xmax = xmax;
            _xmin -= 0.01 * (_xmax - _xmin);
            _xmax += 0.01 * (_xmax - _xmin);
            _deltax = (_xmax - _xmin) / (_points - 1);
            _ymin = ymin;
            _ymax = ymax;
            Initialize();
        }

        public void UpdateModelByResidual(double[] residual, double[] x)
        {
            //Make pseudoinverse
            int nRecords = residual.Length;
            double[][] B = new double[nRecords][];
            for (int i = 0; i < nRecords; ++i)
            {
                double[] b = GetBasicVector(x[i]);
                B[i] = new double[_points];
                for (int j = 0; j < _points; ++j)
                {
                    B[i][j] = b[j];
                }
            }
            double[][] P = Helper.PseudoInverse(B);

            //Update coefficients
            double[] C = new double[_points];
            for (int i = 0; i < _points; ++i)
            {
                C[i] = 0.0;
                for (int j = 0; j < nRecords; ++j)
                {
                    C[i] += P[j][i] * residual[j];
                }
            }
            UpdateDirect(C, 1.0);
        }

        public void UpdateDirect(double[] c, double mu)
        {
            for (int i = 0; i < _basisList.Count; i++)
            {
                _coefficients[i] += c[i] * mu;
            }
        }

        public double[] GetBasicVector(double x)
        {
            (int k, double relative) = GetSplineAndRelative(x);
            List<double> v = new List<double>();

            for (int i = 0; i < _basisList.Count; i++)
            {
                v.Add(_basisList[i].GetValue(k, relative));
            }
            return v.ToArray();
        }

        public double GetDerrivative(double x)
        {
            FitDefinition(x);
            (int k, double relative) = GetSplineAndRelative(x);

            double v = 0.0;
            for (int i = 0; i < _basisList.Count; i++)
            {
                v += _basisList[i].GetDerivative(k, relative) * _coefficients[i] / _deltax;
            }
            return v;
        }

        private void PopulateBasisFunctions(SplineGenerator sg, double[][] R, double[] h)
        {
            for (int i = 0; i < _points; ++i)
            {
                double[] e = new double[_points];
                for (int j = 0; j < _points; ++j)
                {
                    e[j] = 0.0;
                }
                e[i] = 1.0;

                (double[] a, double[] b, double[] c, double[] d) = sg.MakeSplines(R, e, h);

                Basis basis = new Basis();
                for (int j = 0; j < a.Length; ++j)
                {
                    basis.AddSpline(a[j], b[j], c[j], d[j]);
                }
                _basisList.Add(basis);
            }
        }

        private void InitializeCoefficients()
        {
            _coefficients = new double[_basisList.Count];
            for (int i = 0; i < _coefficients.Length; ++i)
            {
                _coefficients[i] = _rnd.Next(10, 1000) / 1000.0 * (_ymax - _ymin) + _ymax;
                _coefficients[i] /= _coefficients.Length;
            }
        }

        private void FitDefinition(double x)
        {
            if (x < _xmin)
            {
                x = x - 0.01 * (_xmax - _xmin);
                _deltax = (_xmax - x) / (_points - 1);
                _xmin = x;
            }
            else if (x > _xmax)
            {
                x = x + 0.01 * (_xmax - _xmin);
                _deltax = (x - _xmin) / (_points - 1);
                _xmax = _xmin + (_points - 1) * _deltax;
            }
        }

        public (int k, double relative) GetSplineAndRelative(double x)
        {
            int k = (int)((x - _xmin) / _deltax);
            if (k > _points - 2) k = _points - 2;
            double relative = (x - (_xmin + _deltax * k)) / _deltax;
            return (k, relative);
        }

        public double GetFunctionValue(double x)
        {
            FitDefinition(x);

            (int k, double relative) = GetSplineAndRelative(x);  

            double v = 0.0;
            for (int i = 0; i < _basisList.Count; i++)
            {
                v += _basisList[i].GetValue(k, relative) * _coefficients[i];
            }
            return v;
        }

        public void Update(double x, double delta, double mu)
        {
            FitDefinition(x);

            (int k, double relative) = GetSplineAndRelative(x);

            for (int i = 0; i < _basisList.Count; i++)
            {
                _coefficients[i] += delta * mu * _basisList[i].GetValue(k, relative);
            }
        }

        private void Initialize()
        { 
            SplineGenerator sg = new SplineGenerator();
            double[] h = new double[_points - 1];
            for (int i = 0; i < h.Length; i++)
            {
                h[i] = 1.0;
            }
            double[][] M = sg.GenerateTriDiagonal(_points, h);
            double[][] R = sg.MatInverseQR(M);
            PopulateBasisFunctions(sg, R, h);
            InitializeCoefficients();
        }
    }
}
