import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { getFranchise } from '../api/franchises';
import { createBranch, getBranchesByFranchise } from '../api/branches';

export default function BranchesPage() {
  const { franchiseId } = useParams();

  const [franchise, setFranchise] = useState(null);
  const [branches, setBranches] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const [name, setName] = useState('');
  const [saving, setSaving] = useState(false);
  const [formError, setFormError] = useState('');

  useEffect(() => {
    async function loadData() {
      try {
        const [franchiseData, branchesData] = await Promise.all([
          getFranchise(franchiseId),
          getBranchesByFranchise(franchiseId),
        ]);
        setFranchise(franchiseData);
        setBranches(branchesData);
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    }

    loadData();
  }, [franchiseId]);

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
      const created = await createBranch(franchiseId, trimmedName);
      setBranches((current) =>
        [...current, created].sort((a, b) => a.name.localeCompare(b.name)),
      );
      setName('');
    } catch (err) {
      setFormError(err.message);
    } finally {
      setSaving(false);
    }
  }

  if (loading) {
    return <p className="muted">Loading branches...</p>;
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
      <Link to="/franchises" className="back-link">← Back to franchises</Link>
      <h1>Branches of {franchise.name}</h1>

      <form className="card form" onSubmit={handleSubmit}>
        <input
          type="text"
          placeholder="Branch name"
          value={name}
          maxLength={100}
          onChange={(event) => setName(event.target.value)}
          disabled={saving}
        />
        <button type="submit" disabled={saving}>
          {saving ? 'Saving...' : 'Add branch'}
        </button>
      </form>
      {formError && <p className="error">{formError}</p>}

      {branches.length === 0 && (
        <p className="muted">This franchise has no branches yet.</p>
      )}

      <ul className="list">
        {branches.map((branch) => (
          <li key={branch.id} className="card list-item">
            <span>{branch.name}</span>
            <Link to={`/branches/${branch.id}/products`}>View products →</Link>
          </li>
        ))}
      </ul>
    </section>
  );
}