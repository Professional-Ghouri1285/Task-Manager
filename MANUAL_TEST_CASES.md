# TaskManager — Manual Test Case Document

## Purpose
This document provides a structured set of manual test cases for the TaskManager API and web UI, suitable for portfolio review, QA onboarding, or regression verification. All cases are written from the perspective of a tester executing against a deployed instance (e.g., `http://localhost:5059`).

## Test Environment
- **Application URL:** `http://localhost:5059`
- **Database:** PostgreSQL (`task_manager`)
- **Known seeded users:**
  - Admin: `admin@test.local` / `AdminPass123!`
  - Manager: `manager@test.local` / `ManagerPass123!`
  - Member: `member@test.local` / `MemberPass123!`
- **Organization:** `Acme Corporation` (`11111111-1111-1111-1111-111111111111`)

---

## Test Cases

### Authentication

| TC-AUTH-01 | Successful Admin Login |
|---|---|
| **Preconditions** | User `admin@test.local` exists with password `AdminPass123!`. |
| **Steps** | 1. Navigate to `/login.html`<br>2. Enter email `admin@test.local`<br>3. Enter password `AdminPass123!`<br>4. Click **Sign In** |
| **Expected Result** | User is redirected to `/projects.html`. A JWT is stored in `localStorage.token`. The projects grid loads without errors. |

| TC-AUTH-02 | Successful Manager Login |
|---|---|
| **Preconditions** | User `manager@test.local` exists with password `ManagerPass123!`. |
| **Steps** | 1. Navigate to `/login.html`<br>2. Enter email `manager@test.local`<br>3. Enter password `ManagerPass123!`<br>4. Click **Sign In** |
| **Expected Result** | User is redirected to `/projects.html`. Manager projects are visible. |

| TC-AUTH-03 | Successful Member Login |
|---|---|
| **Preconditions** | User `member@test.local` exists with password `MemberPass123!`. |
| **Steps** | 1. Navigate to `/login.html`<br>2. Enter email `member@test.local`<br>3. Enter password `MemberPass123!`<br>4. Click **Sign In** |
| **Expected Result** | User is redirected to `/projects.html`. Member projects are visible. |

| TC-AUTH-04 | Invalid Email — Login Fails |
|---|---|
| **Preconditions** | None. |
| **Steps** | 1. Navigate to `/login.html`<br>2. Enter email `nonexistent@test.local`<br>3. Enter password `AnyPass123!`<br>4. Click **Sign In** |
| **Expected Result** | Login fails. An error alert is displayed: *"Invalid credentials"*. User remains on `/login.html`. No JWT is stored. |

| TC-AUTH-05 | Wrong Password — Login Fails |
|---|---|
| **Preconditions** | User `admin@test.local` exists. |
| **Steps** | 1. Navigate to `/login.html`<br>2. Enter email `admin@test.local`<br>3. Enter password `WrongPassword!`<br>4. Click **Sign In** |
| **Expected Result** | Login fails. An error alert is displayed: *"Invalid credentials"*. User remains on `/login.html`. |

| TC-AUTH-06 | Already Authenticated — Redirect Bypasses Login |
|---|---|
| **Preconditions** | User has a valid JWT in `localStorage.token`. |
| **Steps** | 1. Navigate to `/` or `/login.html` |
| **Expected Result** | User is immediately redirected to `/projects.html` without seeing the login form. |

| TC-AUTH-07 | Session Expiry — JWT Rejected |
|---|---|
| **Preconditions** | User has a JWT with an expired `exp` claim in `localStorage`. |
| **Steps** | 1. Navigate to `/projects.html` |
| **Expected Result** | User is redirected to `/login.html`. The expired token is cleared from `localStorage`. An error message may be shown. |

---

### Authorization & Role Permissions

| TC-ROLE-01 | Admin Can Create Project |
|---|---|
| **Preconditions** | Admin is logged in. |
| **Steps** | 1. Navigate to `/projects.html`<br>2. Click **New Project**<br>3. Enter name *"Admin Test Project"*<br>4. Click **Create** |
| **Expected Result** | Project is created (201). The new project appears in the projects grid. |

| TC-ROLE-02 | Manager Can Create Project |
|---|---|
| **Preconditions** | Manager is logged in. |
| **Steps** | 1. Navigate to `/projects.html`<br>2. Click **New Project**<br>3. Enter name *"Manager Test Project"*<br>4. Click **Create** |
| **Expected Result** | Project is created (201). The new project appears in the projects grid. |

| TC-ROLE-03 | Member Cannot Create Project (403 Forbidden) |
|---|---|
| **Preconditions** | Member is logged in. |
| **Steps** | 1. Navigate to `/projects.html`<br>2. Click **New Project**<br>3. Enter name *"Member Test Project"*<br>4. Click **Create** |
| **Expected Result** | Request is rejected with 403 Forbidden. An error toast/message is displayed: *"You are not authorized to perform this action."* No project is created. |

| TC-ROLE-04 | Member Can View Projects But Not Create |
|---|---|
| **Preconditions** | Member is logged in. At least one project exists in the organization. |
| **Steps** | 1. Navigate to `/projects.html`<br>2. Observe UI |
| **Expected Result** | Projects list is visible. The **New Project** button is absent or disabled. |

| TC-ROLE-05 | Unauthenticated User Redirected to Login |
|---|---|
| **Preconditions** | User is not logged in (no token in `localStorage`). |
| **Steps** | 1. Open browser DevTools > Application > Local Storage<br>2. Clear all entries<br>3. Navigate to `/projects.html` |
| **Expected Result** | User is redirected to `/login.html`. The projects page content is never rendered. |

| TC-ROLE-06 | Member Cannot Access Admin-Only Endpoints Directly |
|---|---|
| **Preconditions** | Member is logged in. Member has a valid JWT. |
| **Steps** | 1. Open DevTools > Network<br>2. Trigger any Admin-only action (e.g., try to create a project via API using member token)<br>3. Observe response |
| **Expected Result** | API returns 403 Forbidden. The UI does not expose a path to trigger this for members. |

---

### Project CRUD

| TC-PROJ-01 | Create Project — Success |
|---|---|
| **Preconditions** | Admin or Manager is logged in. |
| **Steps** | 1. Navigate to `/projects.html`<br>2. Click **New Project**<br>3. Enter name *"Test Project Alpha"*<br>4. Enter description *"Manual test project"*<br>5. Select status **Active**<br>6. Click **Create** |
| **Expected Result** | Project is created with 201 response. Project appears in the grid with correct name, description, status, and owner. |

| TC-PROJ-02 | View Project Details — Correct Data Loaded |
|---|---|
| **Preconditions** | Project *"Test Project Alpha"* exists. |
| **Steps** | 1. Navigate to `/projects.html`<br>2. Click on *"Test Project Alpha"* card |
| **Expected Result** | User is navigated to `/project-details.html?id=<projectId>`. Project name and description are displayed correctly. Task list is visible. |

| TC-PROJ-03 | Update Project — Fields Persisted |
|---|---|
| **Preconditions** | Project *"Test Project Alpha"* exists. Admin is logged in. |
| **Steps** | 1. Open project details<br>2. Click **Edit**<br>3. Change name to *"Test Project Alpha Updated"*<br>4. Change status to **Archived**<br>5. Click **Save** |
| **Expected Result** | Project name and status are updated. Grid card and detail view reflect new values immediately. |

| TC-PROJ-04 | Delete Project — Removed From List |
|---|---|
| **Preconditions** | Project *"Test Project Alpha Updated"* exists. Admin is logged in. |
| **Steps** | 1. Open project details<br>2. Click **Delete**<br>3. Confirm deletion |
| **Expected Result** | Project is deleted (204). User is redirected to `/projects.html`. The project no longer appears in the grid. |

| TC-PROJ-05 | Non-Existent Project — 404 |
|---|---|
| **Preconditions** | None. |
| **Steps** | 1. Navigate to `/projects.html#` or attempt to access `/project-details.html?id=00000000-0000-0000-0000-000000000000` |
| **Expected Result** | UI shows a "not found" state or redirects to projects list. API returns 404. |

---

### Task CRUD

| TC-TASK-01 | Create Task — Appears in Table |
|---|---|
| **Preconditions** | Project *"Test Project Alpha Updated"* exists. Admin is logged in and viewing project details. |
| **Steps** | 1. Navigate to project details<br>2. Click **New Task**<br>3. Enter description *"Manual test task"*<br>4. Set priority to **Medium**<br>5. Click **Create** |
| **Expected Result** | Task is created (201). Task row appears in the task table with correct description and priority badge. |

| TC-TASK-02 | Update Task — Description Changed |
|---|---|
| **Preconditions** | Task *"Manual test task"* exists. |
| **Steps** | 1. In project details, click the **Edit** icon on the task row<br>2. Change description to *"Manual test task updated"*<br>3. Click **Save** |
| **Expected Result** | Task description is updated in the table immediately. |

| TC-TASK-03 | Change Task Status — Reflected in UI |
|---|---|
| **Preconditions** | Task exists with status **Not Started**. |
| **Steps** | 1. In project details, locate the status dropdown for the task<br>2. Select **In Progress**<br>3. Observe the table |
| **Expected Result** | Status badge updates to **In Progress** without a page reload. |

| TC-TASK-04 | Delete Task — Removed From Table |
|---|---|
| **Preconditions** | Task *"Manual test task updated"* exists. |
| **Steps** | 1. In project details, click the **Delete** icon on the task row<br>2. Confirm deletion |
| **Expected Result** | Task row disappears from the table. API returns 204. Subsequent reload does not show the task. |

| TC-TASK-05 | Task Lifecycle — Create → Edit → Status Change → Delete |
|---|---|
| **Preconditions** | Admin is logged in. A project exists. |
| **Steps** | 1. Create a task with description *"Lifecycle Task"* and priority **High**<br>2. Edit description to *"Lifecycle Task v2"*<br>3. Change status to **Done**<br>4. Delete the task |
| **Expected Result** | All four operations succeed in sequence. Final state: task does not exist in the table or API. |

| TC-TASK-06 | Assign Task — Assignee Visible |
|---|---|
| **Preconditions** | Task exists. Manager user exists. |
| **Steps** | 1. Open task edit dialog<br>2. Select assignee *"Manager User"*<br>3. Save |
| **Expected Result** | Task row shows assignee name. API returns 200 with `assignedToId` set. |

---

### Membership

| TC-MEMB-01 | Add Member — Appears in Member List |
|---|---|
| **Preconditions** | Project exists. Admin is logged in. Manager user exists and is in same organization. |
| **Steps** | 1. Open project details<br>2. Click **Manage Members**<br>3. Select user *"Manager User"*<br>4. Click **Add Member** |
| **Expected Result** | Manager user appears in the member list. API returns 204. |

| TC-MEMB-02 | Remove Member — Removed From List |
|---|---|
| **Preconditions** | Manager user is a member of the project. |
| **Steps** | 1. Open **Manage Members**<br>2. Click **Remove** next to *"Manager User"*<br>3. Confirm |
| **Expected Result** | Manager user is removed from the member list. API returns 204. |

| TC-MEMB-03 | Change Project Owner — Ownership Transferred |
|---|---|
| **Preconditions** | Project exists. Admin and Manager users exist. |
| **Steps** | 1. Open **Project Settings** or admin API<br>2. Change owner to *"Manager User"*<br>3. Save |
| **Expected Result** | Project owner is updated to Manager. Manager can now see the project as their own. |

---

### Error Handling & Edge Cases

| TC-ERR-01 | Login With Empty Fields — Validation Error |
|---|---|
| **Preconditions** | None. |
| **Steps** | 1. Navigate to `/login.html`<br>2. Leave email and password blank<br>3. Click **Sign In** |
| **Expected Result** | HTML5 validation prevents submission, or API returns 400 with validation errors. User stays on login page. |

| TC-ERR-02 | Duplicate Project Name — Allowed |
|---|---|
| **Preconditions** | Project *"Duplicate Test"* exists. |
| **Steps** | 1. Create a new project with the same name *"Duplicate Test"* |
| **Expected Result** | Project is created (duplicate names are allowed by the current schema). Both projects appear in the list. |

| TC-ERR-03 | Task With Empty Description — Validation Error |
|---|---|
| **Preconditions** | Project exists. Admin is logged in. |
| **Steps** | 1. Open new task dialog<br>2. Leave description blank<br>3. Set priority<br>4. Click **Create** |
| **Expected Result** | API returns 400 Bad Request. Task is not created. UI shows a validation message. |

| TC-ERR-04 | Very Long Description — Truncated or Accepted |
|---|---|
| **Preconditions** | Project exists. |
| **Steps** | 1. Create a task with a 1000-character description |
| **Expected Result** | Task is created. Description is stored and displayed (may be truncated in the UI table but fully visible in the detail view). |

---

### Redirect Behavior

| TC-REDIR-01 | Root Path Redirects to Login When Not Authenticated |
|---|---|
| **Preconditions** | User has no valid token. |
| **Steps** | 1. Navigate to `http://localhost:5059/` |
| **Expected Result** | User is redirected to `/login.html`. |

| TC-REDIR-02 | Root Path Redirects to Projects When Authenticated |
|---|---|
| **Preconditions** | Admin token is valid. |
| **Steps** | 1. Set `localStorage.token` to a valid admin JWT<br>2. Navigate to `http://localhost:5059/` |
| **Expected Result** | User is redirected to `/projects.html`. |

| TC-REDIR-03 | Login Redirects Back to Projects After Auth |
|---|---|
| **Preconditions** | User attempted to access `/projects.html` while unauthenticated. |
| **Steps** | 1. Navigate to `/projects.html` while logged out → should redirect to `/login.html`<br>2. Complete login with valid credentials |
| **Expected Result** | After successful login, user is redirected to `/projects.html` (not back to `/login.html`). |

---

## Summary

| Category | Test Case Count |
|---|---|
| Authentication | 7 |
| Authorization & Role Permissions | 6 |
| Project CRUD | 5 |
| Task CRUD | 6 |
| Membership | 3 |
| Error Handling & Edge Cases | 4 |
| Redirect Behavior | 3 |
| **Total** | **34** |

### Recommended Execution Order
1. Execute TC-AUTH-01 through TC-AUTH-03 to establish valid sessions.
2. Execute TC-ROLE-01 through TC-ROLE-06 to verify role-based access control.
3. Execute TC-PROJ-01 through TC-PROJ-04 for project lifecycle.
4. Execute TC-TASK-01 through TC-TASK-06 for task lifecycle.
5. Execute TC-MEMB-01 through TC-MEMB-03 for membership operations.
6. Execute TC-ERR-01 through TC-ERR-04 for negative-path validation.
7. Execute TC-REDIR-01 through TC-REDIR-03 for routing behavior.

### Notes for Reviewers
- **Flaky test known issue:** `FullLifecycle_CreateEditStatusDelete` in the automated suite is sensitive to shared database state. When running manually, ensure each test starts from a clean project or uses unique project names.
- **Database-only checks:** Refer to `DATABASE_VERIFICATION.md` for 7 checks that require direct SQL (cascade delete, restrict, set-null, unique constraints, check constraints, default values).
- **Known API bugs fixed during test authoring:**
  - `ProjectsController.AddMember` and `ChangeOwner` were missing `[FromBody]` on GUID parameters, causing 400 errors. Fixed.
  - `Program.cs` had no JSON reference-handling configuration, causing circular-reference serialization failures. Fixed with `ReferenceHandler.IgnoreCycles`.
