using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Zadacha1 {
    class Program {
        static int[] primes;

        static void Main(string[] args) {

            string result = Properties.Resources._10000;
            var nums = from lns in result.Split('\n')
                       from wrd in lns.Split(' ')
                       where wrd != "" && wrd != "\r"
                       select int.Parse(wrd);

            int N = 11550; //N = 11550 S(N) = 33336083
            primes = nums.Where(n => n<=N).ToArray();

            var tmp = new int[N+1];
            for(int i = 0; i < primes.Length; i++) {
                tmp[primes[i]] = 1;
            }
            int count = 0;
            int perc = N / 1000;
            for(int i = 1; i < N; i++) {
                count += tmp.Sum();
                tmp = Shifter(tmp);
                if(i % perc == 0) {
                    Console.Clear();
                    Console.WriteLine($"сделано {0.1*i/perc}%");
                }
                    
            }

            Console.WriteLine($"N = {N},  S(N) = {count}");
            Console.ReadKey();
            
        }

        static int[] Shifter(int[] whoNeedToShift) {
            var res = new int[whoNeedToShift.Length]; ;
            for(int i = 0; i < whoNeedToShift.Length; i++) {
                if(whoNeedToShift[i] == 0)
                    continue;
                for(int j = 0; j < primes.Length; j++) {
                    if(i + primes[j] >= res.Length)
                        break;
                    res[i + primes[j]] = 1;
                }

            }
            return res;
        }

    }
}
