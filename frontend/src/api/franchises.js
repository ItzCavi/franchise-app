import { request } from './client';

export function getFranchises() {
  return request('/franchises');
}

export function getFranchise(id) {
  return request(`/franchises/${id}`);
}

export function createFranchise(name) {
  return request('/franchises', {
    method: 'POST',
    body: JSON.stringify({ name }),
  });
}
export function getFranchiseReport(id) {
  return request(`/franchises/${id}/report`);
}