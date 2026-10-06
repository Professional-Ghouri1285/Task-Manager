namespace TaskManager.UITests.LoginTests;

[TestFixture]
[NonParallelizable]
public class LoginTests : TestFixture
{
    private LoginPage _loginPage = null!;

    [SetUp]
    public async Task TestSetUp()
    {
        _loginPage = new LoginPage(Page, Config);
        await _loginPage.GotoAsync();
    }

    // ── Deliverable 3: Login Flow ──────────────────────────────────────

    [Test]
    [Description("Valid admin credentials are accepted and redirect to the projects page.")]
    public async Task ValidAdminCredentials_RedirectsToProjectsPage()
    {
        await _loginPage.LoginAsync(Config.AdminEmail, Config.AdminPassword);

        await Assertions.Expect(Page).ToHaveURLAsync(new Regex(".*projects\\.html"));
        var projectsPage = new ProjectsPage(Page, Config);
        Assert.IsTrue(await projectsPage.IsLoggedInAsync(),
            "User info badge should be visible after successful login.");
    }

    [Test]
    [Description("Valid manager credentials are accepted and redirect to the projects page.")]
    public async Task ValidManagerCredentials_RedirectsToProjectsPage()
    {
        await _loginPage.LoginAsync(Config.ManagerEmail, Config.ManagerPassword);

        await Assertions.Expect(Page).ToHaveURLAsync(new Regex(".*projects\\.html"));
        var badge = await Page.Locator("#user-info-badge").TextContentAsync();
        StringAssert.Contains("Manager", badge, "Role badge should show 'Manager'.");
    }

    [Test]
    [Description("Invalid email + password shows an error alert and stays on the login page.")]
    public async Task InvalidCredentials_ShowsErrorAlertAndStaysOnLoginPage()
    {
        await _loginPage.LoginAsync("nonexistent@test.local", "WrongPassword123!");

        // The frontend API.js maps every 401 to a warning alert.
        Assert.IsTrue(await _loginPage.HasErrorAlertAsync(), "An alert should be visible after failed login.");
        var alertText = await _loginPage.GetAlertMessageAsync();
        StringAssert.Contains("unauthenticated", alertText?.ToLowerInvariant(),
            "Alert should mention the unauthenticated/session issue.");

        // Must still be on the login page.
        StringAssert.Contains("login.html", _loginPage.CurrentUrl,
            "Should remain on the login page after invalid credentials.");
    }

    [Test]
    [Description("Valid email but wrong password shows an error and stays on login page.")]
    public async Task WrongPassword_ShowsErrorAlertAndStaysOnLoginPage()
    {
        await _loginPage.LoginAsync(Config.AdminEmail, "DefinitelyWrongPassword!");

        Assert.IsTrue(await _loginPage.HasErrorAlertAsync());
        StringAssert.Contains("login.html", _loginPage.CurrentUrl);
    }

    [Test]
    [Description("When a token already exists in localStorage, login.html redirects to projects.html.")]
    public async Task AlreadyAuthenticatedUser_RedirectsToProjectsPage()
    {
        // Obtain a valid token via API (decoupled from the UI login test).
        var api = new AuthApiClient(Config);
        var auth = await api.LoginAsync(Config.AdminEmail, Config.AdminPassword);
        Assert.That(auth, Is.Not.Null, "API login should succeed for seeding token.");

        var userJson = JsonSerializer.Serialize(new
        {
            userId = auth!.UserId.ToString(),
            name = auth.Name,
            email = auth.Email,
            role = auth.Role,
            organizationId = auth.OrganizationId.ToString()
        });

        // Pre-seed localStorage before the page loads so the
        // document-ready guard script redirects immediately.
        await _loginPage.PreSeedLocalStorageAsync(auth.Token, userJson);
        await _loginPage.GotoAsync();

        // The client-side redirect lands on projects.html.
        await Assertions.Expect(Page).ToHaveURLAsync(new Regex(".*projects\\.html"));
        Assert.IsTrue(await Page.Locator("#projects-grid").IsVisibleAsync());
    }

    [Test]
    [Description("Login button is disabled and spinner shown while the request is in flight, then re-enabled afterwards.")]
    public async Task LoginButton_DisabledDuringRequest_ThenReEnabled()
    {
        // Trigger login; immediately check that the button is disabled / spinner visible.
        // We do this by typing and then starting the click but waiting briefly.
        await Page.FillAsync("#email", Config.AdminEmail);
        await Page.FillAsync("#password", Config.AdminPassword);

        var clickTask = Page.Locator("#login-btn").ClickAsync();

        // During the request the button should be disabled and spinner visible.
        // Use a short wait + check.
        await Task.Delay(100);
        var duringDisabled = await Page.Locator("#login-btn").IsDisabledAsync();
        var duringSpinner = await Page.Locator("#btn-spinner").IsVisibleAsync();

        await clickTask;

        // After the redirect or error, button should be enabled.
        // Give a moment for the UI to settle.
        await Page.WaitForTimeoutAsync(500);
        var afterEnabled = await Page.Locator("#login-btn").IsEnabledAsync();

        Assert.Multiple(() =>
        {
            Assert.That(duringDisabled || duringSpinner, "Button should be disabled or spinner shown during request.");
            Assert.That(afterEnabled, "Button should be enabled after the login attempt completes.");
        });
    }

    [Test]
    [Description("Navigating directly to index.html redirects to login.html when no token is present.")]
    public async Task UnauthenticatedUser_AccessingRootRedirectsToLogin()
    {
        await Page.GotoAsync(Config.BaseUrl.TrimEnd('/') + "/");
        await Page.WaitForFunctionAsync("window.location.href.includes('login.html')",
            null,
            new PageWaitForFunctionOptions { Timeout = 5000 });
        StringAssert.Contains("login.html", Page.Url);
    }

    [Test]
    [Description("The 'Register here' link navigates to the registration page.")]
    public async Task RegisterLink_NavigatesToRegisterPage()
    {
        var navigationTask = Page.WaitForURLAsync(new Regex(".*register\\.html"));
        await Page.Locator("a[href='register.html']").ClickAsync();
        await navigationTask;
        StringAssert.Contains("register.html", Page.Url);
    }
}
