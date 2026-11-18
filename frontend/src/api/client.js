const API_URL = import.meta.env.VITE_API_URL;

export async function request(path, options = {}) {
  let response;

  try {
    response = await fetch(`${API_URL}${path}`, {
      headers: { 'Content-Type': 'application/json' },
      ...options,
    });
  } catch {
    throw new Error('Could not connect to the server. Make sure the API is running.');
  }

  if (!response.ok) {
    throw new Error(await getErrorMessage(response));
  }

  return response.json();
}

async function getErrorMessage(response) {
  try {
    const problem = await response.json();

    if (problem.errors) {
      return Object.values(problem.errors).flat().join(' ');
    }

    return problem.detail || problem.title || `Request failed with status ${response.status}.`;
  } catch {
    return `Request failed with status ${response.status}.`;
  }
}