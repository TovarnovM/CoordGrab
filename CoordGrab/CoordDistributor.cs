using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CoordGrab {
    public class CoordDistributor {
        public static Vector NoPoint {
            get {
                return new Vector(-1,-1000);
            }
        }
        public static int Nminimum = 5;
        public List<IKnownCoords> goodBase;
        public List<UnknownCoords> UnknownClosePoints;

        public CoordDistributor(IEnumerable<IKnownCoords> crdenum) {
            goodBase = new List<IKnownCoords>(crdenum);
            UnknownClosePoints = new List<UnknownCoords>();
            for(int i = 0; i < goodBase.Count; i++) {
                UnknownClosePoints.Add(new UnknownCoords());
            }
            
        }

        //public int AddPoints(Vector[] points) {
        //    StatisticFormula.FDistribution()
        //}

        //public Vector[] GetLastCoords() {

        //}


    }

    public struct VectorTime {
        public Vector Vec;
        public double Time;
        public VectorTime(Vector vec, double time) {
            Vec = vec;
            Time = time;
        }
    }

    public class Otrezok {
        public int Signal;
        public double Tmin;
        public double Tmax;
        public double Length {
            get {
                return Tmax - Tmin;
            }
        }
        public Otrezok(int sgnal, double tmin, double tmax) {
            Signal = sgnal;
            Tmin = tmin;
            Tmax = tmax;
        }
        public Otrezok(int sgnal,double tmin):this(sgnal, tmin, tmin) {

        }
        public bool Belong(double t) {
            return t >= Tmin && t <= Tmax;
        }
    }
    public interface IUnknownCoords {
        double GetDistToLastPoint(Vector toPoint);
        void AddPoint(Vector point,double time);
        LinkedList<VectorTime> CoordList { get; }
        double Get_t1_param();
        Vector  GetCoord(double toTime,bool allowExtrapol = true);
    }

    public class UnknownCoords : IUnknownCoords {
        public LinkedList<VectorTime> CoordList { get; } = new LinkedList<VectorTime>();
        public VectorTime Last1 = new VectorTime(CoordDistributor.NoPoint, -1);
        public VectorTime PreLast1 = new VectorTime(CoordDistributor.NoPoint,-1);
        public virtual void AddPoint(Vector point,double time) {
            if(time <= CoordList?.Last?.Value.Time)
                throw new Exception("Прошлого не вернуть!");
            CoordList.AddLast(new VectorTime(point,time));
            if(point.X >= 0) {
                PreLast1 = Last1;
                Last1 = new VectorTime(point,time);


            }
                
        }

        public Vector GetCoord(double toTime,bool allowExtrapol = true) {
            if(toTime < CoordList.Last.Value.Time)
                throw new Exception("Прошлого не вернуть!");
            if(!allowExtrapol) {

                return Last1.Vec;
            }
            if(Last1.Vec.X < 0 || PreLast1.Vec.X < 0)
                return Last1.Vec;
            return (PreLast1.Vec * (Last1.Time - toTime) + Last1.Vec * (toTime - PreLast1.Time)) / (Last1.Time - PreLast1.Time);


        }

        public virtual double GetDistToLastPoint(Vector toPoint) {
            if(CoordList.Count == 0)
                return 0;
            return (CoordList.Last().Vec - toPoint).Length;
        }

        public double Get_t1_param() {
            int n1, n_all;
            n1 = 0;
            n_all = 0;
            foreach(var vt in CoordList) {
                if(vt.Vec.X >= 0)
                    n1++;
                n_all++;
            }
            if(n_all != 0)
                return (double)n1 / (double)n_all;
            return 0;
        }

    }

    public interface IKnownCoords: IUnknownCoords {
        double ProbOfSignal1ToTime(double toTime);
        double T1{ get; }
    }

    public class Signal_Const1 : UnknownCoords, IKnownCoords {
        public double T1 {
            get {
                return 1;
            }
        }

        public double ProbOfSignal1ToTime(double toTime) {
            return 1d;
        }
    }

    public class Signal_Blink01: UnknownCoords, IKnownCoords {
        public double Hz { get; set; }
        public double DeltaHz { get; set; }
        public double T1 { get; set; }
        private double l1min, l0min, l1max, l0max;
        private void InitConst() {
            l1min = T1 / (Hz + DeltaHz);
            l0min = (1 - T1) / (Hz + DeltaHz);
            l1max = T1 / (Hz - DeltaHz);
            l0max = (1 - T1) / (Hz - DeltaHz);
        }
        public LinkedList<Otrezok> OtrList = new LinkedList<Otrezok>();

        public Signal_Blink01(double hz,double dHz,double t1) {
            Hz = hz;
            DeltaHz = dHz;
            T1 = t1;
            InitConst();
        }
        public override void AddPoint(Vector point, double time) {
            //ToDo Добавить удаление старых точк и отрезков
            base.AddPoint(point,time);
            int signal = point.X < 0 ? 0 : 1;
            if(OtrList.Count == 0 || OtrList.Last.Value.Signal != signal) {
                OtrList.AddLast(new Otrezok(signal,time));

            } else if(time >= OtrList.Last.Value.Tmax) {
                OtrList.Last.Value.Tmax = time;
            } else
                throw new Exception("Прошлое не изменить!)");
            
        }
       
        /// <summary>
        /// Вероятность того, какой сигнал ожидать ко времни toTime
        /// </summary>
        /// <param name="toTime"></param>
        /// <returns>0, 1, 0.5</returns>
        public double ProbOfSignal1ToTime(double toTime) {
            if(CoordList.Count == 0)
                return 0.5;
            //if(toTime > OtrList.Last.Value.Tmax + l1max + l0max) //TODO уточнить
            //    return 0.5;
            //Определяем максимальную правую границу следующего "перехода"
            //левый конец последнего "участка" (0)
            var t0min_last = OtrList.Last.Value.Tmin;
            //максимально возможная крайняя точка окончания участка
            var t_prav = t0min_last + (OtrList.Last.Value.Signal == 1 ? l1max : l0max);
            //но, возможно, она немного левее:
            var currOtr = OtrList.Last.Previous;
                
            while(currOtr != null) {
                var lmax = currOtr.Value.Signal == 1 ? l1max : l0max;
                if(t0min_last - currOtr.Value.Tmin > lmax) {
                    var shift = t0min_last - currOtr.Value.Tmin - lmax;
                    t_prav -= shift;
                    t0min_last = currOtr.Value.Tmin;
                    currOtr = currOtr.Previous;
                } else {
                    break;
                }      
            }

            //Определяем максимальную Лвую границу следующего "перехода"
            //правый конец последнего "участка" (0)
            var t0max_last = OtrList.Last.Value.Tmax;
            //максимально возможная левая крайняя точка окончания провала
            var t_lev = t0max_last;
            //но, возможно, она немного праве:
            currOtr = OtrList.Last.Previous;
            //1 цикл, потом переделать без wile
            if(currOtr != null) {
                var lmin = currOtr.Value.Signal == 1 ? l1min : l0min;
                if(t0max_last - currOtr.Value.Tmax < lmin) {
                    var shift = lmin - (t0max_last - currOtr.Value.Tmin) ;
                    t_lev += shift;
                } 
            }

            int shifter = OtrList.Last.Value.Signal == 1 ? 1 : 0;
            var p_left = PerfectMixSignal(toTime - t_lev,  l1min,l0min,-l1min * shifter,
                                                           l1max,l0max,-l1max * shifter);
            var p_right = PerfectMixSignal(toTime - t_prav,l1min,l0min,-l1min * shifter,
                                                           l1max,l0max,-l1max * shifter);
            if(Math.Abs(p_left - p_right) < 0.01)
                return p_left;

            return 0.5;
        }

        /// <summary>
        /// Дает точное значение сигнала ко времени t
        /// </summary>
        /// <param name="t">время, для которого измеряется сигнал</param>
        /// <param name="l1">длина площадки сигнала 1</param>
        /// <param name="l0">длина впадины сигнала 0</param>
        /// <param name="shift">смещение начала площадки"1" вправо, относительно t=0</param>
        /// <returns>0 или 1</returns>
        public static int PerfectSignal(double t, double l1, double l0, double shift) {
            return PerfectSignalBool(t,l1,l0,shift) ? 1 : 0;

        }

        public static bool PerfectSignalBool(double t,double l1,double l0,double shift) {
            double first = shift;
            double interval = l1 + l0;
            t = t - Math.Floor((t - first) / interval) * interval - first;
            return t <= l1;
        }

        public static double PerfectMixSignal(double t,double l1_1,double l0_1,double shift_1,
                                                       double l1_2,double l0_2,double shift_2) {
            double first_1 = shift_1;
            double interval_1 = l1_1 + l0_1;
            int n_1 = (int)Math.Floor((t - first_1) / interval_1);

            double first_2 = shift_1;
            double interval_2 = l1_1 + l0_1;
            int n_2 = (int)Math.Floor((t - first_2) / interval_2);

            if(n_1 != n_2)
                return 0.5;

            var t_1 = t - n_1 * interval_1 - first_1;            
            var t_2 = t - n_2 * interval_2 - first_2;
            double s_1 = t_1 <= l1_1 ? 1.0 : 0.0;
            double s_2 = t_2 <= l1_2 ? 1.0 : 0.0;
            return (s_1 + s_2) * 0.5;


        }

    }
}
