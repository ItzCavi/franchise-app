import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { getFranchiseReport } from '../api/franchises';

export default function ReportPage() {
  const { franchiseId } = useParams();

  const [report, setReport] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    async function loadReport() {
      try {
        const data = await getFranchiseReport(franchiseId);
        setReport(data);
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    }

    loadReport();
  }, [franchiseId]);

  if (loading) {
    return <p className="muted">Loading report...</p>;
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
      <Link to={`/franchises/${report.franchiseId}/branches`} className="back-link">
        ← Back to branches
      </Link>
      <h1>Stock report – {report.franchiseName}</h1>

      <div className="stats">
        <div className="card stat">
          <span className="stat-value">{report.branchCount}</span>
          <span className="muted">Branches</span>
        </div>
        <div className="card stat">
          <span className="stat-value">{report.productCount}</span>
          <span className="muted">Products</span>
        </div>
        <div className="card stat">
          <span className="stat-value">{report.totalStock}</span>
          <span className="muted">Total stock</span>
        </div>
      </div>

      {report.branches.length === 0 ? (
        <p className="muted">This franchise has no branches yet.</p>
      ) : (
        <div className="card table-wrapper">
          <table>
            <thead>
              <tr>
                <th>Branch</th>
                <th>Products</th>
                <th>Total stock</th>
                <th>Top product</th>
              </tr>
            </thead>
            <tbody>
              {report.branches.map((branch) => (
                <tr key={branch.branchId}>
                  <td>
                    <Link to={`/branches/${branch.branchId}/products`}>{branch.branchName}</Link>
                  </td>
                  <td>{branch.productCount}</td>
                  <td>{branch.totalStock}</td>
                  <td>
                    {branch.topProductName
                      ? `${branch.topProductName} (${branch.topProductStock})`
                      : '—'}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  );
}