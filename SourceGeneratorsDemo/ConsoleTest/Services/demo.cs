using System;
using System.Diagnostics;

namespace ConsoleTest
{
    public static partial class LoggingExtensions
    {
        public static ConsoleTest.Services.IAccountingService WithLogging2(this ConsoleTest.Services.IAccountingService baseInterface) => new AccountingServiceLoggingProxy(baseInterface);
    }

    public class AccountingServiceLoggingProxy2 : ConsoleTest.Services.IAccountingService
    {
        private readonly ConsoleTest.Services.IAccountingService _target;
        public AccountingServiceLoggingProxy2(ConsoleTest.Services.IAccountingService target) => _target = target;
        string ConsoleTest.Services.IAccountingService.GetClientAccounts(System.String client)
        {
            Console.WriteLine($"method started Arguments: client = {client}");
            var sw = new Stopwatch();
            sw.Start();
            try
            {
                var result = _target.GetClientAccounts(client);
                Console.WriteLine($" GetClientAccounts finished in {sw.ElapsedMilliseconds} ms ");
                return result;
            }
            catch (Exception e) { Console.WriteLine($"GetClientAccounts has an error: {e.Message}"); throw; }
        }
    }
}