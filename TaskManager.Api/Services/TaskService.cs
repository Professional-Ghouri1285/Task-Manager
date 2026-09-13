using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Models;

namespace TaskManager.Services;

public class TaskService : ITaskService
{
    private readonly AppDbContext _context;

    public TaskService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<TaskItem>> GetAllByProject(Guid projectId) =>
        await _context.Tasks
            .Where(t => t.ProjectId == projectId)
            .Include(t => t.Project)
            .Include(t => t.AssignedTo)
            .Include(t => t.Comments)
            .ToListAsync();

    public async Task<TaskItem?> GetById(Guid id) =>
        await _context.Tasks
            .Include(t => t.Project)
            .Include(t => t.AssignedTo)
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<TaskItem> CreateTask(TaskItem newTask)
    {
        _context.Tasks.Add(newTask);
        await _context.SaveChangesAsync();
        return newTask;
    }

    public async Task<bool> DeleteTask(Guid id)
    {
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id);
        if (task == null) return false;

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<TaskItem?> UpdateTask(TaskItem incomingTask)
    {
        var existingTask = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == incomingTask.Id);

        if (existingTask != null)
        {
            existingTask.Description = incomingTask.Description;
            existingTask.Status = incomingTask.Status;
            existingTask.Priority = incomingTask.Priority;
            existingTask.DueDate = incomingTask.DueDate;
            existingTask.AssignedToId = incomingTask.AssignedToId;

            await _context.SaveChangesAsync();
        }

        return existingTask;
    }

    public async Task<TaskItem?> AssignTask(Guid id, Guid? userId)
    {
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id);

        if (task != null)
        {
            task.AssignedToId = userId;
            await _context.SaveChangesAsync();
        }

        return task;
    }

    public async Task<TaskItem?> ChangeStatus(Guid id, global::TaskManager.Models.TaskStatus status)
    {
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id);

        if (task != null)
        {
            task.Status = status;
            await _context.SaveChangesAsync();
        }

        return task;
    }
}