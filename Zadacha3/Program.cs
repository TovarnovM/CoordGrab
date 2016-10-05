using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Zadacha3 {
    class Program {
        static BigInteger[] primes;
        static void Main(string[] args) {
            string result = Properties.Resources.primes11;
            var nums = from lns in result.Split('\n','\r',' ')
                       where lns != "" && lns != "\r"
                       select BigInteger.Parse(lns);

            BigInteger N = 9400000; //N = 11550 S(N) = 33336083
            primes = nums.Where(n => n <= N).ToArray();





            BigInteger min = 7300000;
            BigInteger max = 7400000;

            Console.WriteLine($" n = 10 m(n) = { DelBezOst(10)}   (5 is right)");
            Console.WriteLine($" n = 25 m(n) = { DelBezOst(25)}   (10 is right)");
            //Console.WriteLine(BigInteger.Pow(135,135));
       
            BigInteger sum = 0;
            using(StreamWriter sw = new StreamWriter(@"C:\Users\Миша\Desktop\razl.txt")) {
                for(BigInteger i = min; i <= max; i++) {
                    var plus = DelBezOst(i);
                    sum += plus;
                    sw.WriteLine($"{i}   {plus}");
                //var rzlMin = Razl(i);
                //if(rzlMin[rzlMin.Keys.Max()] == 1)
                //    continue;
                //Console.WriteLine("==================================");
                //Console.WriteLine($"i = {i}");
                //foreach(var item in rzlMin) {
                //    Console.Write($"{item.Key}^{item.Value}   ");
                //}
                //Console.WriteLine();
                //Console.WriteLine($"proizv = {rzlMin.Select(kp => (BigInteger)Math.Pow(kp.Key,kp.Value)).Aggregate(1,(agr,next) =>  agr*next)}");

                }
            }

            Console.WriteLine($"From S({min};{max}) = {sum}");

            Console.ReadKey();
        }



        static BigInteger DelBezOst(BigInteger znam) {
            var rzl = Razl(znam);
            BigInteger osn = rzl.Keys.Max();
            bool isOstatok = Ostatok(osn,rzl);
            while(isOstatok) {
                osn++;
                isOstatok = Ostatok(osn,rzl);
            }
            return osn;
            

        }

        static Dictionary<BigInteger,long> Razl(BigInteger num) {
            long del = 0;
            var res = new List<BigInteger>();
            bool isprimeSEARCH = false;
            do {
                if(!isprimeSEARCH && Array.BinarySearch(primes, num) >= 0) {
                    res.Add(num);
                    isprimeSEARCH = true;
                    break;
                }
                if(num % primes[del] == 0) {
                    num = num / primes[del];
                    isprimeSEARCH = false;
                    res.Add(primes[del]);
                } else 
                    del++;
            } while(num >= del);

            var umn = res.GroupBy(r => r).ToDictionary(g =>  g.Key, g=> g.LongCount());


            return umn;

        }

        static BigInteger GetStepenFakPriMn(BigInteger osn,BigInteger prMn) {
            BigInteger sum = 0;

            BigInteger tmp;
           
            do {
                tmp = osn / prMn;
                prMn *= prMn;
                sum += tmp;
                //if(sum > sravn)
                //    return sum;
            } while(tmp>0);
            return sum;
        }

        static bool Ostatok(BigInteger fakosn, Dictionary<BigInteger,long> znam) {
            foreach(var item in znam) {
                if(GetStepenFakPriMn(fakosn,item.Key) < item.Value)
                    return true;
            }
            return false;
        }
        
    }
}
