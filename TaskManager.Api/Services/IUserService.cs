using TaskManager.Models;

namespace TaskManager.Services;

public interface IUserService
{
    Task<User?> GetById(Guid id);
    Task<User?> GetByIdInOrganization(Guid id, Guid organizationId);
    Task<bool> ExistsInOrganization(Guid userId, Guid organizationId);
}