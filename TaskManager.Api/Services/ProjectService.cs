using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Models;

namespace TaskManager.Services;

public class ProjectService : IProjectService
{
    private readonly AppDbContext _context;

    public ProjectService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Project>> GetAllProjects(Guid organizationId) =>
        await _context.Projects
            .Where(p => p.OrganizationId == organizationId)
            .Include(p => p.Owner)
            .Include(p => p.Members)
            .ToListAsync();

    public async Task<Project?> GetById(Guid organizationId, Guid id) =>
        await _context.Projects
            .Include(p => p.Owner)
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.OrganizationId == organizationId && p.Id == id);

    public async Task<Project> CreateProject(Project project)
    {
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();
        return project;
    }

    public async Task<bool> DeleteProject(Guid organizationId, Guid id)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.OrganizationId == organizationId && p.Id == id);
        if (project == null) return false;

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Project?> UpdateProject(Guid organizationId, Project incomingProject)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.OrganizationId == organizationId && p.Id == incomingProject.Id);

        if (project != null)
        {
            project.Name = incomingProject.Name;
            project.Description = incomingProject.Description;
            project.Status = incomingProject.Status;
            project.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        return project;
    }

    public async Task<bool> AddMember(Guid projectId, Guid userId)
    {
        var project = await _context.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == projectId);
        if (project == null) return false;

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return false;

        if (user.OrganizationId != project.OrganizationId) return false;

        if (project.Members.Any(m => m.UserId == userId)) return true;

        project.Members.Add(new ProjectMember { ProjectId = projectId, UserId = userId });
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveMember(Guid projectId, Guid userId)
    {
        var membership = await _context.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
        if (membership == null) return false;

        _context.ProjectMembers.Remove(membership);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<ProjectMember>> GetAllMembers(Guid projectId) =>
        await _context.ProjectMembers
            .Where(pm => pm.ProjectId == projectId)
            .Include(pm => pm.User)
            .ToListAsync();

    public async Task<Project?> ChangeOwner(Guid projectId, Guid newOwnerId)
    {
        var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
        if (project == null) return null;

        var newOwner = await _context.Users.FirstOrDefaultAsync(u => u.Id == newOwnerId);
        if (newOwner == null) return null;

        if (newOwner.OrganizationId != project.OrganizationId) return null;

        project.OwnerId = newOwnerId;
        project.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return project;
    }
}