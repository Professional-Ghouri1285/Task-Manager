using TaskManager.Models;

namespace TaskManager.Services;

public interface IProjectService
{
    Task<List<Project>> GetAllProjects(Guid organizationId);
    Task<Project?> GetById(Guid organizationId, Guid id);
    Task<Project> CreateProject(Project project);
    Task<bool> DeleteProject(Guid organizationId, Guid id);
    Task<Project?> UpdateProject(Guid organizationId, Project incomingProject);
    Task<bool> AddMember(Guid projectId, Guid userId);
    Task<bool> RemoveMember(Guid projectId, Guid userId);
    Task<List<ProjectMember>> GetAllMembers(Guid projectId);
    Task<Project?> ChangeOwner(Guid projectId, Guid newOwnerId);
}