namespace ConsoleTest.Services
{
    [Log]
    public interface IAccountingService
    {
        string GetClientAccounts(De client);
    }

    public class De
    {
        public string A { get; set; }
    }
}

