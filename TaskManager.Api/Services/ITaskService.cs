using TaskManager.Models;

namespace TaskManager.Services;

public interface ITaskService
{
    Task<List<TaskItem>> GetAllByProject(Guid projectId);
    Task<TaskItem?> GetById(Guid id);
    Task<TaskItem> CreateTask(TaskItem newTask);
    Task<bool> DeleteTask(Guid id);
    Task<TaskItem?> UpdateTask(TaskItem incomingTask);
    Task<TaskItem?> AssignTask(Guid id, Guid? userId);
    Task<TaskItem?> ChangeStatus(Guid id, global::TaskManager.Models.TaskStatus status);
}