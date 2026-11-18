import { request } from './client';

export function getBranchesByFranchise(franchiseId) {
  return request(`/franchises/${franchiseId}/branches`);
}

export function getBranch(id) {
  return request(`/branches/${id}`);
}

export function createBranch(franchiseId, name) {
  return request(`/franchises/${franchiseId}/branches`, {
    method: 'POST',
    body: JSON.stringify({ name }),
  });
}