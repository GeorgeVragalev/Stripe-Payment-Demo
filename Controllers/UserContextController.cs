using Microsoft.AspNetCore.Mvc;
using PaymentDemo.Constants;
using PaymentDemo.Services.Account;

namespace PaymentDemo.Controllers;

/// <summary>
/// API controller for user context/preferences (currency, language, etc.)
/// </summary>
[ApiController]
[Route("api/user")]
public class UserContextController : ControllerBase
{
    private readonly IUserContextService _userContextService;

    public UserContextController(IUserContextService userContextService)
    {
        _userContextService = userContextService;
    }

    /// <summary>
    /// Get current user currency preference
    /// </summary>
    [HttpGet("currency")]
    public IActionResult GetCurrency()
    {
        var currency = _userContextService.GetCurrency();
        return Ok(new
        {
            currency = currency.ToString(),
            code = currency.GetCode(),
            symbol = currency.GetSymbol(),
            displayName = currency.GetDisplayName()
        });
    }

    /// <summary>
    /// Set user currency preference
    /// </summary>
    [HttpPost("currency")]
    public IActionResult SetCurrency([FromBody] SetCurrencyRequest request)
    {
        if (string.IsNullOrEmpty(request.Currency))
        {
            return BadRequest(new { error = "Currency is required" });
        }

        var currency = CurrencyInfo.FromCode(request.Currency);
        _userContextService.SetCurrency(currency);

        return Ok(new
        {
            success = true,
            currency = currency.ToString(),
            code = currency.GetCode(),
            symbol = currency.GetSymbol()
        });
    }

    /// <summary>
    /// Get all available currencies
    /// </summary>
    [HttpGet("currencies")]
    public IActionResult GetAvailableCurrencies()
    {
        var currencies = CurrencyInfo.GetAll().Select(c => new
        {
            value = c.ToString(),
            code = c.GetCode(),
            symbol = c.GetSymbol(),
            displayName = c.GetDisplayName(),
            dropdownDisplay = c.GetDropdownDisplay()
        });

        return Ok(currencies);
    }

    /// <summary>
    /// Get current payment mode setting
    /// </summary>
    [HttpGet("payment-mode")]
    public IActionResult GetPaymentMode()
    {
        var isTwoStep = _userContextService.IsSeparateAuthorizeAndCapture();
        return Ok(new
        {
            separateAuthorizeAndCapture = isTwoStep,
            description = isTwoStep
                ? "Two-step: Authorize during checkout, capture on confirmation"
                : "One-step: Automatic capture during checkout"
        });
    }

    /// <summary>
    /// Set payment mode (two-step vs automatic capture)
    /// </summary>
    [HttpPost("payment-mode")]
    public IActionResult SetPaymentMode([FromBody] SetPaymentModeRequest request)
    {
        _userContextService.SetSeparateAuthorizeAndCapture(request.SeparateAuthorizeAndCapture);

        return Ok(new
        {
            success = true,
            separateAuthorizeAndCapture = request.SeparateAuthorizeAndCapture
        });
    }
}

public class SetCurrencyRequest
{
    public string Currency { get; set; } = string.Empty;
}

public class SetPaymentModeRequest
{
    public bool SeparateAuthorizeAndCapture { get; set; }
}
