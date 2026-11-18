import { Navigate, NavLink, Route, Routes } from 'react-router-dom';
import FranchisesPage from './pages/FranchisesPage';

export default function App() {
  return (
    <div className="app">
      <header className="app-header">
        <NavLink to="/franchises" className="brand">
          Franchise Manager
        </NavLink>
      </header>

      <main className="container">
        <Routes>
          <Route path="/" element={<Navigate to="/franchises" replace />} />
          <Route path="/franchises" element={<FranchisesPage />} />
          <Route path="*" element={<p>Page not found.</p>} />
        </Routes>
      </main>
    </div>
  );
}