import { request } from './client';

export function getProductsByBranch(branchId) {
  return request(`/branches/${branchId}/products`);
}

export function createProduct(branchId, name, stock) {
  return request(`/branches/${branchId}/products`, {
    method: 'POST',
    body: JSON.stringify({ name, stock }),
  });
}

export function updateProductStock(productId, stock) {
  return request(`/products/${productId}/stock`, {
    method: 'PATCH',
    body: JSON.stringify({ stock }),
  });
}