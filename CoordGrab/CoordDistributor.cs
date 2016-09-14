using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CoordGrab {
    public class CoordDistributor {
        public int CoordCount { get; set; }
        private List<List<Vector>> goodBase;
      

    }

    public class OneCoord {
        public List<Vector> CoordList;
        public double SignalOnePodrAver;
        public double SignalZeroPodeAver;
    }
}
