using Interpolator;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace CoordGrab {
    [Serializable,XmlRoot(nameof(TrueCoord))]
    public class TrueCoord {
        private string _name;
        public InterpXY X { get; set; }
        public InterpXY Y { get; set; }
        public TrueCoord():this("") {

        }
        public string Name { get { return _name; }
            set {
                _name = value;
                X.Title = "X_Coords_of_" + _name;
                Y.Title = "Y_Coords_of_" + _name;
            } }
        public TrueCoord(string name = "") {
            X = new InterpXY();
            Y = new InterpXY();
            Name = name;
        }
        public TrueCoord(string fileName, string name):this(name) {
            LoadFromXml(fileName);
        }

        public Vector Getpoint(double t) {
            return new Vector(X[t],Y[t]);
        }
        public void Addpoint(double t, Vector point) {
            Addpoint(t,point.X,point.Y);
        }
        public void Addpoint(double t,double x, double y) {
            X.Add(t,x);
            Y.Add(t,y);
        }

        public void LoadFromXml(string fileName) {
            LoadFromXml(fileName,Name);
        }
        public void LoadFromXml(string fileName, string name) {
            var xOld = X;
            var yOld = Y;
            try {
                XmlSerializer serial = new XmlSerializer(typeof(List<InterpXY>));
                var sw = new StreamReader(fileName);
                var ss = (List<InterpXY>)serial.Deserialize(sw);
                sw.Close();
                X = ss.First(el => el.Title == "X_Coords_of_" + name);
                Y = ss.First(el => el.Title == "Y_Coords_of_" + name);
                xOld?.Dispose();
                yOld?.Dispose();
            }
            catch(Exception) {
                X = xOld;
                Y = yOld;
            }
        }
        public void SaveToXml(string fileName) {
            try {
                XmlSerializer serial = new XmlSerializer(typeof(List<InterpXY>));
                var sw = new StreamWriter(fileName);
                var lst = new List<InterpXY>(2);
                lst.Add(X);
                lst.Add(Y);
                serial.Serialize(sw,lst);
                sw.Close();
            }
            catch(Exception) { }
        }

        public IEnumerable<DataPoint> DataPoints() {
            for(int i = 0; i < X.Count; i++) {
                yield return new DataPoint(X.Data.Values[i].Value,Y.Data.Values[i].Value);
            }
        }

        public IEnumerable<DataPoint> DataPoints(Func<DataPoint,DataPoint> f) {
            for(int i = 0; i < X.Count; i++) {
                yield return f(new DataPoint(X.Data.Values[i].Value,Y.Data.Values[i].Value));
            }
        }

        public void ResizeTime(double mnozj) {
            var nwX = new InterpXY();
            foreach(var xs in X.Data) {
                nwX.Add(xs.Key * mnozj,xs.Value.Value);
            }
            X.Dispose();
            X = nwX;

            var nwY = new InterpXY();
            foreach(var ys in Y.Data) {
                nwY.Add(ys.Key * mnozj,ys.Value.Value);
            }
            Y.Dispose();
            Y = nwY;
        }
        
    }
}
