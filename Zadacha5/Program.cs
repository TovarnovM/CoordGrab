using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zadacha5 {
    class Program {
        private static int i;

        /* Задача 5

            Дано равенство, в котором цифры заменены на буквы:
            vww + rqq = www 

            Найдите сколько у него решений, если различным буквам соответствуют различные цифры (ведущих нулей в числе не бывает).
        */
        static void Main(string[] args) {
            int vars = 0;
            for(int v = 1; v < 10; v++) {
                for(int w = 0; w < 10; w++) {
                    for(int r = 1; r < 10; r++) {
                        for(int q = 0; q < 10; q++) {
                            if(uravn(v,w,r,q)) {
                                vars++;
                                Console.WriteLine($"{vars}: {v}{w}{w} + {r}{q}{q} = {w}{w}{w}");
                            }
                                
                        }
                    }
                }
            }
            Console.WriteLine(vars);
            Console.ReadLine();

        }

        static bool uravn(int v, int w, int r,int q) {
            int first = 100 * v + w * 10 + w;
            int sec = 100 * r + 10 * q + q;
            int answ = 100 * w + 10 * w + w;
            return first + sec == answ;
        }
    }
}
