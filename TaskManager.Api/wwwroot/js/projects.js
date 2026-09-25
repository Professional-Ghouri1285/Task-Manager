let currentUser = null;
let projectToDeleteId = null;

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

  loadProjects();

  $('#open-create-modal-btn').on('click', function () {
    $('#projectModalLabel').text('Create Project');
    $('#project-id').val('');
    $('#project-name').val('');
    $('#project-desc').val('');
    $('#project-status').val('0');
    if (currentUser) {
      $('#project-owner-id').val(currentUser.userId);
    }
  });

  $('#project-form').on('submit', async function (e) {
    e.preventDefault();
    await saveProject();
  });

  $('#confirm-delete-project-btn').on('click', async function () {
    if (!projectToDeleteId) return;
    try {
      await API.delete(`/api/organizations/${currentUser.organizationId}/projects/${projectToDeleteId}`);
      API.showAlert('Project deleted successfully.', 'success');
      const modal = bootstrap.Modal.getInstance(document.getElementById('deleteProjectModal'));
      if (modal) modal.hide();
      loadProjects();
    } catch (err) {
      // Handled in API wrapper
    }
  });
});

async function loadProjects() {
  $('#projects-loading').removeClass('d-none');
  $('#projects-grid').empty();
  $('#projects-empty').addClass('d-none');

  try {
    const projects = await API.get(`/api/organizations/${currentUser.organizationId}/projects`);
    $('#projects-loading').addClass('d-none');

    if (!projects || projects.length === 0) {
      $('#projects-empty').removeClass('d-none');
      return;
    }

    projects.forEach(project => {
      const cardHtml = renderProjectCard(project);
      $('#projects-grid').append(cardHtml);
    });

    // Attach card event handlers
    $('.edit-project-btn').on('click', function () {
      const p = $(this).data('project');
      openEditModal(p);
    });

    $('.delete-project-btn').on('click', function () {
      projectToDeleteId = $(this).data('id');
      const deleteModal = new bootstrap.Modal(document.getElementById('deleteProjectModal'));
      deleteModal.show();
    });

  } catch (err) {
    $('#projects-loading').addClass('d-none');
  }
}

function renderProjectCard(project) {
  const statusBadges = {
    0: '<span class="badge badge-status badge-inprogress">Active</span>',
    1: '<span class="badge badge-status badge-completed">Completed</span>',
    2: '<span class="badge badge-status badge-cancelled">Archived</span>'
  };

  const statusText = typeof project.status === 'number' ? statusBadges[project.status] || 'Active' : project.status;
  const ownerName = project.owner ? project.owner.name : (project.ownerId ? `User: ${project.ownerId.substring(0, 8)}...` : 'Unassigned');
  const memberCount = project.members ? project.members.length : 0;
  const projectJson = JSON.stringify(project).replace(/'/g, "&apos;");

  return `
    <div class="col-md-6 col-lg-4">
      <div class="card card-custom h-100 p-4 d-flex flex-column justify-content-between">
        <div>
          <div class="d-flex justify-content-between align-items-start mb-2">
            <h5 class="fw-bold mb-0 text-truncate me-2 text-light">${escapeHtml(project.name)}</h5>
            ${statusText}
          </div>
          <p class="text-muted small mb-4 text-break" style="min-height: 40px;">
            ${escapeHtml(project.description || 'No description provided.')}
          </p>
          <div class="d-flex align-items-center gap-3 text-muted small mb-3">
            <div><i class="bi bi-person-fill text-info me-1"></i> Owner: <strong class="text-light">${escapeHtml(ownerName)}</strong></div>
            <div><i class="bi bi-people-fill text-info me-1"></i> Members: <strong class="text-light">${memberCount}</strong></div>
          </div>
        </div>

        <div class="pt-3 border-top border-secondary d-flex align-items-center justify-content-between">
          <a href="project-details.html?projectId=${project.id}" class="btn btn-outline-custom btn-sm">
            <i class="bi bi-kanban me-1"></i> View Tasks & Members
          </a>
          <div class="btn-group">
            <button class="btn btn-outline-custom btn-sm edit-project-btn" data-project='${projectJson}'>
              <i class="bi bi-pencil-square"></i>
            </button>
            <button class="btn btn-outline-danger btn-sm delete-project-btn" data-id="${project.id}">
              <i class="bi bi-trash"></i>
            </button>
          </div>
        </div>
      </div>
    </div>
  `;
}

function openEditModal(project) {
  $('#projectModalLabel').text('Edit Project');
  $('#project-id').val(project.id);
  $('#project-name').val(project.name);
  $('#project-desc').val(project.description || '');
  $('#project-status').val(project.status !== undefined ? project.status : 0);
  $('#project-owner-id').val(project.ownerId);

  const modal = new bootstrap.Modal(document.getElementById('projectModal'));
  modal.show();
}

async function saveProject() {
  const id = $('#project-id').val();
  const name = $('#project-name').val().trim();
  const description = $('#project-desc').val().trim();
  const status = parseInt($('#project-status').val());
  const ownerId = $('#project-owner-id').val().trim();

  if (!name || !ownerId) {
    API.showAlert('Name and Owner ID are required.', 'warning');
    return;
  }

  const payload = {
    id: id || undefined,
    organizationId: currentUser.organizationId,
    ownerId,
    name,
    description,
    status
  };

  try {
    if (id) {
      await API.put(`/api/organizations/${currentUser.organizationId}/projects/${id}`, payload);
      API.showAlert('Project updated successfully.', 'success');
    } else {
      await API.post(`/api/organizations/${currentUser.organizationId}/projects`, payload);
      API.showAlert('Project created successfully.', 'success');
    }

    const modalEl = document.getElementById('projectModal');
    const modal = bootstrap.Modal.getInstance(modalEl);
    if (modal) modal.hide();

    loadProjects();
  } catch (err) {
    // Handled in API wrapper
  }
}

function escapeHtml(str) {
  if (!str) return '';
  return str.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/"/g, "&quot;").replace(/'/g, "&#039;");
}
