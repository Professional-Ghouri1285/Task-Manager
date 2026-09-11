namespace TaskManager.Models;

public enum UserRole
{
    Member,
    Manager,
    Admin
    
}

public enum ProjectStatus
{
    Active,
    Completed,
    Archived
}

public enum TaskStatus
{
    Todo,
    InProgress,
    Completed,
    Cancelled
}

public enum TaskPriority
{
    Medium, Low, High, Critical 
}
