using Interpolator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoordGrab {
    public class RndFunction {
        public Func<double,double> Func { get; set; }
        public double Shag { get; set; }
        public double SKOx3 { get; set; }
        public Func<double,double> SKOx3_Funct { get; set; }
        public double SKOx3dT { get; set; }
        public MyRnd Random { get; set; }
        public RndFunction(Func<double,double> f,Func<double,double> skoF, double dt,double skoX3_dt = 0d) {
            Func = f;
            Shag = dt;
            SKOx3 = skoF(0);
            SKOx3_Funct = skoF;
            SKOx3dT = skoX3_dt;
            Random = new MyRnd();
        }
        public RndFunction(Func<double,double> f, double skoX3, double dt, double skoX3_dt = 0d) {
            Func = f;
            Shag = dt;
            SKOx3 = skoX3;
            SKOx3_Funct = t => SKOx3;
            SKOx3dT = skoX3_dt;
            Random = new MyRnd();
        }
        public InterpXY GetInterp(double fromT, double toT) {
            var res = new InterpXY();
            double t = fromT;
            while(t < toT) {
                res.Add(t,Random.GetNorm(Func(t),SKOx3_Funct(t) / 3));
                double currShag;
                do {
                    currShag = Random.GetNorm(Shag,SKOx3dT / 3);
                } while(currShag <= 0);
                t += Shag;
            }
            t = toT;
            res.Add(t,Random.GetNorm(Func(t),SKOx3_Funct(t) / 3));
            return res;
        }


    }
}
