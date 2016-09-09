using Interpolator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace CoordGrab {
    [Serializable]
    public class TrueCoord {
        public InterpXY X { get; set; }
        public InterpXY Y { get; set; }
        public string Name { get; private set; }
        public TrueCoord(string name = "") {
            X = new InterpXY() { Title = "X_Coords_of_" + name };
            Y = new InterpXY() { Title = "Y_Coords_of_" + name };
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
        
    }
}
