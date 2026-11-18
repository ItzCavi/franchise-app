import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { createFranchise, getFranchises } from '../api/franchises';

export default function FranchisesPage() {
  const [franchises, setFranchises] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const [name, setName] = useState('');
  const [saving, setSaving] = useState(false);
  const [formError, setFormError] = useState('');

  useEffect(() => {
    async function loadFranchises() {
      try {
        const data = await getFranchises();
        setFranchises(data);
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    }

    loadFranchises();
  }, []);

  async function handleSubmit(event) {
    event.preventDefault();

    const trimmedName = name.trim();
    if (!trimmedName) {
      setFormError('Name is required.');
      return;
    }

    setSaving(true);
    setFormError('');

    try {
      const created = await createFranchise(trimmedName);
      setFranchises((current) =>
        [...current, created].sort((a, b) => a.name.localeCompare(b.name)),
      );
      setName('');
    } catch (err) {
      setFormError(err.message);
    } finally {
      setSaving(false);
    }
  }

  return (
    <section>
      <h1>Franchises</h1>

      <form className="card form" onSubmit={handleSubmit}>
        <input
          type="text"
          placeholder="Franchise name"
          value={name}
          maxLength={100}
          onChange={(event) => setName(event.target.value)}
          disabled={saving}
        />
        <button type="submit" disabled={saving}>
          {saving ? 'Saving...' : 'Add franchise'}
        </button>
      </form>
      {formError && <p className="error">{formError}</p>}

      {loading && <p className="muted">Loading franchises...</p>}
      {error && <p className="error">{error}</p>}

      {!loading && !error && franchises.length === 0 && (
        <p className="muted">No franchises yet. Create the first one above.</p>
      )}

      <ul className="list">
        {franchises.map((franchise) => (
          <li key={franchise.id} className="card list-item">
            <span>{franchise.name}</span>
            <Link to={`/franchises/${franchise.id}/branches`}>View branches →</Link>
          </li>
        ))}
      </ul>
    </section>
  );
}