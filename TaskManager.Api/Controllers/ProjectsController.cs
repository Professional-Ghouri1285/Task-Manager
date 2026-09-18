using Microsoft.AspNetCore.Mvc;
using TaskManager.Models;
using TaskManager.Services;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/organizations/{organizationId:guid}/projects")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(Guid organizationId) =>
        Ok(await _projectService.GetAllProjects(organizationId));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid organizationId, Guid id)
    {
        var project = await _projectService.GetById(organizationId, id);
        return project == null ? NotFound() : Ok(project);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid organizationId, Project newProject)
    {
        newProject.OrganizationId = organizationId;
        var created = await _projectService.CreateProject(newProject);
        return CreatedAtAction(nameof(GetById), new { organizationId, id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid organizationId, Guid id, Project incomingProject)
    {
        if (id != incomingProject.Id) return BadRequest();

        var existing = await _projectService.GetById(organizationId, id);
        if (existing == null) return NotFound();

        incomingProject.OrganizationId = organizationId;
        var updated = await _projectService.UpdateProject(organizationId, incomingProject);
        return updated == null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid organizationId, Guid id)
    {
        var existing = await _projectService.GetById(organizationId, id);
        if (existing == null) return NotFound();

        var success = await _projectService.DeleteProject(organizationId, id);
        return success ? NoContent() : NotFound();
    }

    [HttpGet("{projectId:guid}/members")]
    public async Task<IActionResult> GetMembers(Guid organizationId, Guid projectId)
    {
        var project = await _projectService.GetById(organizationId, projectId);
        if (project == null) return NotFound();

        var members = await _projectService.GetAllMembers(projectId);
        return Ok(members);
    }

    [HttpPost("{projectId:guid}/members")]
    public async Task<IActionResult> AddMember(Guid organizationId, Guid projectId, [FromBody] Guid userId)
    {
        var project = await _projectService.GetById(organizationId, projectId);
        if (project == null) return NotFound();

        var success = await _projectService.AddMember(projectId, userId);
        return success ? NoContent() : BadRequest("User not found or not in same organization");
    }

    [HttpDelete("{projectId:guid}/members/{userId:guid}")]
    public async Task<IActionResult> RemoveMember(Guid organizationId, Guid projectId, Guid userId)
    {
        var project = await _projectService.GetById(organizationId, projectId);
        if (project == null) return NotFound();

        var success = await _projectService.RemoveMember(projectId, userId);
        return success ? NoContent() : NotFound();
    }

    [HttpPut("{projectId:guid}/owner")]
    public async Task<IActionResult> ChangeOwner(Guid organizationId, Guid projectId, [FromBody] Guid newOwnerId)
    {
        var project = await _projectService.GetById(organizationId, projectId);
        if (project == null) return NotFound();

        var updated = await _projectService.ChangeOwner(projectId, newOwnerId);
        return updated == null ? NotFound() : Ok(updated);
    }
}