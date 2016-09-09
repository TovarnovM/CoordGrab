namespace WpfApplication1 {
    using System.Collections.Generic;

    using OxyPlot;
    using OxyPlot.Series;

    public class MainViewModel {
        public MainViewModel() {
            this.Title = "Example 2";
            this.Points = new List<DataPoint>
                              {
                                  new DataPoint(0, 4),
                                  new DataPoint(10, 13),
                                  new DataPoint(20, 15),
                                  new DataPoint(30, 16),
                                  new DataPoint(40, 12),
                                  new DataPoint(50, 12)
                              };
            MyModel = new PlotModel { Title = "Example 1" };
            this.MyModel.Series.Add(new FunctionSeries(System.Math.Cos,0,10,0.1,"cos(x)"));
        }

        public string Title { get; private set; }

        public IList<DataPoint> Points { get; private set; }
        public PlotModel MyModel { get; private set; }
    }
}
