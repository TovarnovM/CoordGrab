using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zadacha {
    class Program {
        static void Main(string[] args) {
            int n = 0;
            for(int i = 0; i <= 8400000; i++) {
                if(CoolNumber(i))
                    n++;
            }
            Console.WriteLine(n);
            Console.ReadKey();
        }
        public static bool CoolNumber(long number) {
            var nums = new List<long>(8);
            while(number > 0) {
                nums.Add(number % 10l);
                number /= 10;
            }
            var coolCifra = new bool[nums.Count];

            for(int i = 0; i < nums.Count; i++) {
                coolCifra[i] = nums[i] == 0;
            }
            
            for(int i = 0; i < nums.Count; i++) {
                var sum = nums[i];
                for(int j = i+1; j < nums.Count; j++) {
                    sum += nums[j];
                    if(sum == 10) {
                        for(int k = i; k <= j; k++) {
                            coolCifra[k] = true;                            
                        }
                        break;
                    } else if(sum > 10) {
                        break;
                    }

                }
                if(!coolCifra[i])
                    return false;
                if(coolCifra[nums.Count-1])
                    return true;
            }
            for(int i = 0; i < nums.Count; i++) {
                if(!coolCifra[i])
                    return false;
            }
            return true;
        }
    }
}
