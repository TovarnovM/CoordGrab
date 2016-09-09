using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OxyPlot;
using OxyPlot.Series;
using Interpolator;

namespace CoordGrab {
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private void button_Click(object sender,RoutedEventArgs e) {
            
            var rc = new TrueCoord();
            rc.Addpoint(0,1,2);
            rc.Addpoint(1,3,4);
            rc.Addpoint(3,5,6);
            rc.SaveToXml(@"C:\Users\Миша\Desktop\tst.xml");
        }

        private void button1_Click(object sender,RoutedEventArgs e) {
            var rc = new TrueCoord();
            rc.LoadFromXml(@"C:\Users\Миша\Desktop\tst.xml");
        }

        

        private void button2_Click(object sender,RoutedEventArgs e) {
            var vm = DataContext as MainViewModel;
            vm.MyModel.Series.Clear();
            PlotSignal(1,0,1);
            PlotSignal(1,10,0.6);
        }

        private void PlotSignal(double hz, double dhz, double amp) {
            var ss = new LineSeries();

            var signal = new Signal(hz);
            signal.Amplitude = amp;
            signal.SetSinImpulse();
            //signal.SetRectangleImpulse(0.5);
            //signal.SetTrapecImpulse(0.1,0.5,0.7);
            double dt = 0.001;
            double t0 = 0d;
            double t1 = 5d;
            double ddhz = dhz / ((t1 - t0) / dt);
            while(t0< t1) {
                var sg = signal.GetAmp(t0 ,hz);
                ss.Points.Add(new DataPoint(t0 ,sg));
                t0 += dt;
                hz += ddhz;
            }
            var vm = DataContext as MainViewModel;
            vm.MyModel.Series.Add(ss);
            vm.MyModel.InvalidatePlot(false);
        }

        private void checkBox_Checked(object sender,RoutedEventArgs e) {
            var vm = DataContext as MainViewModel;

                vm.MyModel.RenderingDecorator = rc => new XkcdRenderingDecorator(rc);

            vm.MyModel.InvalidatePlot(false);

        }

        private void chbx_Click(object sender,RoutedEventArgs e) {

        }

        private void chbx_Unchecked(object sender,RoutedEventArgs e) {
            var vm = DataContext as MainViewModel;
            vm.MyModel.RenderingDecorator = vm.StandartContext;
            vm.MyModel.InvalidatePlot(false);

        }

        private void button3_Click(object sender,RoutedEventArgs e) {
            var vm = DataContext as MainViewModel;
            vm.MyModel.Series.Clear();
            var rndF = new RndFunction(t => Math.Cos(t) ,t => Math.Sin(t)/5 ,0.01,0.01);
            for(int i = 0; i < 1; i++) {
                var ser = new LineSeries();
                foreach(var point in rndF.GetInterp(0, 10).Points()) {
                    ser.Points.Add(new DataPoint(point.X,point.Y));
                }
                vm.MyModel.Series.Add(ser);
            }
            vm.MyModel.InvalidatePlot(false);
        }

        private void button3_Copy_Click(object sender,RoutedEventArgs e) {
            var vm = DataContext as MainViewModel;
            vm.MyModel.Series.Clear();
            var fuz = new FuzzySignal(20,19) {
                GenerateNewHzFunct = false
            };
            fuz.SetSinImpulse();
            for(int i = 0; i < 2; i++) {
                fuz.Amplitude = 1d / (i+1);
                fuz.Reset();
                var ser = new LineSeries();
                foreach(var point in fuz.GetInterpSignal(0,1,nPoints:1000).Points()) {
                    ser.Points.Add(new DataPoint(point.X,point.Y));
                }
                vm.MyModel.Series.Add(ser);
                var ser2 = new LineSeries();

                ser2.Points.AddRange(fuz.InterpHz.DataPoints(y => y/100));
                vm.MyModel.Series.Add(ser2);


            }
            vm.MyModel.InvalidatePlot(false);

        }
    }

    public static class InterpXYExt {
        public static IEnumerable<DataPoint> DataPoints(this InterpXY interp) {
            foreach(var point in interp.Points()) {
                yield return new DataPoint(point.X,point.Y);

            }
        }

        public static IEnumerable<DataPoint> DataPoints(this InterpXY interp, Func<double,double> changeYcoord) {
            foreach(var point in interp.Points()) {
                yield return new DataPoint(point.X,changeYcoord(point.Y));

            }
        }
    }


}
