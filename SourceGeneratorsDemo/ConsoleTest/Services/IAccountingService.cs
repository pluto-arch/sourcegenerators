namespace ConsoleTest.Services
{
    [Log]
    public interface IAccountingService
    {
        string GetClientAccounts(string client);
    }
}

