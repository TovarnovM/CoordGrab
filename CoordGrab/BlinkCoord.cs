using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneticSharp.Domain.Randomizations;
using Interpolator;

namespace CoordGrab {
    [Serializable]
    public class FuzzyBlinkCoord {
        public TrueCoord Coord { get; set; }
        public FuzzySignal InterpSignal { get; set; }
        public int MyProperty { get; set; }

    }
}
