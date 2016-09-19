using Microsoft.VisualStudio.TestTools.UnitTesting;
using CoordGrab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CoordGrab.Tests {
    [TestClass()]
    public class OneCoordTests {
        [TestMethod()]
        public void OneCoordTest() {
            var tst = new Signal_Blink01(20,1,0.8);
            tst.AddPoint(new Vector(1,0),1);
            var ss = tst.CoordList.LastOrDefault(vt => vt.Vec.X < 0);
        }

        [TestMethod()]
        public void PerfectSignalTest() {
            var lst = new List<int>();
            for(int i = 0; i < 20; i++) {
                lst.Add(Signal_Blink01.PerfectSignal(i,4,3,-2));
            }
            Assert.AreEqual(1,Signal_Blink01.PerfectSignal(4,3,2,-2));
            Assert.AreEqual(0,Signal_Blink01.PerfectSignal(10,4,3,-2));
            Assert.AreEqual(1,Signal_Blink01.PerfectSignal(1,4,3,-2));
            Assert.AreEqual(1,Signal_Blink01.PerfectSignal(-1,4,3,-2));
            Assert.AreEqual(0,Signal_Blink01.PerfectSignal(-3,4,3,-2));
        }
    }
}