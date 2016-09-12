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
using System.Xml.Serialization;
using System.IO;

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
            vm.Model1.Series.Clear();
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
            vm.Model1.Series.Add(ss);
            vm.Model1.InvalidatePlot(false);
        }

        private void checkBox_Checked(object sender,RoutedEventArgs e) {
            var vm = DataContext as MainViewModel;

            vm.Model1.RenderingDecorator = vm.CoolContext;
            vm.Model2.RenderingDecorator = vm.CoolContext;
            vm.Model3.RenderingDecorator = vm.CoolContext;
            vm.Model4.RenderingDecorator = vm.CoolContext;
            vm.Model1.InvalidatePlot(false);
            vm.Model2.InvalidatePlot(false);
            vm.Model3.InvalidatePlot(false);
            vm.Model4.InvalidatePlot(false);

        }


        private void chbx_Unchecked(object sender,RoutedEventArgs e) {
            var vm = DataContext as MainViewModel;
            vm.Model1.RenderingDecorator = vm.StandartContext;
            vm.Model2.RenderingDecorator = vm.StandartContext;
            vm.Model1.InvalidatePlot(false);
            vm.Model2.InvalidatePlot(false);
            vm.Model3.RenderingDecorator = vm.StandartContext;
            vm.Model4.RenderingDecorator = vm.StandartContext;
            vm.Model3.InvalidatePlot(false);
            vm.Model4.InvalidatePlot(false);

        }

        private void button3_Click(object sender,RoutedEventArgs e) {
            var vm = DataContext as MainViewModel;
            vm.Model1.Series.Clear();
            var rndF = new RndFunction(t => Math.Cos(t) ,t => Math.Sin(t)/5 ,0.01,0.01);
            for(int i = 0; i < 1; i++) {
                var ser = new LineSeries();
                foreach(var point in rndF.GetInterp(0, 10).Points()) {
                    ser.Points.Add(new DataPoint(point.X,point.Y));
                }
                vm.Model1.Series.Add(ser);
            }
            vm.Model1.InvalidatePlot(false);
        }

        private void button3_Copy_Click(object sender,RoutedEventArgs e) {
            var vm = DataContext as MainViewModel;
            vm.Model1.Series.Clear();
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
                vm.Model1.Series.Add(ser);
                var ser2 = new LineSeries();

                ser2.Points.AddRange(fuz.InterpHz.DataPoints(y => y/100));
                vm.Model1.Series.Add(ser2);


            }
            vm.Model1.InvalidatePlot(false);

        }

        private void radioButton_Checked(object sender,RoutedEventArgs e) {
            var vm = DataContext as MainViewModel;
            vm.ActiveIndex = 0;
        }

        private void radioButton_Copy_Checked(object sender,RoutedEventArgs e) {
            var vm = DataContext as MainViewModel;
            vm.ActiveIndex = 1;
        }

        private void radioButton_Copy1_Checked(object sender,RoutedEventArgs e) {
            var vm = DataContext as MainViewModel;
            vm.ActiveIndex = 2;
        }

        private void radioButton_Copy2_Checked(object sender,RoutedEventArgs e) {
            var vm = DataContext as MainViewModel;
            vm.ActiveIndex = 3;
        }

        private void button4_Click(object sender,RoutedEventArgs e) {
            var vm = DataContext as MainViewModel;
            vm.DelLsatPoint();
        }

        private void checkBox_Checked_1(object sender,RoutedEventArgs e) {
            var vm = DataContext as MainViewModel;
            vm.PointsVisible = true;
        }

        private void checkBox_Unchecked(object sender,RoutedEventArgs e) {
            var vm = DataContext as MainViewModel;
            vm.PointsVisible = false;
        }

        private void button5_Click(object sender,RoutedEventArgs e) {
            // Configure save file dialog box
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Coords"; // Default file name
            dlg.DefaultExt = ".xml"; // Default file extension
            dlg.Filter = "Text documents (.xml)|*.xml"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if(result == true) {
                // Save document
                string filename = dlg.FileName;
                SaveCoords(filename);

            }
        }

        private void SaveCoords(string filename) {
            try {
                XmlSerializer serial = new XmlSerializer(typeof(List<TrueCoord>));
                var sw = new StreamWriter(filename);
                var vm = DataContext as MainViewModel;
                double resizeMasht;
                Double.TryParse(tbResize.Text,out resizeMasht);


                serial.Serialize(sw,vm.GetTrueCoordsList(resizeMasht));
                sw.Close();
            }
            catch {
                throw new Exception("((");

            }
        }

        private void button5_Copy_Click(object sender,RoutedEventArgs e) {
            // Configure save file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "Coords"; // Default file name
            dlg.DefaultExt = ".xml"; // Default file extension
            dlg.Filter = "Text documents (.xml)|*.xml"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if(result == true) {
                // Save document
                string filename = dlg.FileName;
                OpenCoords(filename);
            }
        }

        private void OpenCoords(string filename) {
            try {
                XmlSerializer serial = new XmlSerializer(typeof(List<TrueCoord>));
                var sr = new StreamReader(filename);
                var res = (List<TrueCoord>)serial.Deserialize(sr);
                sr.Close();
                var vm = DataContext as MainViewModel;
                vm.LoadInterpList(res);
            }
            catch {
                throw new Exception("((");

            }
        }

        private void button7_Click(object sender,RoutedEventArgs e) {
            var vm = DataContext as MainViewModel;
            double resizeMasht;
            Double.TryParse(tbResize.Text,out resizeMasht);
            double cameraHz;
            Double.TryParse(tbCameraHz.Text,out cameraHz);
            double dcameraHz;
            Double.TryParse(tbDCameraHz.Text,out dcameraHz);

            vm.SynchModels(resizeMasht,cameraHz,dcameraHz);
        }

        private void button8_Click(object sender,RoutedEventArgs e) {
            try {
                var lst = new List<FuzzySignal>(4);

                double hz0 , dhz0, t1;
                Double.TryParse(tbHz0.Text,out hz0);     
                Double.TryParse(tbdHz0.Text,out dhz0);               
                Double.TryParse(tbT0.Text,out t1);
                var s1 = new FuzzySignal(hz0,dhz0,"Signal_1");
                s1.SetRectangleImpulse(t1);
                lst.Add(s1);

                Double.TryParse(tbHz1.Text,out hz0);
                Double.TryParse(tbdHz1.Text,out dhz0);
                Double.TryParse(tbT1.Text,out t1);
                s1 = new FuzzySignal(hz0,dhz0,"Signal_2");
                s1.SetRectangleImpulse(t1);
                lst.Add(s1);

                Double.TryParse(tbHz2.Text,out hz0);
                Double.TryParse(tbdHz2.Text,out dhz0);
                Double.TryParse(tbT2.Text,out t1);
                s1 = new FuzzySignal(hz0,dhz0,"Signal_3");
                s1.SetRectangleImpulse(t1);
                lst.Add(s1);

                Double.TryParse(tbHz3.Text,out hz0);
                Double.TryParse(tbdHz3.Text,out dhz0);
                Double.TryParse(tbT3.Text,out t1);
                s1 = new FuzzySignal(hz0,dhz0,"Signal_4");
                s1.SetRectangleImpulse(t1);
                lst.Add(s1);

                var vm = DataContext as MainViewModel;
                vm.Signals = lst;
                vm.RedrawModel3();
            }
            catch(Exception) {

                throw;
            }
        }

        private void button8_Copy1_Click(object sender,RoutedEventArgs e) {

            // Configure save file dialog box
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Signals"; // Default file name
            dlg.DefaultExt = ".xml"; // Default file extension
            dlg.Filter = "Text documents (.xml)|*.xml"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if(result == true) {
                // Save document
                string filename = dlg.FileName;
                SaveSignals(filename);
            }
        }

        private void SaveSignals(string filename) {
                try {
                    XmlSerializer serial = new XmlSerializer(typeof(List<FuzzySignal>));
                    var sw = new StreamWriter(filename);
                    var vm = DataContext as MainViewModel;
                    serial.Serialize(sw,vm.Signals);
                    sw.Close();
                }
                catch {
                    throw new Exception("((");

                }
        }

        private void button8_Copy_Click(object sender,RoutedEventArgs e) {
            // Configure save file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "Signals"; // Default file name
            dlg.DefaultExt = ".xml"; // Default file extension
            dlg.Filter = "Text documents (.xml)|*.xml"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if(result == true) {
                // Save document
                string filename = dlg.FileName;
                OpenSignals(filename);
            }
        }

        private void OpenSignals(string filename) {
            try {
                XmlSerializer serial = new XmlSerializer(typeof(List<FuzzySignal>));
                var sr = new StreamReader(filename);
                var lst = (List<FuzzySignal>)serial.Deserialize(sr);
                sr.Close();

                tbHz0.Text = lst[0].Hz0.ToString();
                tbdHz0.Text = lst[0].DeltaHz.ToString();
                tbT0.Text = lst[0].T1.ToString();

                tbHz1.Text = lst[1].Hz0.ToString();
                tbdHz1.Text = lst[1].DeltaHz.ToString();
                tbT1.Text = lst[1].T1.ToString();

                tbHz2.Text = lst[2].Hz0.ToString();
                tbdHz2.Text = lst[2].DeltaHz.ToString();
                tbT2.Text = lst[2].T1.ToString();

                tbHz3.Text = lst[3].Hz0.ToString();
                tbdHz3.Text = lst[3].DeltaHz.ToString();
                tbT3.Text = lst[3].T1.ToString();
                var vm = DataContext as MainViewModel;

                vm.Signals = lst;
                vm.RedrawModel3();
            }
            catch {
                throw new Exception("((");

            }
        }

        private void button7_Copy_Click(object sender,RoutedEventArgs e) {
            OpenCoords(@"Coords.xml");
            OpenSignals(@"Signals.xml");
            var vm = DataContext as MainViewModel;
            vm.SynchModels(1,120,10);
        }

        private void checkBox1_Checked(object sender,RoutedEventArgs e) {
            var vm = DataContext as MainViewModel;
            vm.ColoredPoints = true;
        }

        private void checkBox1_Unchecked(object sender,RoutedEventArgs e) {
            var vm = DataContext as MainViewModel;
            vm.ColoredPoints = false;
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
