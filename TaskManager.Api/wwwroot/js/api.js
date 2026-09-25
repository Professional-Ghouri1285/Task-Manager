const API = {
  baseUrl: window.location.origin,

  getToken() {
    return localStorage.getItem('token');
  },

  getCurrentUser() {
    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
  },

  setAuth(authResponse) {
    localStorage.setItem('token', authResponse.token);
    localStorage.setItem('user', JSON.stringify({
      userId: authResponse.userId,
      name: authResponse.name,
      email: authResponse.email,
      role: authResponse.role,
      organizationId: authResponse.organizationId
    }));
  },

  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    window.location.href = 'login.html';
  },

  requireAuth() {
    const token = this.getToken();
    if (!token) {
      window.location.href = 'login.html';
      return false;
    }
    return true;
  },

  showAlert(message, type = 'danger', duration = 5000) {
    let container = document.getElementById('alert-container');
    if (!container) {
      container = document.createElement('div');
      container.id = 'alert-container';
      document.body.appendChild(container);
    }

    const alertEl = document.createElement('div');
    alertEl.className = `alert alert-${type} alert-dismissible fade show shadow-lg`;
    alertEl.role = 'alert';
    alertEl.innerHTML = `
      <div>${message}</div>
      <button type="button" class="btn-close btn-close-white" data-bs-dismiss="alert" aria-label="Close"></button>
    `;

    container.appendChild(alertEl);

    if (duration) {
      setTimeout(() => {
        if (alertEl.parentNode) {
          alertEl.classList.remove('show');
          setTimeout(() => alertEl.remove(), 150);
        }
      }, duration);
    }
  },

  async request(endpoint, options = {}) {
    const token = this.getToken();
    const headers = {
      'Content-Type': 'application/json',
      ...options.headers
    };

    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }

    const config = {
      ...options,
      headers
    };

    try {
      const response = await fetch(`${this.baseUrl}${endpoint}`, config);

      if (response.status === 401) {
        this.showAlert('Session expired or unauthenticated. Redirecting to login...', 'warning');
        setTimeout(() => {
          this.logout();
        }, 1500);
        throw new Error('Unauthorized');
      }

      if (response.status === 403) {
        const msg = 'Access Denied (403 Forbidden): You do not have the required role privileges for this action.';
        this.showAlert(msg, 'danger');
        throw new Error(msg);
      }

      if (!response.ok) {
        let errorMessage = `Request failed with status ${response.status}`;
        try {
          const errorData = await response.json();
          if (errorData && (errorData.error || errorData.message)) {
            errorMessage = errorData.error || errorData.message;
          } else if (typeof errorData === 'string') {
            errorMessage = errorData;
          }
        } catch (e) {
          // Response body wasn't JSON
        }
        throw new Error(errorMessage);
      }

      if (response.status === 204) {
        return null;
      }

      return await response.json();
    } catch (err) {
      if (err.message !== 'Unauthorized' && !err.message.includes('403 Forbidden')) {
        this.showAlert(err.message || 'An unexpected error occurred.', 'danger');
      }
      throw err;
    }
  },

  get(endpoint) {
    return this.request(endpoint, { method: 'GET' });
  },

  post(endpoint, body) {
    return this.request(endpoint, {
      method: 'POST',
      body: JSON.stringify(body)
    });
  },

  put(endpoint, body) {
    return this.request(endpoint, {
      method: 'PUT',
      body: JSON.stringify(body)
    });
  },

  delete(endpoint) {
    return this.request(endpoint, { method: 'DELETE' });
  }
};
