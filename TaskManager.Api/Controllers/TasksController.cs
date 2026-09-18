using Microsoft.AspNetCore.Mvc;
using TaskManager.Models;
using TaskManager.Services;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/tasks")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(Guid projectId) =>
        Ok(await _taskService.GetAllByProject(projectId));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid projectId, Guid id)
    {
        var task = await _taskService.GetById(id);
        if (task == null || task.ProjectId != projectId) return NotFound();
        return Ok(task);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid projectId, TaskItem newTask)
    {
        newTask.ProjectId = projectId;
        var created = await _taskService.CreateTask(newTask);
        return CreatedAtAction(nameof(GetById), new { projectId, id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid projectId, Guid id, TaskItem incomingTask)
    {
        if (id != incomingTask.Id) return BadRequest();
        
        var existing = await _taskService.GetById(id);
        if (existing == null || existing.ProjectId != projectId) return NotFound();

        incomingTask.ProjectId = projectId;
        var updated = await _taskService.UpdateTask(incomingTask);
        return updated == null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid projectId, Guid id)
    {
        var existing = await _taskService.GetById(id);
        if (existing == null || existing.ProjectId != projectId) return NotFound();

        var success = await _taskService.DeleteTask(id);
        return success ? NoContent() : NotFound();
    }

    [HttpPut("{id:guid}/assign")]
    public async Task<IActionResult> Assign(Guid projectId, Guid id, [FromBody] Guid? userId)
    {
        var existing = await _taskService.GetById(id);
        if (existing == null || existing.ProjectId != projectId) return NotFound();

        var updated = await _taskService.AssignTask(id, userId);
        return updated == null ? NotFound() : Ok(updated);
    }

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid projectId, Guid id, [FromBody] global::TaskManager.Models.TaskStatus status)
    {
        var existing = await _taskService.GetById(id);
        if (existing == null || existing.ProjectId != projectId) return NotFound();

        var updated = await _taskService.ChangeStatus(id, status);
        return updated == null ? NotFound() : Ok(updated);
    }
}