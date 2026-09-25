let currentUser = null;
let projectId = null;
let currentProject = null;
let currentMembers = [];
let currentTasks = [];

let taskToDeleteId = null;
let memberToRemoveId = null;

$(document).ready(function () {
  if (!API.requireAuth()) return;

  currentUser = API.getCurrentUser();
  if (currentUser) {
    $('#nav-user-name').text(currentUser.name || currentUser.email);
    $('#nav-user-role').text(currentUser.role || 'User');
    $('#user-info-badge').removeClass('d-none');
  }

  $('#logout-btn').on('click', function () {
    API.logout();
  });

  const urlParams = new URLSearchParams(window.location.search);
  projectId = urlParams.get('projectId');

  if (!projectId) {
    API.showAlert('No Project ID specified. Redirecting to projects list...', 'danger');
    setTimeout(() => { window.location.href = 'projects.html'; }, 2000);
    return;
  }

  loadProjectData();

  // Event Listeners
  $('#open-create-task-btn').on('click', function () {
    openCreateTaskModal();
  });

  $('#task-form').on('submit', async function (e) {
    e.preventDefault();
    await saveTask();
  });

  $('#confirm-reassign-btn').on('click', async function () {
    await executeReassign();
  });

  $('#add-member-form').on('submit', async function (e) {
    e.preventDefault();
    await addMember();
  });

  $('#confirm-remove-member-btn').on('click', async function () {
    await removeMember();
  });

  $('#change-owner-form').on('submit', async function (e) {
    e.preventDefault();
    await changeOwner();
  });

  $('#confirm-delete-task-btn').on('click', async function () {
    await deleteTask();
  });
});

async function loadProjectData() {
  try {
    const project = await API.get(`/api/organizations/${currentUser.organizationId}/projects/${projectId}`);
    currentProject = project;
    renderProjectHeader(project);

    await Promise.all([loadMembers(), loadTasks()]);
  } catch (err) {
    $('#project-header-name').text('Error loading project');
  }
}

function renderProjectHeader(project) {
  const statusBadges = {
    0: '<span class="badge badge-status badge-inprogress">Active</span>',
    1: '<span class="badge badge-status badge-completed">Completed</span>',
    2: '<span class="badge badge-status badge-cancelled">Archived</span>'
  };

  $('#project-header-name').text(project.name);
  $('#project-header-status').html(typeof project.status === 'number' ? statusBadges[project.status] || 'Active' : project.status);
  $('#project-header-desc').text(project.description || 'No description provided.');
  const ownerName = project.owner ? project.owner.name : project.ownerId;
  $('#project-header-owner').text(ownerName);
  $('#project-header-org').text(project.organizationId);
}

async function loadMembers() {
  try {
    const members = await API.get(`/api/organizations/${currentUser.organizationId}/projects/${projectId}/members`);
    currentMembers = members || [];
    $('#member-count').text(currentMembers.length);
    renderMembersGrid(currentMembers);
    populateMemberDropdowns(currentMembers);
  } catch (err) {
    $('#members-grid').html('<div class="col-12 text-center py-4 text-danger">Failed to load members.</div>');
  }
}

function renderMembersGrid(members) {
  if (members.length === 0) {
    $('#members-grid').html(`
      <div class="col-12 text-center py-5 text-muted">
        <i class="bi bi-people display-4"></i>
        <p class="mt-2">No members assigned to this project yet.</p>
      </div>
    `);
    return;
  }

  let html = '';
  members.forEach(m => {
    const user = m.user || { name: 'User', email: m.userId, role: 'Member' };
    const isOwner = currentProject && currentProject.ownerId === m.userId;

    html += `
      <div class="col-md-6 col-lg-4">
        <div class="card card-custom p-3 d-flex flex-row align-items-center justify-content-between">
          <div class="d-flex align-items-center gap-3">
            <div class="bg-dark p-2 rounded-circle border border-secondary text-info">
              <i class="bi bi-person-fill fs-4"></i>
            </div>
            <div>
              <div class="fw-bold text-light">${escapeHtml(user.name)} ${isOwner ? '<span class="badge bg-warning text-dark ms-1">Owner</span>' : ''}</div>
              <div class="text-muted small">${escapeHtml(user.email)}</div>
              <div class="text-muted extra-small" style="font-size:0.75rem;">ID: ${m.userId}</div>
            </div>
          </div>
          ${!isOwner ? `
            <button class="btn btn-outline-danger btn-sm remove-member-btn" data-userid="${m.userId}">
              <i class="bi bi-person-x"></i>
            </button>
          ` : ''}
        </div>
      </div>
    `;
  });

  $('#members-grid').html(html);

  $('.remove-member-btn').on('click', function () {
    memberToRemoveId = $(this).data('userid');
    const modal = new bootstrap.Modal(document.getElementById('removeMemberModal'));
    modal.show();
  });
}

function populateMemberDropdowns(members) {
  let assigneeOptions = '<option value="">-- Unassigned --</option>';
  let ownerOptions = '<option value="">-- Select Member --</option>';

  members.forEach(m => {
    const user = m.user || { name: `User (${m.userId.substring(0,8)}...)`, id: m.userId };
    assigneeOptions += `<option value="${m.userId}">${escapeHtml(user.name)} (${m.userId})</option>`;
    ownerOptions += `<option value="${m.userId}">${escapeHtml(user.name)} (${m.userId})</option>`;
  });

  $('#task-assignee-select').html(assigneeOptions);
  $('#reassign-member-select').html(assigneeOptions);
  $('#new-owner-select').html(ownerOptions);
}

async function loadTasks() {
  try {
    const tasks = await API.get(`/api/projects/${projectId}/tasks`);
    currentTasks = tasks || [];
    $('#task-count').text(currentTasks.length);
    renderTasksTable(currentTasks);
  } catch (err) {
    $('#tasks-table-body').html('<tr><td colspan="6" class="text-center py-4 text-danger">Failed to load tasks.</td></tr>');
  }
}

function renderTasksTable(tasks) {
  if (tasks.length === 0) {
    $('#tasks-table-body').html(`
      <tr>
        <td colspan="6" class="text-center py-5 text-muted">
          <i class="bi bi-inbox display-4 d-block mb-2"></i>
          No tasks found for this project. Click "Create Task" to add one.
        </td>
      </tr>
    `);
    return;
  }

  const priorityBadges = {
    0: '<span class="badge badge-medium">Medium</span>',
    1: '<span class="badge badge-low">Low</span>',
    2: '<span class="badge badge-high">High</span>',
    3: '<span class="badge badge-critical">Critical</span>'
  };

  let html = '';
  tasks.forEach(task => {
    const priorityHtml = typeof task.priority === 'number' ? priorityBadges[task.priority] || 'Medium' : task.priority;
    const assigneeName = task.assignedTo ? task.assignedTo.name : (task.assignedToId ? `ID: ${task.assignedToId.substring(0, 8)}...` : '<span class="text-muted fst-italic">Unassigned</span>');
    const dueDateStr = task.dueDate ? new Date(task.dueDate).toLocaleDateString() : '<span class="text-muted">None</span>';
    const taskJson = JSON.stringify(task).replace(/'/g, "&apos;");

    const taskStatusVal = task.status !== undefined ? task.status : 0;

    html += `
      <tr>
        <td class="ps-4 text-break" style="max-width: 300px;">
          <div class="fw-semibold text-light">${escapeHtml(task.description)}</div>
          <div class="text-muted extra-small" style="font-size:0.75rem;">Created: ${new Date(task.createdAt).toLocaleDateString()}</div>
        </td>
        <td>
          <select class="form-select form-select-sm form-select-custom task-status-select" style="width: 140px;" data-id="${task.id}">
            <option value="0" ${taskStatusVal === 0 ? 'selected' : ''}>Todo</option>
            <option value="1" ${taskStatusVal === 1 ? 'selected' : ''}>In Progress</option>
            <option value="2" ${taskStatusVal === 2 ? 'selected' : ''}>Completed</option>
            <option value="3" ${taskStatusVal === 3 ? 'selected' : ''}>Cancelled</option>
          </select>
        </td>
        <td>${priorityHtml}</td>
        <td>
          <div class="d-flex align-items-center gap-2">
            <span>${assigneeName}</span>
            <button class="btn btn-link btn-sm p-0 text-info reassign-task-btn" data-id="${task.id}" data-assigned="${task.assignedToId || ''}" title="Reassign">
              <i class="bi bi-person-gear"></i>
            </button>
          </div>
        </td>
        <td>${dueDateStr}</td>
        <td class="text-end pe-4">
          <div class="btn-group">
            <button class="btn btn-outline-custom btn-sm edit-task-btn" data-task='${taskJson}'>
              <i class="bi bi-pencil-square"></i>
            </button>
            <button class="btn btn-outline-danger btn-sm delete-task-btn" data-id="${task.id}">
              <i class="bi bi-trash"></i>
            </button>
          </div>
        </td>
      </tr>
    `;
  });

  $('#tasks-table-body').html(html);

  // Status select quick update event
  $('.task-status-select').on('change', async function () {
    const id = $(this).data('id');
    const newStatus = parseInt($(this).val());
    try {
      await API.put(`/api/projects/${projectId}/tasks/${id}/status`, newStatus);
      API.showAlert('Task status updated successfully.', 'success', 2000);
      loadTasks();
    } catch (err) {
      loadTasks();
    }
  });

  // Reassign button click
  $('.reassign-task-btn').on('click', function () {
    const id = $(this).data('id');
    const assignedId = $(this).data('assigned');
    $('#reassign-task-id').val(id);
    $('#reassign-member-select').val(assignedId || '');
    $('#reassign-manual-guid').val('');
    const modal = new bootstrap.Modal(document.getElementById('reassignTaskModal'));
    modal.show();
  });

  // Edit task button
  $('.edit-task-btn').on('click', function () {
    const t = $(this).data('task');
    openEditTaskModal(t);
  });

  // Delete task button
  $('.delete-task-btn').on('click', function () {
    taskToDeleteId = $(this).data('id');
    const modal = new bootstrap.Modal(document.getElementById('deleteTaskModal'));
    modal.show();
  });
}

function openCreateTaskModal() {
  $('#taskModalLabel').text('Create Task');
  $('#task-id').val('');
  $('#task-desc').val('');
  $('#task-status').val('0');
  $('#task-priority').val('0');
  $('#task-assignee-select').val('');
  $('#task-assignee-manual').val('');
  $('#task-duedate').val('');
}

function openEditTaskModal(task) {
  $('#taskModalLabel').text('Edit Task');
  $('#task-id').val(task.id);
  $('#task-desc').val(task.description);
  $('#task-status').val(task.status !== undefined ? task.status : 0);
  $('#task-priority').val(task.priority !== undefined ? task.priority : 0);

  if (task.assignedToId) {
    $('#task-assignee-select').val(task.assignedToId);
    if (!$('#task-assignee-select').val()) {
      $('#task-assignee-manual').val(task.assignedToId);
    }
  } else {
    $('#task-assignee-select').val('');
    $('#task-assignee-manual').val('');
  }

  if (task.dueDate) {
    $('#task-duedate').val(task.dueDate.split('T')[0]);
  } else {
    $('#task-duedate').val('');
  }

  const modal = new bootstrap.Modal(document.getElementById('taskModal'));
  modal.show();
}

async function saveTask() {
  const id = $('#task-id').val();
  const description = $('#task-desc').val().trim();
  const status = parseInt($('#task-status').val());
  const priority = parseInt($('#task-priority').val());
  const manualAssignee = $('#task-assignee-manual').val().trim();
  const selectAssignee = $('#task-assignee-select').val();
  const dueDateVal = $('#task-duedate').val();

  const assignedToId = manualAssignee || selectAssignee || null;
  const dueDate = dueDateVal ? new Date(dueDateVal).toISOString() : null;

  if (!description) {
    API.showAlert('Task description is required.', 'warning');
    return;
  }

  const payload = {
    id: id || undefined,
    projectId: projectId,
    description,
    status,
    priority,
    assignedToId,
    dueDate
  };

  try {
    if (id) {
      await API.put(`/api/projects/${projectId}/tasks/${id}`, payload);
      API.showAlert('Task updated successfully.', 'success');
    } else {
      await API.post(`/api/projects/${projectId}/tasks`, payload);
      API.showAlert('Task created successfully.', 'success');
    }

    const modalEl = document.getElementById('taskModal');
    const modal = bootstrap.Modal.getInstance(modalEl);
    if (modal) modal.hide();

    loadTasks();
  } catch (err) {
    // API handled
  }
}

async function executeReassign() {
  const taskId = $('#reassign-task-id').val();
  const manualGuid = $('#reassign-manual-guid').val().trim();
  const selectGuid = $('#reassign-member-select').val();
  const userId = manualGuid || selectGuid || null;

  try {
    await API.put(`/api/projects/${projectId}/tasks/${taskId}/assign`, userId ? userId : null);
    API.showAlert('Task assignment updated.', 'success');
    const modalEl = document.getElementById('reassignTaskModal');
    const modal = bootstrap.Modal.getInstance(modalEl);
    if (modal) modal.hide();
    loadTasks();
  } catch (err) {
    // API handled
  }
}

async function addMember() {
  const userId = $('#new-member-userid').val().trim();
  if (!userId) {
    API.showAlert('Please enter a User GUID.', 'warning');
    return;
  }

  try {
    await API.post(`/api/organizations/${currentUser.organizationId}/projects/${projectId}/members`, userId);
    API.showAlert('Member added to project.', 'success');
    const modalEl = document.getElementById('addMemberModal');
    const modal = bootstrap.Modal.getInstance(modalEl);
    if (modal) modal.hide();
    $('#new-member-userid').val('');
    loadMembers();
  } catch (err) {
    // API handled
  }
}

async function removeMember() {
  if (!memberToRemoveId) return;

  try {
    await API.delete(`/api/organizations/${currentUser.organizationId}/projects/${projectId}/members/${memberToRemoveId}`);
    API.showAlert('Member removed from project.', 'success');
    const modalEl = document.getElementById('removeMemberModal');
    const modal = bootstrap.Modal.getInstance(modalEl);
    if (modal) modal.hide();
    loadMembers();
  } catch (err) {
    // API handled
  }
}

async function changeOwner() {
  const manualOwner = $('#new-owner-userid').val().trim();
  const selectOwner = $('#new-owner-select').val();
  const newOwnerId = manualOwner || selectOwner;

  if (!newOwnerId) {
    API.showAlert('Please select or enter a new owner User GUID.', 'warning');
    return;
  }

  try {
    await API.put(`/api/organizations/${currentUser.organizationId}/projects/${projectId}/owner`, newOwnerId);
    API.showAlert('Project ownership transferred successfully.', 'success');
    const modalEl = document.getElementById('changeOwnerModal');
    const modal = bootstrap.Modal.getInstance(modalEl);
    if (modal) modal.hide();
    loadProjectData();
  } catch (err) {
    // API handled
  }
}

async function deleteTask() {
  if (!taskToDeleteId) return;

  try {
    await API.delete(`/api/projects/${projectId}/tasks/${taskToDeleteId}`);
    API.showAlert('Task deleted successfully.', 'success');
    const modalEl = document.getElementById('deleteTaskModal');
    const modal = bootstrap.Modal.getInstance(modalEl);
    if (modal) modal.hide();
    loadTasks();
  } catch (err) {
    // API handled
  }
}

function escapeHtml(str) {
  if (!str) return '';
  return str.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/"/g, "&quot;").replace(/'/g, "&#039;");
}
