$(document).ready(function () {
  // If already logged in, redirect to projects page
  if (API.getToken()) {
    window.location.href = 'projects.html';
  }

  $('#register-form').on('submit', async function (e) {
    e.preventDefault();

    const name = $('#name').val().trim();
    const email = $('#email').val().trim();
    const password = $('#password').val();
    const organizationId = $('#organizationId').val().trim();
    const jobTitle = $('#jobTitle').val().trim() || null;

    if (!name || !email || !password || !organizationId) {
      API.showAlert('Please fill in all required fields.', 'warning');
      return;
    }

    const payload = {
      name,
      email,
      password,
      organizationId,
      jobTitle
    };

    $('#register-btn').prop('disabled', true);
    $('#btn-text').addClass('d-none');
    $('#btn-spinner').removeClass('d-none');

    try {
      const response = await API.post('/api/auth/register', payload);
      API.setAuth(response);
      API.showAlert('Registration successful! Redirecting to projects...', 'success');
      setTimeout(() => {
        window.location.href = 'projects.html';
      }, 1000);
    } catch (err) {
      // Error alert handled by API.post / API.request
    } finally {
      $('#register-btn').prop('disabled', false);
      $('#btn-text').removeClass('d-none');
      $('#btn-spinner').addClass('d-none');
    }
  });
});
