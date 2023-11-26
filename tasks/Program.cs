using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tasks
{
    internal class Program
    {
        void Commmand 


        static void Main(string[] args)
        {
            int actionCode = 1;

            while(actionCode != 0)
            {
                if(int.TryParse(Console.ReadLine(), out int ac))
                {
                    actionCode = ac;

                }
                else
                {
                    Console.WriteLine("Неверные ");
                }
            }
        }
    }
}
