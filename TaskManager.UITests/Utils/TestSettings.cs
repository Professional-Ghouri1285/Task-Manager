using Microsoft.Extensions.Configuration;

namespace TaskManager.UITests.Utils;

public class TestSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public Guid OrganizationId { get; set; }
    public string AdminEmail { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;
    public string ManagerEmail { get; set; } = string.Empty;
    public string ManagerPassword { get; set; } = string.Empty;
    public string MemberEmail { get; set; } = string.Empty;
    public string MemberPassword { get; set; } = string.Empty;
    public Guid AdminUserId { get; set; }
    public Guid ManagerUserId { get; set; }
    public Guid MemberUserId { get; set; }
    public Guid TestProjectId { get; set; }
    public string ConnectionString { get; set; } = string.Empty;

    public static TestSettings Load()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.Test.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables("TEST_")
            .Build();

        var settings = config.GetSection("TestSettings").Get<TestSettings>();
        if (settings is null || string.IsNullOrEmpty(settings.BaseUrl))
        {
            throw new InvalidOperationException(
                "TestSettings not configured. Ensure appsettings.Test.json exists and contains TestSettings.");
        }

        return settings;
    }

    public Uri BuildUri(string relativePath = "")
    {
        var baseAddr = BaseUrl.TrimEnd('/');
        if (string.IsNullOrEmpty(relativePath))
            return new Uri(baseAddr);
        return new Uri($"{baseAddr}/{relativePath.TrimStart('/')}");
    }
}
