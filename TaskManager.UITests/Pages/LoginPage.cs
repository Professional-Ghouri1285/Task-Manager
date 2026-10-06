using System.Text.Json;

namespace TaskManager.UITests.Pages;

/// <summary>
/// Page Object for the login screen (wwwroot/login.html).
/// </summary>
public sealed class LoginPage : BasePage
{
    // --- element locators (lazily resolved) ---
    private ILocator EmailInput => Page.Locator("#email");
    private ILocator PasswordInput => Page.Locator("#password");
    private ILocator LoginButton => Page.Locator("#login-btn");
    private ILocator ButtonText => Page.Locator("#btn-text");
    private ILocator Spinner => Page.Locator("#btn-spinner");
    private ILocator RegisterLink => Page.Locator("a[href='register.html']");

    public LoginPage(IPage page, TestSettings settings) : base(page, settings) { }

    public async Task GotoAsync()
    {
        await Page.GotoAsync(BuildUrl("login.html"));
        await WaitForNetworkIdleAsync();
    }

    /// <summary>
    /// Fills the email and password fields and submits the login form.
    /// Waits for the client-side redirect (if successful) or for the
    /// button to be re-enabled (if the credentials are invalid).
    /// </summary>
    public async Task LoginAsync(string email, string password)
    {
        await EmailInput.FillAsync(email);
        await PasswordInput.FillAsync(password);

        // Click and wait for either a navigation or the button to be re-enabled
        var clickTask = LoginButton.ClickAsync();
        await clickTask;

        // Wait for either a redirect (success) or the button to re-enable (failure)
        await WaitForLoginOutcomeAsync();
    }

    private async Task WaitForLoginOutcomeAsync(int timeoutMs = 10000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < deadline)
        {
            // Success path: URL changed to projects.html
            if (Page.Url.Contains("projects.html"))
                return;

            // Failure path: button is re-enabled (login attempt finished)
            if (await LoginButton.IsEnabledAsync() && !await Spinner.IsVisibleAsync())
                return;

            await Task.Delay(200);
        }
    }

    public async Task<bool> IsLoginButtonEnabledAsync() => await LoginButton.IsEnabledAsync();

    public async Task<bool> IsSpinnerVisibleAsync() => await Spinner.IsVisibleAsync();

    public async Task<bool> IsFormVisibleAsync() => await EmailInput.IsVisibleAsync();

    public async Task<bool> HasErrorAlertAsync(int timeoutMs = 5000)
    {
        return await HasVisibleAlertAsync(timeoutMs);
    }

    public async Task<string?> GetAlertMessageAsync(int timeoutMs = 5000)
    {
        return await GetAlertTextAsync(timeoutMs);
    }

    public string CurrentUrl => Page.Url;

    /// <summary>
    /// Pre-seeds localStorage with a valid token so the page-level
    /// "already logged in" guard redirects immediately.
    /// Must be called before GotoAsync.
    /// </summary>
    public async Task PreSeedLocalStorageAsync(string token, string userJson)
    {
        var script = string.Join('\n',
            "localStorage.setItem('token', " + JsonSerializer.Serialize(token) + ");",
            "localStorage.setItem('user', " + JsonSerializer.Serialize(userJson) + ");");
        await Page.AddInitScriptAsync(script);
    }
}
