using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zadacha2 {
    class Program {
        static Dictionary<int,int> PolyDict;
        static int Limit = 50;

        static void Main(string[] args) {
            PolyDict = new Dictionary<int,int>(Int32.MaxValue/100);
            int fail = 0;
            for(int i = 0; i < 13332; i++) {
                if(!ReOperPoly(i))
                    fail++;

            }
            Console.WriteLine(fail);
            Console.ReadKey();
        }

        static bool ReOperPoly(int num) {
            var history = new int[Limit];
            int lngth = -1;
            for(int i = 0; i < Limit; i++) {

                int oper = Operation(num);
                history[i] = oper;
                if(Polyndrome(oper)) {
                    return true;
                    lngth = i;
                    break;
                }
                continue;
                if(PolyDict.ContainsKey(oper) && PolyDict[oper] + i<Limit) {
                    lngth = i;
                    break;
                }
                num = oper;
            }
            return false;
            if(lngth < 0)
                return false;
            for(int i = 0; i < lngth; i++) {
                PolyDict.Add(history[i],i + 1);
            }
            return true;
        }

        static int Operation(int num) {
            return num + ReverseInt(num);
        } 

        static int ReverseInt(int num) {
            int rev = 0;
            while(num > 0) {
                int dig = num % 10;
                rev = rev * 10 + dig;
                num = num / 10;
            }
            return rev;
        }

        static bool Polyndrome(int num) {
            return num == ReverseInt(num) && num >= 10;
        }
    }
}
