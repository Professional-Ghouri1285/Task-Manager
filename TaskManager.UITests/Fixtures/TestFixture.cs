namespace TaskManager.UITests.Fixtures;

/// <summary>
/// Shared fixture for all UI test classes. Inherits from Playwright's
/// PageTest (which manages browser + per-test context/page lifecycle)
/// and adds deterministic database seeding before the test suite runs.
///
/// Why NUnit over xUnit?
///   - Playwright's official .NET templates default to NUnit.
///   - NUnit's separate [OneTimeSetUp]/[SetUp]/[TearDown] lifecycle maps
///     cleanly to Playwright's once-per-suite browser + per-test page model.
///   - [NonParallelizable] / [Parallelizable] give precise control over
///     concurrency — essential when tests share a PostgreSQL database
///     and must run sequentially to avoid data races on seed rows.
///   - NUnit 4's async setup support is first-class.
/// </summary>
[NonParallelizable]
public class TestFixture : PageTest
{
    protected TestSettings Config { get; private set; } = null!;

    [OneTimeSetUp]
    public async Task GlobalSetup()
    {
        Config = TestSettings.Load();
        var dbHelper = new DatabaseHelper(Config);
        await dbHelper.EnsureSeededAsync();
        await dbHelper.DeleteTestTasksAsync();
    }

    [OneTimeTearDown]
    public async Task GlobalTeardown()
    {
        if (Config is not null)
        {
            var dbHelper = new DatabaseHelper(Config);
            await dbHelper.DeleteTestTasksAsync();
        }
    }
}
