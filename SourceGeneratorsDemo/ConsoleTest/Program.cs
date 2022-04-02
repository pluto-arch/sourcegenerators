using System;
using ConsoleTest.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var log = NullLogger.Instance;
            var dd = new AccountingService().WithLogging(log);
            Console.WriteLine(dd.GetClientAccounts(new De(){A = "123123"}));
            Console.ReadKey();
        }
    }

}