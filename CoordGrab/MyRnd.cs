using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneticSharp.Domain.Randomizations;

namespace CoordGrab {
    public class MyRnd:BasicRandomization {
        public double GetNorm(double mo, double sko) {
            return mo + sko * GetNorm();
        }
        public double GetNorm() {
            return Math.Sqrt(-2.0 * Math.Log(GetDouble())) * Math.Cos(Math.PI * 2 * GetDouble());
        }
    }
}
