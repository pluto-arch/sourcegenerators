using System;
using ConsoleTest.Services;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var dd = new AccountingService().WithLogging();
            Console.WriteLine(dd.GetClientAccounts("11"));
            Console.ReadKey();
        }
    }

}