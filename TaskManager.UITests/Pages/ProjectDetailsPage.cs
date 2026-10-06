namespace TaskManager.UITests.Pages;

/// <summary>
/// Page Object for the project workspace page (wwwroot/project-details.html).
/// Encapsulates all task CRUD interactions: create, edit, change status,
/// and delete — as well as read-side verification helpers.
/// </summary>
public sealed class ProjectDetailsPage : BasePage
{
    // --- static selectors ---
    private const string TaskTableBody = "#tasks-table-body";
    private const string TaskModal = "#taskModal";
    private const string DeleteModal = "#deleteTaskModal";
    private ILocator AlertContainer => Page.Locator("#alert-container");

    // --- form / modal fields ---
    private ILocator TaskIdHidden => Page.Locator("#task-id");
    private ILocator TaskDescription => Page.Locator("#task-desc");
    private ILocator TaskStatusSelect => Page.Locator("#task-status");
    private ILocator TaskPrioritySelect => Page.Locator("#task-priority");
    private ILocator TaskAssigneeSelect => Page.Locator("#task-assignee-select");
    private ILocator TaskAssigneeManual => Page.Locator("#task-assignee-manual");
    private ILocator TaskDueDate => Page.Locator("#task-duedate");
    private ILocator SaveTaskButton => Page.Locator("#save-task-btn");

    // --- action buttons ---
    private ILocator CreateTaskButton => Page.Locator("#open-create-task-btn");
    private ILocator ConfirmDeleteButton => Page.Locator("#confirm-delete-task-btn");
    private ILocator TaskCountBadge => Page.Locator("#task-count");

    // --- table rows ---
    private ILocator TaskRows => Page.Locator($"{TaskTableBody} tr");

    public ProjectDetailsPage(IPage page, TestSettings settings) : base(page, settings) { }

    public async Task GotoAsync(Guid projectId)
    {
        var url = BuildUrl($"project-details.html?projectId={projectId}");
        await Page.GotoAsync(url);
        await WaitForPageReadyAsync();
    }

    public async Task WaitForPageReadyAsync(int timeoutMs = 15000)
    {
        // Wait for project header to be populated
        await Page.Locator("#project-header-name").WaitForAsync(
            new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = timeoutMs });

        // Wait for tasks table to finish loading (loading placeholder disappears)
        var loadingRow = Page.Locator($"{TaskTableBody} tr").GetByText("Loading tasks");
        try
        {
            await Assertions.Expect(loadingRow).ToBeHiddenAsync(new() { Timeout = timeoutMs });
        }
        catch (TimeoutException)
        {
            // loading row might have already been replaced — not an error
        }
    }

    public async Task<int> GetTaskCountAsync()
    {
        var text = await TaskCountBadge.TextContentAsync();
        return int.TryParse(text, out var count) ? count : 0;
    }

    public async Task<bool> IsTaskVisibleAsync(string description)
    {
        var rows = await TaskRows.AllAsync();
        foreach (var row in rows)
        {
            var cellText = await row.InnerTextAsync();
            if (cellText.Contains(description))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Opens the create-task modal, fills the form, and saves.
    /// </summary>
    public async Task CreateTaskAsync(
        string description,
        TaskStatusOption status = TaskStatusOption.Todo,
        TaskPriorityOption priority = TaskPriorityOption.Medium,
        Guid? assigneeUserId = null,
        string? dueDateString = null)
    {
        await CreateTaskButton.ClickAsync();
        await Page.Locator(TaskModal).WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        await TaskDescription.FillAsync(description);
        await TaskStatusSelect.SelectOptionAsync(((int)status).ToString());
        await TaskPrioritySelect.SelectOptionAsync(((int)priority).ToString());

        if (!string.IsNullOrEmpty(dueDateString))
            await TaskDueDate.FillAsync(dueDateString);

        await SaveTaskButton.ClickAsync();

        // Wait for modal to close and table to reload
        await WaitForTasksToReloadAsync();
    }

    /// <summary>
    /// Opens the edit modal for the task matching the given description,
    /// updates its fields, and saves.
    /// </summary>
    public async Task EditTaskAsync(
        string currentDescription,
        string newDescription,
        TaskStatusOption? status = null,
        TaskPriorityOption? priority = null)
    {
        var row = await FindTaskRowAsync(currentDescription);
        if (row is null)
            throw new InvalidOperationException($"Task with description '{currentDescription}' not found");

        var editButton = row.Locator(".edit-task-btn");
        await editButton.ClickAsync();
        await Page.Locator(TaskModal).WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        await TaskDescription.FillAsync(newDescription);
        if (status.HasValue)
            await TaskStatusSelect.SelectOptionAsync(((int)status.Value).ToString());
        if (priority.HasValue)
            await TaskPrioritySelect.SelectOptionAsync(((int)priority.Value).ToString());

        await SaveTaskButton.ClickAsync();
        await WaitForTasksToReloadAsync();
    }

    /// <summary>
    /// Changes the status of an existing task using the quick-update dropdown
    /// in the task table row.
    /// </summary>
    public async Task ChangeTaskStatusAsync(string description, TaskStatusOption newStatus)
    {
        var row = await FindTaskRowAsync(description);
        if (row is null)
            throw new InvalidOperationException($"Task with description '{description}' not found");

        var statusSelect = row.Locator(".task-status-select");
        await statusSelect.SelectOptionAsync(((int)newStatus).ToString());

        await WaitForTasksToReloadAsync();
    }

    public async Task<(string status, string priority)> GetTaskStatusAndPriorityAsync(string description)
    {
        var row = await FindTaskRowAsync(description);
        if (row is null)
            throw new InvalidOperationException($"Task with description '{description}' not found");

        var statusSelect = row.Locator(".task-status-select");
        var priorityBadge = row.Locator(".badge");

        var status = await statusSelect.InputValueAsync();
        var priority = await priorityBadge.TextContentAsync();

        return (status, priority!);
    }

    /// <summary>
    /// Deletes the task whose row matches the given description.
    /// </summary>
    public async Task DeleteTaskAsync(string description)
    {
        var row = await FindTaskRowAsync(description);
        if (row is null)
            throw new InvalidOperationException($"Task with description '{description}' not found");

        var deleteButton = row.Locator(".delete-task-btn");
        await deleteButton.ClickAsync();
        await Page.Locator(DeleteModal).WaitForAsync(
            new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        await ConfirmDeleteButton.ClickAsync();
        await WaitForTasksToReloadAsync();
    }

    /// <summary>
    /// Returns the task row whose text content contains the given description.
    /// Returns null if no matching row exists.
    /// </summary>
    private async Task<ILocator?> FindTaskRowAsync(string description)
    {
        var allRows = await TaskRows.AllAsync();
        foreach (var row in allRows)
        {
            var text = await row.InnerTextAsync();
            if (text.Contains(description))
                return row;
        }
        return null;
    }

    private async Task WaitForTasksToReloadAsync(int timeoutMs = 15000)
    {
        // Wait for modal to close
        try
        {
            await Page.Locator(TaskModal).Locator(".modal-dialog")
                .WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden, Timeout = 3000 });
        }
        catch (TimeoutException) { /* modal might have already closed */ }

        // Wait for network to settle (save/PUT/DELETE request + subsequent loadTasks GET)
        try
        {
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = timeoutMs });
        }
        catch (TimeoutException) { /* fall back to element-level waits */ }

        // Ensure the "Loading tasks…" placeholder is gone
        try
        {
            await Assertions.Expect(Page.Locator($"{TaskTableBody}").GetByText("Loading tasks"))
                .ToBeHiddenAsync(new() { Timeout = 5000 });
        }
        catch (TimeoutException) { /* placeholder never appeared */ }

        // Ensure at least one row is visible (task rows or empty-state row)
        await Assertions.Expect(TaskRows.First).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    // --- status / priority option enums matching the HTML select values ---

    public enum TaskStatusOption
    {
        Todo = 0,
        InProgress = 1,
        Completed = 2,
        Cancelled = 3
    }

    public enum TaskPriorityOption
    {
        Medium = 0,
        Low = 1,
        High = 2,
        Critical = 3
    }
}
