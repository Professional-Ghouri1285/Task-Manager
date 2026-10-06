namespace TaskManager.UITests.TaskCrudTests;

[TestFixture]
[NonParallelizable]
public class TaskCrudTests : TestFixture
{
    private ProjectDetailsPage _projectDetails = null!;

    [SetUp]
    public async Task TestSetUp()
    {
        await LoginAndNavigateToProjectAsync();
        _projectDetails = new ProjectDetailsPage(Page, Config);
        await _projectDetails.WaitForPageReadyAsync();
    }

    private async Task LoginAndNavigateToProjectAsync()
    {
        var loginPage = new LoginPage(Page, Config);
        await loginPage.GotoAsync();
        await loginPage.LoginAsync(Config.AdminEmail, Config.AdminPassword);

        await Assertions.Expect(Page).ToHaveURLAsync(new Regex(".*projects\\.html"));

        var projectsPage = new ProjectsPage(Page, Config);
        await projectsPage.WaitForProjectsToLoadAsync();
        await projectsPage.NavigateToProjectAsync(Config.TestProjectId);
    }

    private static string GenerateDescription() => $"UI-TEST-{Guid.NewGuid():N}";

    // ── Deliverable 4: Task CRUD Flow ──────────────────────────────────

    [Test]
    [Description("Creating a task via the UI makes it appear in the task table.")]
    public async Task CreateTask_VerifiesAppearsInTable()
    {
        var description = GenerateDescription();
        var initialCount = await _projectDetails.GetTaskCountAsync();

        await _projectDetails.CreateTaskAsync(description);

        Assert.IsTrue(await _projectDetails.IsTaskVisibleAsync(description),
            $"Created task '{description}' should be visible in the task table.");

        var newCount = await _projectDetails.GetTaskCountAsync();
        Assert.That(newCount, Is.EqualTo(initialCount + 1),
            "Task count badge should increment by one.");
    }

    [Test]
    [Description("Editing a task's description via the UI updates the table row.")]
    public async Task EditTask_VerifiesUpdatedDescription()
    {
        var original = GenerateDescription();
        var updated = $"UI-TEST-EDITED-{Guid.NewGuid():N}";

        await _projectDetails.CreateTaskAsync(original);
        await _projectDetails.EditTaskAsync(original, updated);

        Assert.IsTrue(await _projectDetails.IsTaskVisibleAsync(updated),
            $"Edited description '{updated}' should appear in the table.");
        Assert.IsFalse(await _projectDetails.IsTaskVisibleAsync(original),
            $"Original description '{original}' should no longer appear.");
    }

    [Test]
    [Description("Changing a task's status via the quick-update dropdown is reflected in the UI.")]
    public async Task ChangeTaskStatus_VerifiesUpdatedStatus()
    {
        var description = GenerateDescription();

        // Create with default status = Todo (0)
        await _projectDetails.CreateTaskAsync(description);

        // Verify initial status is Todo
        var (initialStatus, _) = await _projectDetails.GetTaskStatusAndPriorityAsync(description);
        Assert.That(int.Parse(initialStatus), Is.EqualTo(0), "Newly created task should have status Todo (0).");

        // Change to In Progress (1)
        await _projectDetails.ChangeTaskStatusAsync(description, ProjectDetailsPage.TaskStatusOption.InProgress);
        var (status1, _) = await _projectDetails.GetTaskStatusAndPriorityAsync(description);
        Assert.That(int.Parse(status1), Is.EqualTo(1), "Status should be In Progress (1) after change.");

        // Change to Completed (2)
        await _projectDetails.ChangeTaskStatusAsync(description, ProjectDetailsPage.TaskStatusOption.Completed);
        var (status2, _) = await _projectDetails.GetTaskStatusAndPriorityAsync(description);
        Assert.That(int.Parse(status2), Is.EqualTo(2), "Status should be Completed (2) after change.");
    }

    [Test]
    [Description("Deleting a task via the UI removes it from the task table.")]
    public async Task DeleteTask_VerifiesRemovedFromTable()
    {
        var description = GenerateDescription();
        await _projectDetails.CreateTaskAsync(description);

        Assert.IsTrue(await _projectDetails.IsTaskVisibleAsync(description),
            "Task should exist before deletion.");

        await _projectDetails.DeleteTaskAsync(description);

        Assert.IsFalse(await _projectDetails.IsTaskVisibleAsync(description),
            "Task should be removed from the table after deletion.");
    }

    [Test]
    [Description("A full task lifecycle — create, edit, status change, delete — completes successfully.")]
    public async Task FullLifecycle_CreateEditStatusDelete()
    {
        var description = GenerateDescription();
        var edited = $"{description} - Edited";

        // Create
        await _projectDetails.CreateTaskAsync(description);
        Assert.IsTrue(await _projectDetails.IsTaskVisibleAsync(description));

        // Edit
        await _projectDetails.EditTaskAsync(description, edited);
        Assert.IsTrue(await _projectDetails.IsTaskVisibleAsync(edited));

        // Change status
        await _projectDetails.ChangeTaskStatusAsync(edited, ProjectDetailsPage.TaskStatusOption.InProgress);
        var (status, _) = await _projectDetails.GetTaskStatusAndPriorityAsync(edited);
        Assert.That(int.Parse(status), Is.EqualTo(1), "Status should be In Progress.");

        // Delete
        await _projectDetails.DeleteTaskAsync(edited);
        Assert.IsFalse(await _projectDetails.IsTaskVisibleAsync(edited),
            "Task should be gone after deletion.");
    }

    [Test]
    [Description("Changing the priority during task creation is reflected in the table's priority badge.")]
    public async Task CreateTask_WithHighPriority_VerifiesInTable()
    {
        var description = GenerateDescription();

        await _projectDetails.CreateTaskAsync(description,
            ProjectDetailsPage.TaskStatusOption.Todo,
            ProjectDetailsPage.TaskPriorityOption.High);

        Assert.IsTrue(await _projectDetails.IsTaskVisibleAsync(description));

        var (_, priority) = await _projectDetails.GetTaskStatusAndPriorityAsync(description);
        StringAssert.Contains("High", priority, "Priority badge should show 'High'.");
    }
}
