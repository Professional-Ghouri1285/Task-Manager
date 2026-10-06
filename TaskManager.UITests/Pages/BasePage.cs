namespace TaskManager.UITests.Pages;

/// <summary>
/// Abstract base class for all page objects in the POM layer.
/// Wraps an IPage and exposes common navigation + wait helpers so
/// individual page classes stay focused on their own domain actions.
/// </summary>
public abstract class BasePage
{
    protected readonly IPage Page;
    protected readonly TestSettings Settings;

    protected BasePage(IPage page, TestSettings settings)
    {
        Page = page;
        Settings = settings;
    }

    protected string BuildUrl(string relativePath = "")
    {
        var baseAddr = Settings.BaseUrl.TrimEnd('/');
        if (string.IsNullOrEmpty(relativePath))
            return baseAddr;
        return $"{baseAddr}/{relativePath.TrimStart('/')}";
    }

    protected async Task WaitForNetworkIdleAsync(int timeoutMs = 15000)
    {
        try
        {
            await Page.Locator("body").WaitForAsync(
                new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = timeoutMs });
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = timeoutMs });
        }
        catch (TimeoutException)
        {
            // Not all pages go to NetworkIdle (some end with active polling); ignore.
        }
    }

    protected async Task WaitForUrlAsync(string partialUrl, int timeoutMs = 15000)
    {
        await Page.WaitForFunctionAsync(
            $"window.location.href.includes('{partialUrl}')",
            null,
            new PageWaitForFunctionOptions { Timeout = timeoutMs });
    }

    // ------------------------------------------------------------------
    // Shared toast / alert helpers (present on every page via api.js)
    // ------------------------------------------------------------------

    private ILocator AlertLocator => Page.Locator("#alert-container .alert");

    protected async Task<bool> HasVisibleAlertAsync(int timeoutMs = 5000)
    {
        try
        {
            return await AlertLocator.IsVisibleAsync();
        }
        catch
        {
            return false;
        }
    }

    protected async Task<string?> GetAlertTextAsync(int timeoutMs = 5000)
    {
        try
        {
            await AlertLocator.First.WaitForAsync(
                new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = timeoutMs });
            return await AlertLocator.First.InnerTextAsync();
        }
        catch
        {
            return null;
        }
    }
}
