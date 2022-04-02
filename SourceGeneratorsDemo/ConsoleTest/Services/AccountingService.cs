namespace ConsoleTest.Services;

public class AccountingService: IAccountingService
{
    public string GetClientAccounts(De client)
    {
        return client.A switch
        {
            "Petya" => "aaa",
            _=>"ddd"
        };
    }
}