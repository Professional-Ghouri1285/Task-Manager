# TaskManager

A full-stack task management web application with role-based access control, project management, task tracking, and comprehensive automated + manual test coverage.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Setup & Installation](#setup--installation)
- [Configuration](#configuration)
- [Running the Application](#running-the-application)
- [API Documentation](#api-documentation)
- [Frontend Pages](#frontend-pages)
- [Authentication & Authorization](#authentication--authorization)
- [Testing](#testing)
  - [Playwright UI Tests](#playwright-ui-tests)
  - [Postman/Newman API Tests](#postmannewman-api-tests)
  - [Test Reports](#test-reports)
  - [Manual Test Cases](#manual-test-cases)
  - [Database Verification](#database-verification)
- [Database Schema](#database-schema)
- [Known Issues](#known-issues)
- [Contributing](#contributing)

---

## Overview

TaskManager is a web-based project and task management system supporting organizations with multiple users, projects, and tasks. It provides:

- **Organization-scoped** data isolation
- **Role-based access control**: Admin, Manager, Member
- **Project management**: create, update, delete, ownership transfer, membership management
- **Task management**: CRUD, status changes, assignee management, priority tracking
- **JWT-based authentication** with persistent sessions via localStorage
- **Responsive web UI** built with Bootstrap 5

---

## Features

### Core Features
- User registration and authentication (JWT)
- Organization-scoped project management
- Project membership (add/remove members)
- Project ownership transfer
- Task CRUD with status tracking and priority levels
- Task assignment to users
- Responsive single-page frontend with vanilla JS

### Access Control
| Role | Projects | Tasks | Members | Owner |
|------|----------|-------|---------|-------|
| **Admin** | Full CRUD | Full CRUD | Add/Remove | Change owner |
| **Manager** | Full CRUD | Full CRUD | Add/Remove | — |
| **Member** | Read-only | Read-only | — | — |

---

## Tech Stack

### Backend
| Component | Technology |
|-----------|-----------|
| Runtime | .NET 10 |
| Framework | ASP.NET Core Web API |
| ORM | Entity Framework Core 10.0.11 |
| Database | PostgreSQL (Npgsql.EntityFrameworkCore.PostgreSQL 10.0.3) |
| Authentication | JWT Bearer (Microsoft.IdentityModel.Tokens) |
| OpenAPI | Swashbuckle / OpenAPI (built-in .NET 10) |
| Serialization | System.Text.Json (ReferenceHandler.IgnoreCycles) |

### Frontend
| Component | Technology |
|-----------|-----------|
| Pages | Static HTML5 |
| Styling | Bootstrap 5.3.2 |
| Icons | Bootstrap Icons 1.11.1 |
| Logic | Vanilla JavaScript (ES6+) |

### Testing
| Layer | Tools |
|-------|-------|
| UI Automation | Playwright for .NET (1.52.0) + NUnit 3.13.2 |
| API Automation | Postman Collection + Newman |
| Manual | Structured test case document |

---

## Architecture

```
┌─────────────────────────────────────────────────────┐
│                   Frontend (wwwroot)                 │
│  ┌──────────┬──────────────────┬─────────────────┐ │
│  │ login.html │ projects.html │ project-details │ │
│  │ register.html │ index.html │ api.js (client) │ │
│  └──────────┴──────────────────┴─────────────────┘ │
└─────────────────────────────────────────────────────┘
                        │ HTTPS / HTTP
                        ▼
┌─────────────────────────────────────────────────────┐
│              ASP.NET Core Web API                    │
│  ┌───────────────────────────────────────────────┐  │
│  │  Controllers                                   │  │
│  │  - AuthController    (/api/auth)                │  │
│  │  - ProjectsController (/api/organizations/...) │  │
│  │  - TasksController   (/api/projects/...)       │  │
│  └───────────────────────────────────────────────┘  │
│  ┌───────────────────────────────────────────────┐  │
│  │  Services (Business Logic)                     │  │
│  │  - AuthService, ProjectService, TaskService,   │  │
│  │    UserService, CommentService                 │  │
│  └───────────────────────────────────────────────┘  │
│  ┌───────────────────────────────────────────────┐  │
│  │  Data Layer                                    │  │
│  │  - AppDbContext (EF Core)                      │  │
│  │  - Migrations                                  │  │
│  └───────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────┐
│              PostgreSQL Database                    │
│  Tables: organizations, users, projects,            │
│          project_members, tasks, comments            │
└─────────────────────────────────────────────────────┘
```

---

## Project Structure

```
Task Manager/
├── TaskManager.Api/                    # ASP.NET Core Web API
│   ├── Controllers/
│   │   ├── AuthController.cs          # /api/auth (register, login)
│   │   ├── ProjectsController.cs      # /api/organizations/{orgId}/projects
│   │   └── TasksController.cs         # /api/projects/{projectId}/tasks
│   ├── Services/
│   │   ├── AuthService.cs
│   │   ├── ProjectService.cs
│   │   ├── TaskService.cs
│   │   ├── UserService.cs
│   │   └── CommentService.cs
│   ├── Models/
│   │   ├── Organization.cs
│   │   ├── User.cs
│   │   ├── Project.cs
│   │   ├── ProjectMember.cs
│   │   ├── TaskItem.cs
│   │   ├── Comment.cs
│   │   └── Enums.cs
│   ├── Data/
│   │   └── AppDbContext.cs
│   ├── DTOs/
│   │   └── AuthDTOs.cs
│   ├── Migrations/
│   ├── wwwroot/                       # Frontend static files
│   │   ├── index.html
│   │   ├── login.html
│   │   ├── register.html
│   │   ├── projects.html
│   │   ├── project-details.html
│   │   ├── css/
│   │   └── js/
│   │       ├── api.js                 # Shared API client
│   │       ├── auth.js
│   │       ├── login.js
│   │       ├── projects.js
│   │       └── project-details.js
│   ├── Program.cs                     # App entry point, DI, CORS, JSON config
│   ├── appsettings.json               # Connection strings, JWT settings
│   └── TaskManager.Api.csproj
│
├── TaskManager.UITests/               # Playwright UI test automation
│   ├── Fixtures/
│   │   └── TestFixture.cs             # Browser setup/teardown, test server
│   ├── Pages/                         # Page Object Model
│   │   ├── BasePage.cs
│   │   ├── LoginPage.cs
│   │   ├── ProjectsPage.cs
│   │   └── ProjectDetailsPage.cs
│   ├── LoginTests/
│   │   └── LoginTests.cs              # Login flow tests
│   ├── TaskCrudTests/
│   │   └── TaskCrudTests.cs           # Task CRUD tests
│   ├── Utils/
│   │   ├── TestSettings.cs            # Environment configuration
│   │   ├── DatabaseHelper.cs          # DB seeding/cleanup for tests
│   │   └── AuthApiClient.cs           # Authenticated API client
│   ├── GlobalUsings.cs
│   └── TaskManager.UITests.csproj
│
├── TaskManager.postman_collection.json # Postman/Newman API test collection
├── TaskManager.postman_environment.json # Postman environment variables
├── MANUAL_TEST_CASES.md               # Formal manual test case document (34 cases)
├── DATABASE_VERIFICATION.md            # DB behavior verification guide
├── newman-report.html                  # Newman API test HTML report
├── playwright-report.html              # Playwright UI test HTML report
└── README.md                           # This file
```

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js](https://nodejs.org/) (for Newman)
- PostgreSQL 15+ running locally
- Playwright browsers (auto-installed on first test run)

---

## Setup & Installation

### 1. Clone the Repository

```bash
git clone <repository-url>
cd "Task Manager"
```

### 2. Restore NuGet Packages

```bash
cd TaskManager.Api
dotnet restore
cd ../TaskManager.UITests
dotnet restore
```

### 3. Install Newman (for API test automation)

```bash
npm install --save-dev newman newman-reporter-html
```

---

## Configuration

### Database

The API connects to PostgreSQL using the connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=task_manager;Username=postgres;Password=12061567"
  }
}
```

Update these values to match your PostgreSQL instance.

### JWT Settings

JWT configuration in `appsettings.json`:

```json
{
  "Jwt": {
    "Key": "X8vL2mP9qR4wE7zY1uI6oP0aS3dF5gH8jK2lN4mQ7rT9vW2xZ5yA8bC1dE4fG7h",
    "Issuer": "TaskManager",
    "Audience": "TaskManagerClients",
    "ExpiryMinutes": 60
  }
}
```

> **Note:** The JWT key above is a development/testing key. Use a secure, randomly generated key in production.

### Test Settings

UI test configuration in `TaskManager.UITests/appsettings.Test.json`:

```json
{
  "TestSettings": {
    "BaseUrl": "http://localhost:5059",
    "OrganizationId": "11111111-1111-1111-1111-111111111111",
    "AdminEmail": "admin@test.local",
    "AdminPassword": "AdminPass123!",
    "ManagerEmail": "manager@test.local",
    "ManagerPassword": "ManagerPass123!",
    "MemberEmail": "member@test.local",
    "MemberPassword": "MemberPass123!",
    "TestProjectId": "22222222-0000-0000-0000-000000000001"
  }
}
```

---

## Running the Application

### Start the API Server

```bash
cd TaskManager.Api
dotnet run --urls "http://localhost:5059"
```

The API will be available at `http://localhost:5059`.

### Access the Frontend

Open your browser and navigate to:

- `http://localhost:5059/` — Root redirects based on auth status
- `http://localhost:5059/login.html` — Login page
- `http://localhost:5059/register.html` — Registration page
- `http://localhost:5059/projects.html` — Projects dashboard (requires auth)
- `http://localhost:5059/project-details.html` — Project detail view (requires auth)

### First-Run Seeding

On first run, the API automatically seeds the database with:

- An **organization**: `Acme Corporation` (`11111111-1111-1111-1111-111111111111`)
- **Test users** (if not already present):
  - Admin: `admin@test.local` / `AdminPass123!`
  - Manager: `manager@test.local` / `ManagerPass123!`
  - Member: `member@test.local` / `MemberPass123!`

---

## API Documentation

### Base URL

```
http://localhost:5059
```

### Authentication Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/auth/register` | None | Register a new user |
| POST | `/api/auth/login` | None | Login, returns JWT |

#### Register Request Body
```json
{
  "email": "user@test.local",
  "password": "SecurePass123!",
  "name": "User Name",
  "organizationId": "11111111-1111-1111-1111-111111111111",
  "jobTitle": "Developer"
}
```

#### Login Request Body
```json
{
  "email": "admin@test.local",
  "password": "AdminPass123!"
}
```

#### Login Response
```json
{
  "userId": "11111111-1111-1111-1111-111111111101",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "admin@test.local",
  "name": "Admin User",
  "role": "Admin",
  "organizationId": "11111111-1111-1111-1111-111111111111"
}
```

### Project Endpoints

All project endpoints require JWT authentication. Create/Update/Delete require Admin or Manager role.

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/organizations/{orgId}/projects` | Admin, Manager | Create a project |
| GET | `/api/organizations/{orgId}/projects` | Any authenticated | List all projects |
| GET | `/api/organizations/{orgId}/projects/{id}` | Any authenticated | Get project by ID |
| PUT | `/api/organizations/{orgId}/projects/{id}` | Admin, Manager | Update a project |
| DELETE | `/api/organizations/{orgId}/projects/{id}` | Admin, Manager | Delete a project |
| POST | `/api/organizations/{orgId}/projects/{id}/members` | Admin, Manager | Add member |
| DELETE | `/api/organizations/{orgId}/projects/{id}/members/{userId}` | Admin, Manager | Remove member |
| PUT | `/api/organizations/{orgId}/projects/{id}/owner` | Admin, Manager | Change owner |

### Task Endpoints

All task endpoints require JWT authentication. Create/Update/Delete/Assign require Admin or Manager role.

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/projects/{projectId}/tasks` | Admin, Manager | Create a task |
| GET | `/api/projects/{projectId}/tasks` | Any authenticated | List all tasks |
| GET | `/api/projects/{projectId}/tasks/{id}` | Any authenticated | Get task by ID |
| PUT | `/api/projects/{projectId}/tasks/{id}` | Admin, Manager | Update a task |
| DELETE | `/api/projects/{projectId}/tasks/{id}` | Admin, Manager | Delete a task |
| PUT | `/api/projects/{projectId}/tasks/{id}/assign` | Admin, Manager | Assign task to user |
| PUT | `/api/projects/{projectId}/tasks/{id}/status` | Any authenticated | Change task status |

### Error Responses

| Status | Meaning | Example |
|--------|---------|---------|
| 400 Bad Request | Invalid input | Duplicate email, validation failure |
| 401 Unauthorized | Missing or invalid JWT | Expired token, malformed token |
| 403 Forbidden | Insufficient role | Member trying to create project |
| 404 Not Found | Resource not found | Non-existent project ID |
| 500 Internal Server Error | Server error | Database connection failure |

---

## Frontend Pages

### Page Structure

| Page | Route | Auth Required | Description |
|------|-------|---------------|-------------|
| `index.html` | `/` | No | Root redirector (login or projects based on token) |
| `login.html` | `/login.html` | No | Email/password login form |
| `register.html` | `/register.html` | No | New user registration |
| `projects.html` | `/projects.html` | Yes | Project dashboard with cards |
| `project-details.html` | `/project-details.html` | Yes | Single project view with task table |

### Client-Side Routing

The frontend uses simple JS-based routing with `localStorage` for auth state:

- `localStorage.token` — JWT string
- `localStorage.user` — Serialized user object (name, email, role)

### Key JavaScript Modules

| File | Responsibility |
|------|---------------|
| `js/api.js` | Shared API client with `getToken()`, `setToken()`, authenticated fetch wrappers |
| `js/auth.js` | Login/logout handlers, token management, user info display |
| `js/login.js` | Login form validation, error display, redirect logic |
| `js/projects.js` | Project CRUD via API, card rendering, empty state handling |
| `js/project-details.js` | Task table rendering, inline editing, status changes, task CRUD |

---

## Authentication & Authorization

### JWT Flow

1. User submits login credentials to `/api/auth/login`
2. Server validates credentials and returns a JWT
3. Client stores JWT in `localStorage.token`
4. All subsequent API requests include `Authorization: Bearer <token>` header
5. Server validates JWT on each request using `JwtBearer` middleware
6. If token is expired or invalid, server returns 401, client redirects to login

### Role-Based Access

| Role | Create Project | Create Task | Manage Members | Change Owner |
|------|---------------|-------------|----------------|--------------|
| **Admin** | ✅ | ✅ | ✅ | ✅ |
| **Manager** | ✅ | ✅ | ✅ | ❌ |
| **Member** | ❌ | ❌ | ❌ | ❌ |

Access control is enforced via:
- `[Authorize(Roles = "Admin,Manager")]` attributes on controller actions
- Service-layer validation for ownership and membership checks

---

## Testing

### Playwright UI Tests

**Location:** `TaskManager.UITests/`

**Test Runner:** NUnit 3.13.2

**Why NUnit 3:** `Microsoft.Playwright.NUnit 1.52.0` targets NUnit 3. NUnit 4 removed legacy assertion APIs (`Assert.IsTrue`, `StringAssert`) incompatible with the Playwright adapter.

#### Running UI Tests

```bash
# Build the test project
cd TaskManager.UITests
dotnet build

# Run all tests
dotnet test

# Run with detailed console output
dotnet test --logger "console;verbosity=detailed"

# Run a specific test class
dotnet test --filter "FullyQualifiedName~LoginTests"

# Run a specific test
dotnet test --filter "FullyQualifiedName~ValidAdminCredentials_RedirectsToProjectsPage"
```

#### Test Structure

```
TaskManager.UITests/
├── Fixtures/
│   └── TestFixture.cs          # Browser lifecycle, test server management
├── Pages/                      # Page Object Model
│   ├── BasePage.cs             # Common page operations
│   ├── LoginPage.cs            # Login page interactions
│   ├── ProjectsPage.cs         # Projects page interactions
│   └── ProjectDetailsPage.cs   # Project details page interactions
├── LoginTests/
│   └── LoginTests.cs           # 7 login flow test cases
├── TaskCrudTests/
│   └── TaskCrudTests.cs        # 6 task CRUD test cases
└── Utils/
    ├── TestSettings.cs         # Environment config
    ├── DatabaseHelper.cs       # DB setup/cleanup
    └── AuthApiClient.cs        # Authenticated API client
```

#### Test Coverage

| Category | Tests | Scenarios |
|----------|-------|-----------|
| **Login Flow** | 7 | Valid admin/manager login, invalid credentials, wrong password, already authenticated redirect, unauthenticated root redirect, button state during request, register link navigation |
| **Task CRUD** | 6 | Create task, edit task, change status, delete task, full lifecycle, priority badge |

**Total:** 14 automated UI test cases

### Postman/Newman API Tests

**Collection File:** `TaskManager.postman_collection.json`  
**Environment File:** `TaskManager.postman_environment.json`

The collection covers all API endpoints with success and failure cases.

#### Running API Tests

```bash
# Run with Newman
npx newman run TaskManager.postman_collection.json \
  -e TaskManager.postman_environment.json

# Run with HTML reporter
npx newman run TaskManager.postman_collection.json \
  -e TaskManager.postman_environment.json \
  --reporters html,cli \
  --reporter-html-export newman-report.html
```

#### Test Coverage

| Category | Requests | Assertions | Scenarios |
|----------|----------|------------|-----------|
| **Auth** | 6 | 12 | Login (admin/manager/member), invalid credentials, register (success/duplicate) |
| **Projects CRUD** | 7 | 14 | Create, list, get by ID, update, add member, get members, change owner, remove member |
| **Projects Error Cases** | 4 | 4 | 401 unauthorized, 403 forbidden (wrong role), 404 not found, wrong organization |
| **Tasks CRUD** | 7 | 14 | Create, list, get by ID, update, assign, change status, delete |
| **Tasks Error Cases** | 3 | 3 | 401 unauthorized, 404 not found, wrong project |
| **Database Behavior** | 10 | 21 | Persistence checks, delete cascades, membership lifecycle |
| **Cleanup** | 2 | 0 | Delete test projects |
| **Total** | **43** | **68** | — |

### Test Reports

Two separate HTML reports are generated after test execution:

| Report | File | Description |
|--------|------|-------------|
| **Newman API Report** | `newman-report.html` | Full Postman/Newman run with 43 requests, 68 assertions |
| **Playwright UI Report** | `playwright-report.html` | NUnit TRX converted to HTML, 14 tests |

Open these files in a browser to view detailed results, error messages, and stack traces.

> **Note:** These are separate reports because they come from fundamentally different test runners. Newman generates its own HTML report from Postman collection results. Playwright/NUnit outputs TRX format which is converted to HTML via a custom script.

### Manual Test Cases

**Document:** `MANUAL_TEST_CASES.md`

A formal manual test case document with 34 structured test cases suitable for portfolio review or QA onboarding.

| Category | Test Cases |
|----------|-----------|
| Authentication | 7 (valid login per role, invalid credentials, session expiry, redirect) |
| Authorization & Role Permissions | 6 (create-project restrictions, unauthenticated access, direct API checks) |
| Project CRUD | 5 (create, view, update, delete, non-existent project) |
| Task CRUD | 6 (create, edit, status change, delete, full lifecycle, assignee) |
| Membership | 3 (add, remove, change owner) |
| Error Handling & Edge Cases | 4 (empty fields, duplicate names, empty description, long description) |
| Redirect Behavior | 3 (root path redirects, authenticated/unauthenticated) |

### Database Verification

**Document:** `DATABASE_VERIFICATION.md`

Documents verification of database behavior through API responses. Flags 7 checks that genuinely require direct SQL:

1. **FK Cascade:** Project → Tasks (deleting a project removes its tasks)
2. **FK Cascade:** Project → Members (deleting a project removes memberships)
3. **FK Set-Null:** Task → Assigned User (deleting assigned user sets `assigned_to_id` to NULL)
4. **FK Restrict:** Project Owner (deleting an owner is rejected by the database)
5. **Unique Constraint:** `uq_users_organization_id_id` (composite unique on organization + user ID)
6. **Check Constraint:** `chk_users_role` (only Admin/Manager/Member allowed)
7. **Default Values:** `created_at` / `updated_at` / `status` column defaults

---

## Database Schema

### Entity-Relationship Overview

```
organizations (1) ──── (*) users
    │
    ├── (1) ──── (*) projects
    │               │
    │               ├── (1) ──── (*) tasks
    │               │               │
    │               │               └── (*) comments
    │               │
    │               └── (*) ──── (*) project_members (via users)
    │
    └── users (owner_id → users.id)
```

### Tables

| Table | Key Columns | Relationships |
|-------|-------------|---------------|
| `organizations` | `id` (PK), `name` | 1 → users, projects |
| `users` | `id` (PK), `organization_id` (FK), `Email`, `role`, `PasswordHash` | org → users, projects (owner), tasks (assigned), comments (author), project_members |
| `projects` | `id` (PK), `organization_id` (FK), `owner_id` (FK), `name`, `status` | org → projects, owner → users |
| `project_members` | `project_id` (FK), `user_id` (FK) | project → members, user → memberships |
| `tasks` | `id` (PK), `project_id` (FK), `description`, `status`, `priority`, `assigned_to_id` (FK) | project → tasks, assigned → user |
| `comments` | `id` (PK), `task_id` (FK), `author_id` (FK), `content` | task → comments, author → user |

### Constraints

- `users.Email` — UNIQUE
- `users` — CHECK (`role` IN ('Admin', 'Manager', 'Member'))
- `project_members` — UNIQUE (`project_id`, `user_id`)
- `uq_users_organization_id_id` — UNIQUE (`organization_id`, `id`)
- `fk_users_organization` — ON DELETE CASCADE
- `fk_projects_owner` — ON DELETE RESTRICT
- `fk_tasks_assigned_user` — ON DELETE SET NULL
- `fk_project_members_user` — ON DELETE CASCADE
- `fk_comments_author` — ON DELETE RESTRICT

### Migrations

- `20260907224543_InitialCreate` — Initial schema with all tables, constraints, and indexes

---

## Known Issues

### 1. Flaky UI Test: `DeleteTask_VerifiesRemovedFromTable`

This test intermittently fails with a 30-second timeout when run as part of the full suite. The failure is caused by shared database state between tests — previous tests leave tasks in the database, so the delete test sees more rows than expected.

**Workaround:** Run the test in isolation:
```bash
dotnet test --filter "FullyQualifiedName~DeleteTask_VerifiesRemovedFromTable"
```

### 2. JSON Circular Reference Handling

EF Core 10.0.11 includes navigation properties that create circular references during JSON serialization (e.g., `Project.Organization`, `User.Organization`, `Project.Tasks`). This was resolved by adding `ReferenceHandler.IgnoreCycles` to the JSON serializer options in `Program.cs`.

### 3. NUnit Version Compatibility

The test project uses **NUnit 3.13.2** instead of 4.x because `Microsoft.Playwright.NUnit 1.52.0` targets NUnit 3. NUnit 4 removed `Assert.IsTrue`, `Assert.IsFalse`, and `StringAssert` which are used in the codebase.

### 4. Playwright Browser Installation

Playwright browsers are not installed by default. On first test run, Chromium will be downloaded automatically (~100MB). If installation fails, run:
```bash
node node_modules/.bin/playwright install chromium
```

---

## Contributing

1. Follow existing code conventions (C# naming, JS module pattern, HTML structure)
2. Ensure `dotnet build` passes before committing
3. Add Playwright tests for new UI flows in the appropriate `Pages/` and test class
4. Add Postman requests for new API endpoints in the collection
5. Update `MANUAL_TEST_CASES.md` for new user-facing features
6. Do not commit secrets or keys to the repository

---

## License

This project is a portfolio/educational artifact. No license has been specified.
