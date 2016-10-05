using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoordGrab
{
    using Interpolator;
    using OxyPlot;
    using OxyPlot.Annotations;
    using OxyPlot.Axes;
    using OxyPlot.Series;
    using System.Windows;

    public class MainViewModel {
        public MainViewModel() {
            this.Model1 = GetNewModel("Signal tests");
            StandartContext = Model1.RenderingDecorator;
            CoolContext =  rc => new XkcdRenderingDecorator(rc);

            InitModel2();
            InitModel3("Signals");
            InitModel4("TimeSim");
            InitModel5("Algorythm");

        }
        public PlotModel Model1 { get; private set; }
        public PlotModel Model2 { get; private set; }
        public PlotModel Model3 { get; private set; }
        public PlotModel Model4 { get; private set; }
        public PlotModel Model5 { get; private set; }
        #region Model2

        public int ActiveIndex { get; set; } = 0;
        public Func<IRenderContext,IRenderContext> StandartContext { get; set; }
        public Func<IRenderContext,IRenderContext> CoolContext { get; set; }
        public void InitModel2() {
            Model2 = GetNewModel("GetCoords4Sim");
            Model2.PlotType = PlotType.Cartesian;
            Model2.Series.Add(GetSeries4Model2("траектория1",OxyColors.SkyBlue,0));
            Model2.Series.Add(GetSeries4Model2("траектория2",OxyColors.DarkBlue,1));
            Model2.Series.Add(GetSeries4Model2("траектория3",OxyColors.MediumVioletRed,2));
            Model2.Series.Add(GetSeries4Model2("траектория4",OxyColors.ForestGreen,3));
            Model2.MouseDown += (s,e) => {
                if(e.ChangedButton == OxyMouseButton.Left) {
                    var ser = Model2.Series[ActiveIndex] as LineSeries;
                    // Add a point to the line series.
                    ser.Points.Add(ser.InverseTransform(e.Position));
                    IndOfPointToMove[ActiveIndex] = ser.Points.Count - 1;
                    var an = new PointAnnotation() {
                        Shape = MarkerType.Circle,
                        Size = 3,
                        Fill = ser.Color,
                        X = ser.InverseTransform(e.Position).X,
                        Y = ser.InverseTransform(e.Position).Y,

                        ToolTip = (ser.Points.Count - 1).ToString(),
                        Text = PointsVisible ? (ser.Points.Count - 1).ToString() : ""
                    };
                    Annot[ActiveIndex].Add(an);
                    Model2.Annotations.Add(an);
                    Model2.InvalidatePlot(false);
                    e.Handled = true;
                }
            };

        }

        

        public PlotModel GetNewModel(string title ="") {
            var m = new PlotModel { Title = title};
            var linearAxis1 = new LinearAxis();
            linearAxis1.MajorGridlineStyle = LineStyle.Solid;
            linearAxis1.MaximumPadding = 0;
            linearAxis1.MinimumPadding = 0;
            linearAxis1.MinorGridlineStyle = LineStyle.Dot;
            linearAxis1.Position = AxisPosition.Bottom;
            m.Axes.Add(linearAxis1);
            var linearAxis2 = new LinearAxis();
            linearAxis2.MajorGridlineStyle = LineStyle.Solid;
            linearAxis2.MaximumPadding = 0;
            linearAxis2.MinimumPadding = 0;
            linearAxis2.MinorGridlineStyle = LineStyle.Dot;
            m.Axes.Add(linearAxis2);
            return m;
        }

        public Dictionary<int,int> IndOfPointToMove { get; set; } = new Dictionary<int,int>();
        public Dictionary<int,List<PointAnnotation>> Annot { get; set; } = new Dictionary<int,List<PointAnnotation>>();
        public LineSeries GetSeries4Model2(string title,OxyColor color,int index) {
            
            var s1 = new LineSeries {
                Title = title,
                Color = color,
                //MarkerType = MarkerType.Circle,
                //MarkerSize = 6,
                //MarkerStroke = OxyColors.White,
                //MarkerFill = color,
                //MarkerStrokeThickness = 1.5
            };
            IndOfPointToMove.Add(index,-1);
            Annot.Add(index,new List<PointAnnotation>());
            

            // Subscribe to the mouse down event on the line series
            s1.MouseDown += (s,e) => {
                // only handle the left mouse button (right button can still be used to pan)
                if(e.ChangedButton == OxyMouseButton.Left) {
                    int indexOfNearestPoint = (int)Math.Round(e.HitTestResult.Index);
                    var nearestPoint = s1.Transform(s1.Points[indexOfNearestPoint]);

                    // Check if we are near a point
                    if((nearestPoint - e.Position).Length < 10) {
                        // Start editing this point
                        IndOfPointToMove[index] = indexOfNearestPoint;
                    } else {
                        // otherwise create a point on the current line segment
                        //int i = (int)e.HitTestResult.Index + 1;
                        //s1.Points.Insert(i,s1.InverseTransform(e.Position));
                        //IndOfPointToMove[index] = i;
                    }

                    // Change the linestyle while editing
                    s1.LineStyle = LineStyle.DashDot;

                    // Remember to refresh/invalidate of the plot
                    Model2.InvalidatePlot(false);

                    // Set the event arguments to handled - no other handlers will be called.
                    e.Handled = true;
                }
            };

            s1.MouseMove += (s,e) =>
            {
                if(IndOfPointToMove[index] >= 0) {
                    // Move the point being edited.
                    s1.Points[IndOfPointToMove[index]] = s1.InverseTransform(e.Position);
                    Annot[index][IndOfPointToMove[index]].X = s1.InverseTransform(e.Position).X;
                    Annot[index][IndOfPointToMove[index]].Y = s1.InverseTransform(e.Position).Y;
                    Model2.InvalidatePlot(false);
                    e.Handled = true;
                }
            };

            s1.MouseUp += (s,e) => {
                // Stop editing
                IndOfPointToMove[index] = -1;
                s1.LineStyle = LineStyle.Solid;
                Model2.InvalidatePlot(false);
                e.Handled = true;
            };

            

            return s1;

        }
        public void DelLsatPoint() {
            if(Annot[ActiveIndex].Count == 0)
                return;
            var an = Annot[ActiveIndex].Last();
            Annot[ActiveIndex].Remove(an);
            Model2.Annotations.Remove(an);

            var ser = (Model2.Series[ActiveIndex] as LineSeries);
            ser.Points.RemoveAt(ser.Points.Count - 1);
            IndOfPointToMove[ActiveIndex] = -1;
            Model2.InvalidatePlot(false);
        }
        private bool _pointsVis = true;
        public bool PointsVisible {
            get {
                return _pointsVis;
            }
            set {
                _pointsVis = value;
                foreach(var an in Model2.Annotations) {
                    (an as PointAnnotation).Text = _pointsVis ? (an as PointAnnotation).ToolTip : "";
                }
                Model2.InvalidatePlot(false);
            }
        }

        public List<TrueCoord> GetTrueCoordsList(double resizeMnozj) {
            var res = new List<TrueCoord>(4);
            foreach(var ser in Model2.Series) {
                var s = ser as LineSeries;
                var trCrds = new TrueCoord(s.Title);
                int i = 0;
                foreach(var point in s.Points) {
                    trCrds.Addpoint(i++,point.X,point.Y);
                }
                if(trCrds.X.Count == 0)
                    trCrds.Addpoint(0,0,0);
                trCrds.ResizeTime(resizeMnozj);
                res.Add(trCrds);
            }
            return res;
        }

        public void LoadInterpList(List<TrueCoord> data) {
            Model2.Annotations.Clear();
            foreach(var ser in Model2.Series) {
                (ser as LineSeries).Points.Clear();
            }
            foreach(var item in Annot) {
                item.Value.Clear();
            }

            for(int i = 0; i < 4; i++) {
                var ser = Model2.Series[i] as LineSeries;
                IndOfPointToMove[i] = -1;
                if(data[i].X.Count < 2)
                    continue;
                // Add a point to the line series.
                foreach(var dp in data[i].DataPoints()) {
                    ser.Points.Add(dp);
                    var an = new PointAnnotation() {
                        Shape = MarkerType.Circle,
                        Size = 3,
                        Fill = ser.Color,
                        X = dp.X,
                        Y = dp.Y,

                        ToolTip = (ser.Points.Count - 1).ToString(),
                        Text = PointsVisible ? (ser.Points.Count - 1).ToString() : ""
                    };    
                                    
                    Annot[i].Add(an);
                    Model2.Annotations.Add(an);
                }

                
            }
            Model2.InvalidatePlot(false);
        }
        #endregion

        #region Model3
        public void InitModel3(string title) {
            Model3 = GetNewModel(title);
            Model3.Series.Add(new LineSeries {
                Title = "траектория1",
                Color = OxyColors.SkyBlue
            });
            Model3.Series.Add(new LineSeries {
                Title = "траектория2",
                Color = OxyColors.DarkBlue
            });
            Model3.Series.Add(new LineSeries {
                Title = "траектория3",
                Color = OxyColors.MediumVioletRed
            });
            Model3.Series.Add(new LineSeries {
                Title = "траектория4",
                Color = OxyColors.ForestGreen
            });

           

        }

        public List<FuzzySignal> Signals { get; set; } = new List<FuzzySignal>();
        public void RedrawModel3() {
            foreach(var ser in Model3.Series) {
                (ser as LineSeries).Points.Clear();
            }
            var minHz = Signals.Min(s => s.Hz0);
            var toTime = 7d / minHz;
            for(int i = 0; i < 4; i++) {
                var ser = Model3.Series[i] as LineSeries;
                var sInterp = Signals[i].GetInterpSignal(0,toTime,nPoints:1000);
                ser.Points.AddRange(sInterp.DataPoints(y => y + 4.5 -i * 1.5));
            }
            
            Model3.InvalidatePlot(false);
        }




        #endregion

        #region Model4
        public List<TrueCoord> Coords { get; set; }
        public List<InterpXY> SignalsInterp { get; set; }
        public List<double> TimeMoments { get; set; }
        public List<List<Vector?>> ReceivedPoints { get; set; }
        public double UnityRadius { get; set; } = 3d;
        public List<List<Vector>> ReceivedPointsTrue { get; set; }

        public void InitModel4(string title) {
            Model4 = GetNewModel(title);
            for(int i = 0; i < 4; i++) {
                Model4.Series.Add(new ScatterSeries {
                    Title = "траектория"+i.ToString(),
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 2
                });
            }
            Model4.Series.Add(new ScatterSeries {
                Title = "Фактич. полученные координаты",
                MarkerType = MarkerType.Diamond,
                MarkerSize = 2,
                MarkerFill = OxyColors.DarkOrange
            });
            ColoredPoints = false;


        }

        private bool _coloredPoints = false;

        public bool ColoredPoints {
            get { return _coloredPoints; }
            set {
                _coloredPoints = value;
                if(_coloredPoints) {
                    (Model4.Series[0] as ScatterSeries).MarkerFill = OxyColors.SkyBlue;
                    (Model4.Series[1] as ScatterSeries).MarkerFill = OxyColors.DarkBlue;
                    (Model4.Series[2] as ScatterSeries).MarkerFill = OxyColors.MediumVioletRed;
                    (Model4.Series[3] as ScatterSeries).MarkerFill = OxyColors.ForestGreen;
                } else {
                    for(int i = 0; i < 4; i++) {
                        (Model4.Series[i] as ScatterSeries).MarkerFill = OxyColors.DarkBlue;
                    }
                    
                }
                Model4.InvalidatePlot(false);

            }
        }

        public void SynchModels(double resizeMnozj, double cameraHz, double dcameraHz) {
            Coords = GetTrueCoordsList(resizeMnozj);
            if(Signals.Count != 4)
                throw new Exception("Загрузи сигналы!");
            var timeMaxSeq = from crds in Coords
                             select crds.X.Data.Keys.Max();
            var timeMax = timeMaxSeq.Max();

            double t_shagAver = 0.5 / (cameraHz - dcameraHz) + 0.5 / (cameraHz + dcameraHz);
            double delta_shag = 0.5 / (cameraHz - dcameraHz) - 0.5 / (cameraHz + dcameraHz);
            TimeMoments = new List<double>((int)(timeMax*(cameraHz+ dcameraHz) +7));
            var rndFunct = new RndFunction(t => t,0,t_shagAver,delta_shag);
            var timeInterp = rndFunct.GetInterp(0,timeMax);
            TimeMoments = new List<double>(timeInterp.Data.Keys);


            SignalsInterp = new List<InterpXY>(4);
            for(int i = 0; i < 4; i++) {
                Signals[i].Reset();
                SignalsInterp.Add(Signals[i].GetInterpSignal(TimeMoments));
            }

            ReceivedPoints = new List<List<Vector?>>(TimeMoments.Capacity);
            for(int i = 0; i < TimeMoments.Count; i++) {
                ReceivedPoints.Add(new List<Vector?>(4));
                for(int j = 0; j < 4; j++) {
                    if(SignalsInterp[j][TimeMoments[i]] > 0.1) {
                        ReceivedPoints[i].Add(Coords[j].Getpoint(TimeMoments[i]));
                    }else {
                        ReceivedPoints[i].Add(null);
                    }
                    
                }
            }

            var trp = ReceivedPoints.
                Select(lv => {
                    var notNull = lv.Where(v => v.HasValue).Select(v => v.Value).ToList();

                    var distMatr = new bool[notNull.Count,notNull.Count];
                    for(int i = 0; i < notNull.Count; i++) {
                        for(int j = i + 1; j < notNull.Count; j++) {
                            distMatr[i,j] = (notNull[i] - notNull[j]).Length < UnityRadius;
                            distMatr[j,i] = distMatr[i,j];
                        }
                    }

                    var alreadyUnityP = new List<int>(notNull.Count);
                    var res = new List<Vector>(4);
                    for(int i = 0; i < notNull.Count; i++) {
                        if(alreadyUnityP.Contains(i))
                            continue;
                        var closeP = new List<int>(notNull.Count);

                        Action<int> recurce = null;
                        recurce = new Action<int>((nearMe) => {
                            closeP.Add(nearMe);
                            for(int k = 0; k < notNull.Count; k++) {
                                if(distMatr[nearMe,k] && !closeP.Contains(k)) {
                                    recurce(k);
                                }
                            }
                        });

                        recurce(i);
                        alreadyUnityP.AddRange(closeP);
                        alreadyUnityP = alreadyUnityP.Distinct().ToList();
                        if(closeP.Count > 2) {
                            int ss = 1;
                        }
                        //var resP = notNull[closeP[0]];
                        //for(int j = 1; j< closeP.Count; j++) {
                        //    resP += notNull[closeP[j]];                            
                        //}
                        //resP /= closeP.Count + 1;
                        var resP = closeP.Aggregate(new Vector(),(vec,k) => vec += notNull[k],(vec) => vec /= closeP.Count);
                        res.Add(resP);
                    }

                    return res;
                });
            ReceivedPointsTrue = new List<List<Vector>>(TimeMoments.Capacity);
            foreach(var rp in trp) {
                ReceivedPointsTrue.Add(rp);
            }

            RedrawModel4();

        }

        public void GetGraphics() {
            int N = 5;
            if(Model4.Series.Count == N) {
                Model4.Series.Add(new LineSeries {
                    Title = "t фактич траектории 1",
                    Color = OxyColors.SkyBlue
                });
                Model4.Series.Add(new LineSeries {
                    Title = "t фактич траектории 2",
                    Color = OxyColors.DarkBlue
                });
                Model4.Series.Add(new LineSeries {
                    Title = "t фактич траектории 3",
                    Color = OxyColors.MediumVioletRed
                });
                Model4.Series.Add(new LineSeries {
                    Title = "t фактич траектории 4",
                    Color = OxyColors.ForestGreen
                });
               
            }
            for(int i = N; i < N+4; i++) {
                (Model4.Series[i] as LineSeries).Points.Clear();
            }

            for(int i = 0; i < N; i++) {
                int n = 0;
                for(int j = 0; j < ReceivedPoints.Count; j++) {
                    if(ReceivedPoints[j][i].HasValue)
                        n++;
                    (Model4.Series[i + N] as LineSeries).Points.Add(new DataPoint(j,(double)n / (j+1)));
                }
            }
            Model4.InvalidatePlot(false);
        }

        public void RedrawModel4() {
            for(int i = 0; i < 5; i++) {
                (Model4.Series[i] as ScatterSeries).Points.Clear();
            }

            for(int i = 0; i < 4; i++) {
                var ser = Model4.Series[i] as ScatterSeries;
                ser.Points.Capacity = ReceivedPoints.Count;
            }
            for(int i = 0; i < ReceivedPoints.Count; i++) {
                for(int j = 0; j < 4; j++) {
                    var ser = Model4.Series[j] as ScatterSeries;
                    if(ReceivedPoints[i][j] != null) {
                        var sp = new ScatterPoint(ReceivedPoints[i][j].Value.X,ReceivedPoints[i][j].Value.Y);
                        ser.Points.Add(sp);
                    }
                        
                    
                }
            }
            var seq = from t in ReceivedPointsTrue
                      from v in t
                      select new ScatterPoint(v.X, v.Y);

            (Model4.Series[4] as ScatterSeries).Points.AddRange(seq);




            Model4.InvalidatePlot(false);
        }

        #endregion

        #region Model5
        public int Ntime { get; set; }
        private int ncurr;

        public int NCurr {
            get { return ncurr; }
            set { ncurr = value;
                for(int i = 0; i < 4; i++) {
                    var ss = Model5.Series[i] as ScatterSeries;
                    ss.Points.Clear();
                    for(int j = 0; j < ncurr; j++) {
                        if(AlgorDataPoints[i][j] != null)
                            ss.Points.Add(AlgorDataPoints[i][j]);
                    }
                }
                var sss = Model5.Series[4] as ScatterSeries;
                sss.Points.Clear();
                for(int j = 0; j < ncurr; j++) {
                    foreach(var sp in UDataPoints[j]) {
                        sss.Points.Add(sp);
                    }
                }


                    Model5.InvalidatePlot(true);


            }
        }

        public List<List<ScatterPoint>> AlgorDataPoints = new List<List<ScatterPoint>>(5);
        public List<List<ScatterPoint>> UDataPoints = new List<List<ScatterPoint>>();

        public void LoadData() {
            AlgorDataPoints.Clear();
            UDataPoints.Clear();
            var lst = new List<IKnownCoords>(4);
            foreach(var s in Signals) {
                if(s.T1 >= 1) {
                    lst.Add(new Signal_Const1());
                }else {
                    lst.Add(new Signal_Blink01(s.Hz0,s.DeltaHz,s.T1));
                }
                
                AlgorDataPoints.Add(new List<ScatterPoint>(Ntime+1));
            }

            UDataPoints.Capacity = Ntime + 1;

            var cd = new CoordDistributor(lst);

            Ntime = ReceivedPointsTrue.Count();

            for(int i = 0; i < Ntime; i++) {
                cd.AddPoints(TimeMoments[i],ReceivedPointsTrue[i].ToArray());
                var rPoints = cd.GetLastCoords(false);
                for(int j = 0; j < rPoints.Length; j++) {
                    if(rPoints[j].X >= 0) {
                        AlgorDataPoints[j].Add(new ScatterPoint(rPoints[j].X,rPoints[j].Y));
                    } else {
                        AlgorDataPoints[j].Add(null);
                    }
                }

                
                UDataPoints.Add(new List<ScatterPoint>(cd.UnknownClosePoints.Count));
                for(int j = 0; j < cd.UnknownClosePoints.Count; j++) {
                    var uPoint = cd.UnknownClosePoints[j].GetCoord(false);
                    if(uPoint.X >= 0) {
                        UDataPoints[i].Add(new ScatterPoint(uPoint.X,uPoint.Y));
                    }
                }
            }
        }
        



        public void InitModel5(string title) {
            Model5 = GetNewModel(title);
            for(int i = 0; i < 4; i++) {
                Model5.Series.Add(new ScatterSeries {
                    Title = "траектория" + i.ToString(),
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 2
                });
            }
            (Model5.Series[0] as ScatterSeries).MarkerFill = OxyColors.SkyBlue;
            (Model5.Series[1] as ScatterSeries).MarkerFill = OxyColors.DarkBlue;
            (Model5.Series[2] as ScatterSeries).MarkerFill = OxyColors.MediumVioletRed;
            (Model5.Series[3] as ScatterSeries).MarkerFill = OxyColors.ForestGreen;

            Model5.Series.Add(new ScatterSeries {
                Title = "?? точки",
                MarkerType = MarkerType.Star,
                MarkerSize = 3,
                MarkerStroke = OxyColors.Red
            });
        }
        




        #endregion
    }
}
