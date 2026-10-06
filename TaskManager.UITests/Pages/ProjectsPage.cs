namespace TaskManager.UITests.Pages;

/// <summary>
/// Page Object for the projects list page (wwwroot/projects.html).
/// </summary>
public sealed class ProjectsPage : BasePage
{
    private ILocator ProjectsGrid => Page.Locator("#projects-grid");
    private ILocator CreateProjectButton => Page.Locator("#open-create-modal-btn");
    private ILocator LogoutButton => Page.Locator("#logout-btn");
    private ILocator UserInfoBadge => Page.Locator("#user-info-badge");
    private ILocator UserName => Page.Locator("#nav-user-name");
    private ILocator UserRole => Page.Locator("#nav-user-role");

    public ProjectsPage(IPage page, TestSettings settings) : base(page, settings) { }

    public async Task GotoAsync()
    {
        await Page.GotoAsync(BuildUrl("projects.html"));
        await WaitForProjectsToLoadAsync();
    }

    public async Task WaitForProjectsToLoadAsync(int timeoutMs = 15000)
    {
        // The loading spinner is visible until the API call returns.
        var loading = Page.Locator("#projects-loading");
        try
        {
            await Assertions.Expect(loading).ToBeHiddenAsync(new() { Timeout = timeoutMs });
        }
        catch (TimeoutException) { /* loading might not have appeared */ }

        // Wait for project cards to appear (there should be at least one project)
        await Assertions.Expect(Page.Locator("#projects-grid .card").First)
            .ToBeVisibleAsync(new() { Timeout = timeoutMs });
    }

    public async Task<bool> IsLoggedInAsync() => await UserInfoBadge.IsVisibleAsync();

    public async Task<string?> GetLoggedInUserNameAsync() => await UserName.TextContentAsync();

    public async Task<string?> GetLoggedInUserRoleAsync() => await UserRole.TextContentAsync();

    public async Task<bool> HasProjectCardAsync(Guid projectId, int timeoutMs = 10000)
    {
        try
        {
            await ProjectsGrid
                .Locator($"a[href*='project-details.html?projectId={projectId}']")
                .WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = timeoutMs });
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }

    public async Task NavigateToProjectAsync(Guid projectId)
    {
        var link = ProjectsGrid
            .Locator($"a[href*='project-details.html?projectId={projectId}']");

        var waitForNavigation = Page.WaitForURLAsync(new Regex($".*project-details\\.html.*projectId={projectId}"));

        await link.First.ClickAsync();
        await waitForNavigation;
        await WaitForNetworkIdleAsync();
    }

    public async Task ClickLogoutAsync()
    {
        await LogoutButton.ClickAsync();
        await WaitForUrlAsync("login.html");
    }
}
