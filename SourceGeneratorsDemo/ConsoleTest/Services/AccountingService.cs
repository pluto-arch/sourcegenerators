namespace ConsoleTest.Services;

public class AccountingService: IAccountingService
{
    public string GetClientAccounts(string client)
    {
        return client switch
        {
            "Petya" => "aaa",
            _=>"ddd"
        };
    }
}