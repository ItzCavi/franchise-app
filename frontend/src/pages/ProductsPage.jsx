import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { getBranch } from '../api/branches';
import { createProduct, getProductsByBranch } from '../api/products';
import ProductRow from '../components/ProductRow';

export default function ProductsPage() {
  const { branchId } = useParams();

  const [branch, setBranch] = useState(null);
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const [name, setName] = useState('');
  const [stock, setStock] = useState('0');
  const [saving, setSaving] = useState(false);
  const [formError, setFormError] = useState('');

  useEffect(() => {
    async function loadData() {
      try {
        const [branchData, productsData] = await Promise.all([
          getBranch(branchId),
          getProductsByBranch(branchId),
        ]);
        setBranch(branchData);
        setProducts(productsData);
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    }

    loadData();
  }, [branchId]);

  async function handleSubmit(event) {
    event.preventDefault();

    const trimmedName = name.trim();
    const parsedStock = Number(stock);

    if (!trimmedName) {
      setFormError('Name is required.');
      return;
    }

    if (stock.trim() === '' || !Number.isInteger(parsedStock) || parsedStock < 0) {
      setFormError('Stock must be a whole number greater than or equal to 0.');
      return;
    }

    setSaving(true);
    setFormError('');

    try {
      const created = await createProduct(branchId, trimmedName, parsedStock);
      setProducts((current) =>
        [...current, created].sort((a, b) => a.name.localeCompare(b.name)),
      );
      setName('');
      setStock('0');
    } catch (err) {
      setFormError(err.message);
    } finally {
      setSaving(false);
    }
  }

  function handleProductUpdated(updated) {
    setProducts((current) =>
      current.map((product) => (product.id === updated.id ? updated : product)),
    );
  }

  if (loading) {
    return <p className="muted">Loading products...</p>;
  }

  if (error) {
    return (
      <section>
        <Link to="/franchises" className="back-link">← Back to franchises</Link>
        <p className="error">{error}</p>
      </section>
    );
  }

  return (
    <section>
      <Link to={`/franchises/${branch.franchiseId}/branches`} className="back-link">
        ← Back to branches
      </Link>
      <h1>Products of {branch.name}</h1>

      <form className="card form" onSubmit={handleSubmit}>
        <input
          type="text"
          placeholder="Product name"
          value={name}
          maxLength={100}
          onChange={(event) => setName(event.target.value)}
          disabled={saving}
        />
        <input
          type="number"
          min="0"
          step="1"
          className="stock-input"
          value={stock}
          onChange={(event) => setStock(event.target.value)}
          disabled={saving}
          aria-label="Initial stock"
        />
        <button type="submit" disabled={saving}>
          {saving ? 'Saving...' : 'Add product'}
        </button>
      </form>
      {formError && <p className="error">{formError}</p>}

      {products.length === 0 && (
        <p className="muted">This branch has no products yet.</p>
      )}

      <ul className="list">
        {products.map((product) => (
          <ProductRow
            key={product.id}
            product={product}
            onUpdated={handleProductUpdated}
          />
        ))}
      </ul>
    </section>
  );
}