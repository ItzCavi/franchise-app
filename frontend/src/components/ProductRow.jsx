import { useState } from 'react';
import { updateProductStock } from '../api/products';

export default function ProductRow({ product, onUpdated }) {
  const [stock, setStock] = useState(String(product.stock));
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const parsedStock = Number(stock);
  const isValid = stock.trim() !== '' && Number.isInteger(parsedStock) && parsedStock >= 0;
  const hasChanged = parsedStock !== product.stock;

  async function handleSubmit(event) {
    event.preventDefault();

    if (!isValid) {
      setError('Stock must be a whole number greater than or equal to 0.');
      return;
    }

    setSaving(true);
    setError('');

    try {
      const updated = await updateProductStock(product.id, parsedStock);
      onUpdated(updated);
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  }

  return (
    <li className="card">
      <div className="list-item">
        <div>
          <strong>{product.name}</strong>
          <p className="muted small">Current stock: {product.stock}</p>
        </div>

        <form className="stock-form" onSubmit={handleSubmit}>
          <input
            type="number"
            min="0"
            step="1"
            value={stock}
            onChange={(event) => setStock(event.target.value)}
            disabled={saving}
            aria-label={`Stock for ${product.name}`}
          />
          <button type="submit" disabled={saving || !hasChanged}>
            {saving ? 'Saving...' : 'Update'}
          </button>
        </form>
      </div>
      {error && <p className="error">{error}</p>}
    </li>
  );
}