using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Zadacha4 {
    class Program {
        /*
         Задача 4

            Рассмотрим все возможные числа ab для 1<a<6 и 1<b<6: 
            22=4, 23=8, 24=16, 25=32 32=9, 33=27, 34=81, 35=243 42=16, 43=64, 44=256, 45=1024, 52=25, 53=125, 54=625, 55=3125 
            Если убрать повторения, то получим 15 различных чисел. 

            Сколько различных чисел ab для 2<a<135 и 2<b<136?
             */
        static void Main(string[] args) {
            var lst = new List<BigInteger>(135 * 135);
            for(int i = 3; i < 135; i++) {
                for(int j = 3; j < 136; j++) {
                    lst.Add(BigInteger.Pow(i,j));
                }
            }
            Console.WriteLine(lst.Distinct().Count());
            Console.ReadLine();
        }
    }
}
