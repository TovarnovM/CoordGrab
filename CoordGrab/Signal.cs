using Interpolator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoordGrab {
    [Serializable]
    public class Signal {
        public InterpXY OneHzImpulse { get; set; }
        public double Hz0 { get; set; }
        public double Hz { get; set; }
        public double Amplitude { get; set; }
        public double Shift { get; set; }
        public double TimeCurr { get; set; }
        public string Name { get; set; }
        public Signal() : this(1) { }
        public Signal(double hz0, string name = "") {
            SetRectangleImpulse(0.5);
            Name = name;
            Hz0 = hz0;
            Amplitude = 1d;
            Reset();
        }
        public double GetAmplitudeDt(double dt, double dHz, bool lineHzdifference = true) {
            if(lineHzdifference) {
                var t1Hz_nophaseLine = 0.5*dt * (2*Hz + dHz) + Shift;
                Shift = t1Hz_nophaseLine % 1d;
                Hz += dHz;
                return OneHzImpulse[t1Hz_nophaseLine] * Amplitude;
            }
            Hz += dHz;
            var t1Hz_nophase = dt * Hz + Shift;
            Shift = t1Hz_nophase % 1d;
            return OneHzImpulse[t1Hz_nophase] * Amplitude;
        }
        public double GetAmplitudeDt(double dt) {
            return GetAmplitudeDt(dt,0d,false);
        }
        public double GetAmp(double t, double hz, bool lineHzdifference = true) {
            var res = GetAmplitudeDt(t - TimeCurr,hz - Hz,lineHzdifference);
            TimeCurr = t;
            Hz = hz;
            return res;
        }
        public double GetAmp(double t) {
            return GetAmp(t,Hz);
        }
        public void Reset() {
            Hz = Hz0;
            Shift = 0d;
            TimeCurr = 0d;
            
        }
        public void SetRectangleImpulse(double t1) {
            OneHzImpulse = RectangleImpulse(t1);
        }
        public void SetTrapecImpulse(double t1,double t2,double t3) {
            OneHzImpulse = TrapecImpulse(t1,t2,t3);
        }
        public void SetSinImpulse(int nPoints = 1000) {
            OneHzImpulse = SinImpulse(nPoints);
        }
        public static InterpXY RectangleImpulse(double t1) {
            var res = new InterpXY();
            res.ET_left = ExtrapolType.etRepeat;
            res.ET_right = ExtrapolType.etRepeat;
            res.InterpType = InterpolType.itStep;
            if(t1 >= 1d) {
                res.Add(0d,1d);
                res.Add(1d,1d);
                return res;
            }
            res.Add(0d,1d);
            res.Add(t1,0d);
            res.Add(1d,0d);
            return res;
        }
        public static InterpXY TrapecImpulse(double t1,double t2,double t3) {
            var max = Math.Max(t1,Math.Max(t2,t3));
            if(max >= 1d) {
                t1 /= max;
                t2 /= max;
                t3 /= max;
            }
            var res = new InterpXY();
            res.ET_left = ExtrapolType.etRepeat;
            res.ET_right = ExtrapolType.etRepeat;
            res.Add(0d,0d);
            res.Add(t1,1d);
            res.Add(t2,1d);
            res.Add(t3,0d);
            res.Add(1d,0d);
            return res;
        }
        public static InterpXY SinImpulse(int nPoints = 1000) {
            var res = new InterpXY();
            res.ET_left = ExtrapolType.etRepeat;
            res.ET_right = ExtrapolType.etRepeat;
            double dt = 1d / nPoints;
            double t = 0d;
            for(int i = 0; i < nPoints; i++) {
                res.Add(t,Math.Sin(t * (2 * Math.PI)));
                t += dt;
            }
            return res;
        }
        public InterpXY GetInterpSignal(double fromT,double toT, int nPoints) {
            return GetInterpSignal(fromT,toT,(toT - fromT) / nPoints);
        }
        public InterpXY GetInterpSignal(double fromT, double toT, double dt) {
            var pnts = new List<double>((int)((toT - fromT) / dt)+2);
            while(fromT < toT) {
                pnts.Add(fromT);
                fromT += dt;
            }
            pnts.Add(toT);
            return GetInterpSignal(pnts);
        }
        public virtual InterpXY GetInterpSignal(IEnumerable<double> pointsTime) {
            var res = new InterpXY();
            foreach(var t in pointsTime) {
                res.Add(t,GetAmp(t));
            }
            return res;
        }
    }
    [Serializable]
    public class FuzzySignal : Signal {
        public double  deltaHz { get; set; }
        public InterpXY InterpHz { get; set; }
        public bool GenerateNewHzFunct { get; set; } = true;
        private RndFunction s_rnd;
        public FuzzySignal() : this(1,0) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hz0"></param>
        /// <param name="deltaHz">+ -</param>
        /// <param name="name"></param>
        public FuzzySignal(double hz0, double deltaHz, string name = "") : base(hz0,name) {
            s_rnd = new RndFunction(t => Hz0,deltaHz,1d / Hz0,0.7d / Hz0);
            Shift = s_rnd.Random.GetDouble();
        }
        public override InterpXY GetInterpSignal(IEnumerable<double> pointsTime) {
            var res = new InterpXY();             
            InterpHz = GenerateNewHzFunct || InterpHz == null ? s_rnd.GetInterp(pointsTime.Min(),pointsTime.Max()) : InterpHz;

            
            foreach(var t in pointsTime) {
                res.Add(t,GetAmp(t,InterpHz[t]));
            }
            return res;
        }

    }
}
