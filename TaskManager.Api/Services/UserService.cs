using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Models;

namespace TaskManager.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetById(Guid id) =>
        await _context.Users
            .Include(u => u.Organization)
            .FirstOrDefaultAsync(u => u.Id == id);

    public async Task<User?> GetByIdInOrganization(Guid id, Guid organizationId) =>
        await _context.Users
            .Include(u => u.Organization)
            .FirstOrDefaultAsync(u => u.Id == id && u.OrganizationId == organizationId);

    public async Task<bool> ExistsInOrganization(Guid userId, Guid organizationId) =>
        await _context.Users
            .AnyAsync(u => u.Id == userId && u.OrganizationId == organizationId);
}