using TaskManager.Api.Data;
using Microsoft.EntityFrameworkCore;
using TaskManager.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddOpenApi();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICommentService, CommentService>();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var projectService = scope.ServiceProvider.GetRequiredService<IProjectService>();
    var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>();
    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
    var commentService = scope.ServiceProvider.GetRequiredService<ICommentService>();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();

