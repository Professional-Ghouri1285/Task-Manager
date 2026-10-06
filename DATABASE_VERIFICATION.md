# TaskManager API — Database Verification Guide

## Approach

All verification below is performed **indirectly through API responses**. No direct PostgreSQL queries are used in automation; direct-SQL-only checks are flagged explicitly.

---

## 1. Entity Persistence (Create / Read / Update)

| Check | How Verified | Direct SQL Required? |
|-------|-------------|----------------------|
| Create Project persists all fields | `POST /api/organizations/{orgId}/projects` then `GET /api/organizations/{orgId}/projects/{id}` | **No** — response body confirms DB state |
| Create Task persists all fields | `POST /api/projects/{projectId}/tasks` then `GET /api/projects/{projectId}/tasks/{id}` | **No** |
| Update Project name/status/description | `PUT /api/organizations/{orgId}/projects/{id}` then `GET` same | **No** |
| Update Task description/status/priority | `PUT /api/projects/{projectId}/tasks/{id}` then `GET` same | **No** |

---

## 2. Membership Lifecycle

| Check | How Verified | Direct SQL Required? |
|-------|-------------|----------------------|
| Add member succeeds (204) | `POST /api/organizations/{orgId}/projects/{projectId}/members` with body `"{userId}"` | **No** |
| Member appears in list | `GET /api/organizations/{orgId}/projects/{projectId}/members` → array includes `userId` | **No** |
| Remove member succeeds (204) | `DELETE .../members/{userId}` | **No** |
| Member absent after removal | `GET .../members` → array does NOT include `userId` | **No** |
| Duplicate add is idempotent (204) | `POST .../members` twice with same `userId` | **No** |
| Remove non-existent member (404) | `DELETE .../members/{nonExistentGuid}` | **No** |

---

## 3. Task Lifecycle

| Check | How Verified | Direct SQL Required? |
|-------|-------------|----------------------|
| Task list contains created task | `GET /api/projects/{projectId}/tasks` → array includes `taskId` | **No** |
| Deleted task absent from list | `DELETE /api/projects/{projectId}/tasks/{id}` then `GET .../tasks` → `taskId` not in array | **No** |
| Task status change reflected | `PUT .../tasks/{id}/status` with raw enum value, then `GET` same | **No** |
| Task assignee change reflected | `PUT .../tasks/{id}/assign` with raw `userId` string, then `GET` same | **No** |

---

## 4. Authorization Edge Cases (Role-Based Access)

| Check | How Verified | Direct SQL Required? |
|-------|-------------|----------------------|
| Unauthenticated create project → 401 | `POST .../projects` without `Authorization` header | **No** |
| Member role create project → 403 | `POST .../projects` with `memberToken` | **No** |
| Non-existent project → 404 | `GET /api/organizations/{orgId}/projects/{allZerosGuid}` | **No** |
| Wrong organization → 404 | `DELETE /api/organizations/{allZerosOrg}/projects/{id}` | **No** |
| Task in wrong project → 404 | `PUT /api/projects/{allZerosOrg}/tasks/{taskId}` | **No** |

---

## 5. Checks That GENUINELY Require Direct SQL

These verify **physical schema enforcement** and **referential integrity at the storage layer**, which cannot be proven through API responses alone.

### 5.1 FK Constraint: `ON DELETE CASCADE` for Project → Tasks
- **What to verify**: When a project is deleted, its tasks are physically removed from the `tasks` table.
- **Why API is insufficient**: `DELETE /projects/{id}` returns 204. A subsequent `GET /projects/{id}/tasks` might return 404 because the route handler rejects missing projects, not because the FK cascade fired.
- **Required direct SQL**:
  ```sql
  -- After deleting a project
  SELECT count(*) FROM tasks WHERE project_id = '<deleted-project-id>';
  -- Expected: 0 (proves cascade fired)
  ```

### 5.2 FK Constraint: `ON DELETE SET NULL` for Task → Assigned User
- **What to verify**: When a user who is assigned to a task is deleted, `assigned_to_id` becomes NULL.
- **Why API is insufficient**: Requires a `DELETE /users/{id}` endpoint that the API does not expose.
- **Required direct SQL**:
  ```sql
  -- After deleting user
  SELECT assigned_to_id FROM tasks WHERE id = '<task-id>';
  -- Expected: NULL
  ```

### 5.3 FK Constraint: `ON DELETE RESTRICT` for Project Owner
- **What to verify**: Deleting a user who owns a project is rejected by the database.
- **Why API is insufficient**: No user-delete endpoint exists; even if added, the API might translate the DB error into 400/500, but the **restrict behavior itself** is only observable in the DB error code.
- **Required direct SQL**:
  ```sql
  DELETE FROM users WHERE id = '<owner-user-id>';
  -- Expected: ERROR: update or delete on table "users" violates foreign key constraint "fk_projects_owner" on table "projects". Detail: Key (id)=(...) is still referenced from table "projects".
  ```

### 5.4 FK Constraint: `ON DELETE CASCADE` for Project → Members
- **What to verify**: Deleting a project removes its rows from `project_members`.
- **Required direct SQL**:
  ```sql
  SELECT count(*) FROM project_members WHERE project_id = '<deleted-project-id>';
  -- Expected: 0
  ```

### 5.5 Unique Constraint: `uq_users_organization_id_id`
- **What to verify**: The composite unique constraint on `(organization_id, id)` at the DB level.
- **Why API is insufficient**: The API prevents duplicate creation at the service layer. To prove the **DB constraint** actually exists, you need to attempt a raw SQL insert that bypasses the service.
- **Required direct SQL**:
  ```sql
  INSERT INTO users (id, organization_id, name, email, role, passwordhash)
  VALUES ('11111111-1111-1111-1111-111111111101', '11111111-1111-1111-1111-111111111111', 'X', 'x@test.local', 'Member', 'x');
  -- Expected: ERROR: duplicate key value violates unique constraint "uq_users_organization_id_id"
  ```

### 5.6 Check Constraint: `chk_users_role`
- **What to verify**: Only `Admin`, `Manager`, `Member` values are allowed.
- **Required direct SQL**:
  ```sql
  UPDATE users SET role = 'SuperAdmin' WHERE id = '<user-id>';
  -- Expected: ERROR: new row for relation "users" violates check constraint "chk_users_role"
  ```

### 5.7 Default Values & Column Nullability
- **What to verify**: `created_at` / `updated_at` default to `utc_now`, `status` defaults to `Active`, etc.
- **Required direct SQL**:
  ```sql
  SELECT created_at, updated_at, status FROM projects WHERE id = '<new-project>';
  -- Verify timestamps are within a few seconds of now, status = 0 (Active)
  ```

---

## 6. Summary

- **68 API assertions** in the Postman/Newman collection cover all success and failure paths.
- **7 checks** above are flagged as requiring **direct SQL** to verify physical schema enforcement (cascade, restrict, set-null, unique constraints, check constraints, defaults).
