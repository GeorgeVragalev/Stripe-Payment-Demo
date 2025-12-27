using System.Text.Json;
using PaymentDemo.Constants;
using PaymentDemo.Models;

namespace PaymentDemo.Services.Account;

public class UserContextService : IUserContextService
{
    private const string UserContextSessionKey = "UserContext";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContextService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public UserContextModel GetUserContext()
    {
        var session = GetSession();
        if (session == null)
            return new UserContextModel();

        var json = session.GetString(UserContextSessionKey);
        if (string.IsNullOrEmpty(json))
            return new UserContextModel();

        try
        {
            return JsonSerializer.Deserialize<UserContextModel>(json) ?? new UserContextModel();
        }
        catch
        {
            return new UserContextModel();
        }
    }

    public void SetCurrency(CurrencyType currency)
    {
        var userContext = GetUserContext();
        userContext.Currency = currency;
        SaveContext(userContext);
    }

    public CurrencyType GetCurrency()
    {
        return GetUserContext().Currency;
    }

    public string GetCurrencySymbol()
    {
        return GetCurrency().GetSymbol();
    }

    public string GetCurrencyCode()
    {
        return GetCurrency().GetCode();
    }

    public string FormatPrice(decimal amount)
    {
        return GetCurrency().FormatPrice(amount);
    }

    private void SaveContext(UserContextModel context)
    {
        var session = GetSession();
        if (session == null)
            return;

        var json = JsonSerializer.Serialize(context);
        session.SetString(UserContextSessionKey, json);
    }

    private ISession? GetSession()
    {
        return _httpContextAccessor.HttpContext?.Session;
    }
}
