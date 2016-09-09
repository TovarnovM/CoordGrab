using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoordGrab
{
    using OxyPlot;
    using OxyPlot.Axes;
    using OxyPlot.Series;
    public class MainViewModel {
        public MainViewModel() {
            this.MyModel = new PlotModel { Title = "Example 1" };
            var linearAxis1 = new LinearAxis();
            linearAxis1.MajorGridlineStyle = LineStyle.Solid;
            linearAxis1.MaximumPadding = 0;
            linearAxis1.MinimumPadding = 0;
            linearAxis1.MinorGridlineStyle = LineStyle.Dot;
            linearAxis1.Position = AxisPosition.Bottom;
            MyModel.Axes.Add(linearAxis1);
            var linearAxis2 = new LinearAxis();
            linearAxis2.MajorGridlineStyle = LineStyle.Solid;
            linearAxis2.MaximumPadding = 0;
            linearAxis2.MinimumPadding = 0;
            linearAxis2.MinorGridlineStyle = LineStyle.Dot;
            MyModel.Axes.Add(linearAxis2);
            this.MyModel.Series.Add(new FunctionSeries(Math.Cos,0,10,0.1,"cos(x)"));
            StandartContext = MyModel.RenderingDecorator;
        }
        public Func<IRenderContext,IRenderContext> StandartContext { get; set; }
        public PlotModel MyModel { get; private set; }
    }
}
