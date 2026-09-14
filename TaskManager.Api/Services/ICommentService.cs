using TaskManager.Models;

namespace TaskManager.Services;

public interface ICommentService
{
    Task<List<Comment>> GetByTaskId(Guid taskId);
    Task<Comment?> GetById(Guid id);
    Task<Comment> AddComment(Comment comment);
    Task<bool> DeleteComment(Guid id);
    Task<Comment?> UpdateComment(Guid id, string description);
}