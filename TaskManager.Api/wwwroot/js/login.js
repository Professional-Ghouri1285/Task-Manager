$(document).ready(function () {
  // If already logged in, redirect to projects page
  if (API.getToken()) {
    window.location.href = 'projects.html';
  }

  $('#login-form').on('submit', async function (e) {
    e.preventDefault();

    const email = $('#email').val().trim();
    const password = $('#password').val();

    if (!email || !password) {
      API.showAlert('Please fill in both email and password.', 'warning');
      return;
    }

    const payload = { email, password };

    $('#login-btn').prop('disabled', true);
    $('#btn-text').addClass('d-none');
    $('#btn-spinner').removeClass('d-none');

    try {
      const response = await API.post('/api/auth/login', payload);
      API.setAuth(response);
      API.showAlert('Login successful! Redirecting to projects...', 'success');
      setTimeout(() => {
        window.location.href = 'projects.html';
      }, 800);
    } catch (err) {
      // Error alert handled by API.post / API.request
    } finally {
      $('#login-btn').prop('disabled', false);
      $('#btn-text').removeClass('d-none');
      $('#btn-spinner').addClass('d-none');
    }
  });
});
