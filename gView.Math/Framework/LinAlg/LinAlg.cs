using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Framework.LinAlg
{
    public class LinearEquation2
    {
        private double _l0, _l1, _a00, _a01, _a10, _a11;
        private double t1 = double.NaN, t2 = double.NaN;

        public LinearEquation2(double l0, double l1, double a00, double a01, double a10, double a11)
        {
            _l0 = l0;
            _l1 = l1;
            _a00 = a00;
            _a01 = a01;
            _a10 = a10;
            _a11 = a11;
        }

        public bool Solve()
        {
            double detA = Det.Calc22(_a00, _a01, _a10, _a11);
            if (detA == 0.0) return false;

            t1 = Det.Calc22(_l0, _a01,
                            _l1, _a11) / detA;
            t2 = Det.Calc22(_a00, _l0,
                            _a10, _l1) / detA;
            return true;
        }

        public double Var1
        {
            get { return t1; }
        }
        public double Var2
        {
            get { return t2; }
        }
    }

    public class LinearEquation3
    {
        private double[] _l = new double[3], _c1 = new double[3], _c2 = new double[3], _c3 = new double[3];
        private double t1 = double.NaN, t2 = double.NaN, t3 = double.NaN;

        public void SetRow(int row, double l, double c1, double c2, double c3)
        {
            if (row < 0 || row >= 3)
                return;

            _l[row] = l;
            _c1[row] = c1;
            _c2[row] = c2;
            _c3[row] = c3;
        }

        public bool Solve()
        {
            double detA = Det.Calc33(_c1[0], _c2[0], _c3[0],
                                     _c1[1], _c2[1], _c3[1],
                                     _c1[2], _c2[2], _c3[2]);

            if (detA == 0.0) return false;

            t1 = Det.Calc33(_l[0], _c2[0], _c3[0],
                            _l[1], _c2[1], _c3[1],
                            _l[2], _c2[2], _c3[2]) / detA;

            t2 = Det.Calc33(_c1[0], _l[0], _c3[0],
                            _c1[1], _l[1], _c3[1],
                            _c1[2], _l[2], _c3[2]) / detA;

            t3 = Det.Calc33(_c1[0], _c2[0], _l[0],
                            _c1[1], _c2[1], _l[1],
                            _c1[2], _c2[2], _l[2]) / detA;

            return true;
        }

        public double Var1
        {
            get { return t1; }
        }
        public double Var2
        {
            get { return t2; }
        }
        public double Var3
        {
            get { return t3; }
        }
    }

    public class Det
    {
        static public double Calc22(double a00, double a01, double a10, double a11)
        {
            return a00 * a11 - a01 * a10;
        }

        static public double Calc33(double a11, double a12, double a13,
                                    double a21, double a22, double a23,
                                    double a31, double a32, double a33)
        {
            return a11 * a22 * a33 +
                   a12 * a23 * a31 +
                   a13 * a21 * a32 -
                   a13 * a22 * a31 -
                   a12 * a21 * a33 -
                   a11 * a23 * a32;
        }
    }
}
