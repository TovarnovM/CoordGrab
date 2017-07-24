using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CoordGrab {
    public class CoordDistributor {
        private struct SpornajzTochka {
            public Vector point;
            public double time;
            public int MyProperty { get; set; } 
            public IEnumerable<IKnownCoords> Candidates;
            public SpornajzTochka(Vector p, double t,IEnumerable<IKnownCoords> candidates) {
                point = p;
                time = t;
                Candidates = candidates;
                MyProperty = 77;
            }
        }
        private Random rnd;
        public static Vector NoPoint {
            get {
                return new Vector(-1,-1000);
            }
        }
        public static int Nminimum = 5;
        public double AdoptRadius = 3;
        public double TooLatePeriod = 0.3;
        public List<IKnownCoords> GoodBase;
        public List<UnknownCoords> UnknownClosePoints;
        public List<VectorTime> AbsoluteUnknownPoints;

        public CoordDistributor(IEnumerable<IKnownCoords> crdenum) {
            GoodBase = new List<IKnownCoords>(crdenum);
            UnknownClosePoints = new List<UnknownCoords>();
            AbsoluteUnknownPoints = new List<VectorTime>();
            rnd = new Random();
        }

        public int AddPoints(double t, Vector[] points) {
            GoodBase.ClearOldData(t, TooLatePeriod);
            UnknownClosePoints.ClearOldData(t,TooLatePeriod);
            //точки, которые не подошли идеально
            var unknownPoints = new List<Vector>(points.Length);
            //спорные точки, которые подошли больше, чем 1 известным координатам
            var sporniePoints = new List<SpornajzTochka>(points.Length);
            //список идентицицированных прицелов, неполучивших реальную точку
            var nepoluchivshie = new List<IKnownCoords>(GoodBase);
            //список идентицицированных прицелов, получивших реальную точку
            var poluchivshie = new List<IKnownCoords>(GoodBase.Count);
            //список неидентицицированных прицелов, неполучивших реальную точку
            var nepoluchivshieUnknown = new List<IUnknownCoords>(UnknownClosePoints);
            #region принятие идеальных точек
            foreach(var point in points) {
                //список идентицицированных прицелов, которым подошла текущая точка
                var okCoords =
                    GoodBase.
                    Where(np => np.CoordList.Count > 0).
                    Where(np => np.GetDistToLast1Point(point) <= AdoptRadius).
                    ToList();
                switch(okCoords.Count) {
                    case 1: {
                        //идеальный случай
                        okCoords[0].AddPoint(point,t);
                        nepoluchivshie.Remove(okCoords[0]);
                        poluchivshie.Add(okCoords[0]);
                        break;
                    }
                    case 0: {
                        //точка находится вдалике от идентицицированных прицелов
                        unknownPoints.Add(point);
                        break;
                    }
                    default: {
                        //неподелили
                        sporniePoints.Add(new SpornajzTochka(point,t,okCoords));
                        break;
                    }               
                }
            }
            #endregion

            #region "разруливание" спорных ситуаций
            foreach(var sp in sporniePoints) {
                //исключаем из кандидатов прицелы, которые уже получили свою точку
                var tochnoSpornye = sp.Candidates.Except(poluchivshie).ToArray();
                switch(tochnoSpornye.Length) {
                    case 1: {
                        //не с кем спорить, получай
                        tochnoSpornye[0].AddPoint(sp.point,t);
                        nepoluchivshie.Remove(tochnoSpornye[0]);
                        poluchivshie.Add(tochnoSpornye[0]);
                        break;
                    }
                    case 0: {
                        //опять непонятно что за точка
                        unknownPoints.Add(sp.point);
                        break;
                    }
                    default: {
                        //выясняем, чья это точка скорее всего
                        var maxGroup = tochnoSpornye.Where(c => c.ProbOfSignal1ToTime(t) > 0).ToList();//OrderBy(c => c.ProbOfSignal1ToTime(t))
                        if(maxGroup.Count > 0) {
                            foreach(var gp in maxGroup) {

                                gp.AddPoint(sp.point,t,true);
                                nepoluchivshie.Remove(gp);
                                poluchivshie.Add(gp);
                                //break;

                            }
                        } else {
                            //никто сейчас не может светить => это опять ничья точка
                            unknownPoints.Add(sp.point);
                        }
                        break;
                    }
                }


            }
            #endregion

            #region принятие близких точек к группам пока еще неизвестных
            var novie = new List<UnknownCoords>();
            foreach(var upoint in unknownPoints) {
                //список неидентицицированных прицелов, которым подошла текущая точка
                var okCoords =
                    UnknownClosePoints.
                    Where(np => np.CoordList.Count > 0).
                    Where(np => np.GetDistToLast1Point(upoint) <= AdoptRadius).
                    ToList();
                switch(okCoords.Count) {
                    case 1: {
                        //идеальный случай
                        okCoords[0].AddPoint(upoint,t);
                        nepoluchivshieUnknown.Remove(okCoords[0]);
                        break;
                    }
                    case 0: {
                        //чтожжж.... создаем новый неидентицицированных прицел
                        var newbe = new UnknownCoords();
                        newbe.AddPoint(upoint,t);
                        novie.Add(newbe);
                        break;
                    }
                    default: {
                        //всем по спорной точке!
                        foreach(var ucp in okCoords) {
                            ucp.AddPoint(upoint,t);
                            nepoluchivshieUnknown.Remove(ucp);
                        }
                        break;
                    }
                }
            }
            UnknownClosePoints.AddRange(novie);
            #endregion

            #region Добавление "пустых" точек к последовательностям
            foreach(var npU in nepoluchivshieUnknown) {
                npU.AddPoint(NoPoint,t);
            }
            foreach(var np in nepoluchivshie.Where(np => np.CoordList.Count > 3)) {
                np.AddPoint(NoPoint,t);
            }
            #endregion

            #region Если не хватает известных координат

            int Nposl = Nminimum * 10;
            var mootSeq = GoodBase.Where(np => np.CoordList.Reverse().Take(Nposl).Any(p => p.MootVec) || np.CoordList.Count == 0).ToList();
            var notGood = mootSeq.Where(g => Math.Abs(g.Get_t1_param(Nposl) - g.T1) > g.T1_tol).ToList();
            foreach(var ng in notGood) {
                var un = new UnknownCoords();
                UnknownClosePoints.Add(un);
                foreach(var p in ng.CoordList) {
                    un.AddPoint(p.Vec,p.Time,p.MootVec);
                }
                ng.ClearData();
            }
            if(notGood.Count == 0 && UnknownClosePoints.Count == 0)
                return 1;



            var empty = GoodBase.Where(np => np.CoordList.Count == 0).ToList();

            var coincidence = new List<List<UnknownCoords>>(empty.Count);
            for(int i = 0; i < empty.Count; i++) {
                coincidence.Add(new List<UnknownCoords>());
            }
            foreach(var ucoord in UnknownClosePoints) {
                if(ucoord.CoordList.Count < Nminimum*10)
                    continue;
                var t1 = ucoord.Get_t1_param(Nposl);
                for(int i = 0; i < empty.Count; i++) {
                    if(Math.Abs(empty[i].T1 - t1) < empty[i].T1_tol) {
                        coincidence[i].Add(ucoord);
                    }
                }
            }
            for(int i = 0; i < empty.Count; i++) {
                if(coincidence[i].Count == 1) {
                    empty[i].ClearData();
                    foreach(var vt in coincidence[i][0].CoordList) {
                        empty[i].AddPoint(vt.Vec,vt.Time,vt.MootVec);
                    }
                    UnknownClosePoints.Remove(coincidence[i][0]);
                }
            }
            #endregion

            if(UnknownClosePoints.Any(uc => uc.CoordList.Count > 300))
                return 7;
            return 1;
        }


        public Vector[] GetLastCoords(bool allowExtrapol = true) {
            var res = new Vector[GoodBase.Count];
            for(int i = 0; i < GoodBase.Count; i++) {
                res[i] = GoodBase[i].GetCoord(allowExtrapol);
            }
            return res;
        }


    }

    public struct VectorTime {
        public Vector Vec;
        public double Time;
        public bool MootVec;
        public VectorTime(Vector vec, double time, bool mootVec = false) {
            Vec = vec;
            Time = time;
            MootVec = mootVec;
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
        double GetDistToLast1Point(Vector toPoint);
        double GetLastTime1();
        void AddPoint(Vector point,double time, bool mootPoint = false);
        LinkedList<VectorTime> CoordList { get; set; }
        double Get_t1_param(int nTau = -1, bool countMoot = true);
        Vector  GetCoord(double toTime,bool allowExtrapol = true);
        Vector GetCoord(bool allowExtrapol = true);
        void ClearData();
        void MoveDataTo(IUnknownCoords toMe);
        void SetLast1(VectorTime last1);
        void SetPreLast1(VectorTime preLast1);
    }

    public class UnknownCoords : IUnknownCoords {
        public int MaxPoints { get; set; } = 100;
        public LinkedList<VectorTime> CoordList { get; set; } = new LinkedList<VectorTime>();
        public VectorTime Last1 = new VectorTime(CoordDistributor.NoPoint, -1);
        public VectorTime PreLast1 = new VectorTime(CoordDistributor.NoPoint,-1);
        public virtual void AddPoint(Vector point,double time,bool mootPoint = false) {
            if(time <= CoordList?.Last?.Value.Time)
                return;
            CoordList.AddLast(new VectorTime(point,time,mootPoint));
            if(point.X >= 0) {
                PreLast1 = Last1;
                Last1 = new VectorTime(point,time,mootPoint);
            }
            while(CoordList.Count > MaxPoints) {
                CoordList.RemoveFirst();
            }
                
        }

        public Vector GetCoord(double toTime,bool allowExtrapol = true) {
            if(toTime < (CoordList.Last?.Value.Time ?? 0))
                return CoordDistributor.NoPoint;
            if(!allowExtrapol) {

                return Last1.Vec;
            }
            if(Last1.Vec.X < 0 || PreLast1.Vec.X < 0)
                return Last1.Vec;
            return (PreLast1.Vec * (Last1.Time - toTime) + Last1.Vec * (toTime - PreLast1.Time)) / (Last1.Time - PreLast1.Time);


        }

        public virtual double GetDistToLast1Point(Vector toPoint) {
            if(CoordList.Count == 0)
                return 0;
            return (Last1.Vec - toPoint).Length;
        }

        public double Get_t1_param(int nTau = -1,bool countMoot = true) {
            int n1, n_all;
            n1 = 0;
            n_all = 0;
            var lstCurr = CoordList.Last;
            while(lstCurr?.Previous != null) {
                if(lstCurr.Value.Vec.X >= 0) {
                    if(!countMoot) {
                        if(!lstCurr.Value.MootVec)
                            n1++;
                    }else     
                        n1++;

                }
                    
                n_all++;
                lstCurr = lstCurr.Previous;
                if(nTau > 0 && n_all > nTau)
                    break;
            }
    
            if(n_all != 0)
                return (double)n1 / (double)n_all;
            return 0;
        }

        public virtual void ClearData() {
            CoordList.Clear();
            Last1 = new VectorTime(CoordDistributor.NoPoint,-1);
            PreLast1 = new VectorTime(CoordDistributor.NoPoint,-1);
    }

        public double GetLastTime1() {
            return Last1.Time;
        }

        public Vector GetCoord(bool allowExtrapol = true) {
            double time = CoordList.Last?.Value.Time ?? 0;
            return GetCoord(time,allowExtrapol);
        }

        public virtual void MoveDataTo(IUnknownCoords toMe) {
            toMe.ClearData();
            toMe.CoordList = CoordList;
            toMe.SetLast1(Last1);
            toMe.SetPreLast1(PreLast1);
            CoordList = new LinkedList<VectorTime>();
            ClearData();

        }

        public void SetLast1(VectorTime last1) {
            Last1 = last1;
        }

        public void SetPreLast1(VectorTime preLast1) {
            PreLast1 = preLast1;
        }
    }

    public interface IKnownCoords: IUnknownCoords {
        double ProbOfSignal1ToTime(double toTime);
        double T1{ get; }
        double T1_tol { get; }

    }


    public class Signal_Const1 : UnknownCoords, IKnownCoords, IUnknownCoords {
        public double T1 {
            get {
                return 1;
            }
        }

        public double T1_tol { get; set; } = 0.05;

        public double ProbOfSignal1ToTime(double toTime) {
            return 1d;
        }
    }

    public class Signal_Blink01: UnknownCoords, IKnownCoords, IUnknownCoords {
        public double Hz { get; set; }
        public double DeltaHz { get; set; }
        public double T1 { get; set; }

        public double T1_tol { get; set; } = 0.05;
        private double l1min, l0min, l1max, l0max;
        private void InitConst() {
            l1min = T1 / (Hz + DeltaHz);
            l0min = (1 - T1) / (Hz + DeltaHz);
            l1max = T1 / (Hz - DeltaHz);
            l0max = (1 - T1) / (Hz - DeltaHz);
        }
        public LinkedList<Otrezok> OtrList = new LinkedList<Otrezok>();
        public override void ClearData() {
            base.ClearData();
            OtrList.Clear();
        }
        public Signal_Blink01(double hz,double dHz,double t1) {
            Hz = hz;
            DeltaHz = dHz;
            T1 = t1;
            InitConst();
        }
        public override void AddPoint(Vector point, double time,bool mootPoint = false) {
            //ToDo Добавить удаление старых точк и отрезков
            base.AddPoint(point,time,mootPoint);
            int signal = point.X < 0 ? 0 : 1;
            if(OtrList.Count == 0 || OtrList.Last.Value.Signal != signal) {
                OtrList.AddLast(new Otrezok(signal,time));

            } else if(time >= OtrList.Last.Value.Tmax) {
                OtrList.Last.Value.Tmax = time;
            } else
                throw new Exception("Прошлое не изменить!)");
            while(OtrList.First?.Value.Tmin < CoordList.First?.Value.Time) {
                OtrList.RemoveFirst();
            }
            
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

        public override void MoveDataTo(IUnknownCoords toMe) {

            var toMeSB = toMe as Signal_Blink01;
            if(toMeSB != null) {

                toMeSB.ClearData();
                toMeSB.CoordList = CoordList;
                toMeSB.SetLast1(Last1);
                toMeSB.SetPreLast1(PreLast1);
                toMeSB.OtrList = OtrList;


                CoordList = new LinkedList<VectorTime>();
                ClearData();


            } else {
                base.MoveDataTo(toMe);
            }
        }

    }

    public static class SelectionHelper {
        public static int AddPointToCoords(this IEnumerable<IUnknownCoords> coords, double t, Vector point, double adoptRadius = 1.5) {
            var okCoords =
                coords.
                Where(np => np.CoordList.Count > 0).
                Where(np => np.GetDistToLast1Point(point) <= adoptRadius).
                ToArray();
            if(okCoords.Length == 1) {
                okCoords[0].AddPoint(point,t);

            }
            return okCoords.Length;
        }
        public static void ClearOldData(this IEnumerable<IUnknownCoords> coords, double t, double toolateperiod = 0.5) {
            var oldCoords =
                coords.
                Where(gdCrds => gdCrds.CoordList.Count > 0).
                Where(gdCrds => t - gdCrds.GetLastTime1() > toolateperiod);
            foreach(var oldy in oldCoords) {
                oldy.ClearData();
            }
        }
    }
}
