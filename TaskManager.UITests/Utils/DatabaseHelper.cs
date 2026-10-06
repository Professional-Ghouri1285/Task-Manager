using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Models;

namespace TaskManager.UITests.Utils;

/// <summary>
/// Ensures the test database contains deterministic seed data (users with
/// specific roles and a shared test project) before UI tests execute.
/// Uses the same EF Core DbContext and PasswordHasher as the API so that
/// password hashes are fully compatible with the running backend.
/// </summary>
public class DatabaseHelper
{
    private readonly TestSettings _settings;
    private readonly PasswordHasher<User> _hasher;

    public DatabaseHelper(TestSettings settings)
    {
        _settings = settings;
        _hasher = new PasswordHasher<User>();
    }

    private AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_settings.ConnectionString)
            .Options;
        return new AppDbContext(options);
    }

    public async Task EnsureSeededAsync()
    {
        using var context = CreateContext();
        var orgId = _settings.OrganizationId;

        // --- Organization -------------------------------------------------
        if (!await context.Organizations.AnyAsync(o => o.Id == orgId))
        {
            context.Organizations.Add(new Organization { Id = orgId, Name = "Acme Corporation" });
            await context.SaveChangesAsync();
        }

        // --- Admin user ---------------------------------------------------
        var admin = await UpsertUserAsync(context, orgId, _settings.AdminUserId,
            "Admin User", _settings.AdminEmail, UserRole.Admin, _settings.AdminPassword);

        // --- Manager user -------------------------------------------------
        var manager = await UpsertUserAsync(context, orgId, _settings.ManagerUserId,
            "Manager User", _settings.ManagerEmail, UserRole.Manager, _settings.ManagerPassword);

        // --- Member user --------------------------------------------------
        await UpsertUserAsync(context, orgId, _settings.MemberUserId,
            "Member User", _settings.MemberEmail, UserRole.Member, _settings.MemberPassword);

        // --- Test project (owned by Admin) --------------------------------
        if (!await context.Projects.AnyAsync(p => p.Id == _settings.TestProjectId))
        {
            context.Projects.Add(new Project
            {
                Id = _settings.TestProjectId,
                OrganizationId = orgId,
                OwnerId = admin.Id,
                Name = "UI Test Project",
                Description = "Project created by UI automation tests",
                Status = ProjectStatus.Active
            });
            await context.SaveChangesAsync();
        }
        else
        {
            var project = await context.Projects.FirstAsync(p => p.Id == _settings.TestProjectId);
            project.OwnerId = admin.Id;
            project.OrganizationId = orgId;
            await context.SaveChangesAsync();
        }
    }

    private async Task<User> UpsertUserAsync(
        AppDbContext context, Guid orgId, Guid userId,
        string name, string email, UserRole role, string password)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
        var hash = _hasher.HashPassword(new User(), password);

        if (user is null)
        {
            user = new User
            {
                Id = userId,
                OrganizationId = orgId,
                Name = name,
                Email = email,
                Role = role,
                PasswordHash = hash
            };
            context.Users.Add(user);
        }
        else
        {
            user.Name = name;
            user.Role = role;
            user.PasswordHash = hash;
            user.OrganizationId = orgId;
        }

        await context.SaveChangesAsync();
        return user;
    }

    public async Task DeleteTestTasksAsync()
    {
        using var context = CreateContext();
        await context.Tasks
            .Where(t => t.Description.Contains("UI Test Task") ||
                        t.Description.Contains("UI-TEST-"))
            .ExecuteDeleteAsync();

        await context.Tasks
            .Where(t => t.Description.Contains("Edited"))
            .ExecuteDeleteAsync();
    }
}
