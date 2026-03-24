import { useAuth } from '../contexts/AuthContext';
import { useNavigate } from 'react-router-dom';
import './Dashboard.css';

export function Dashboard() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <div className="dashboard">
      <header className="dashboard-header">
        <div className="header-content">
          <h1>Digital Stokvel</h1>
          <div className="user-info">
            <span className="user-name">
              {user?.firstName} {user?.lastName}
            </span>
            <span className="user-role">
              {user?.roles.includes('chairperson') && '👑 Chairperson'}
            </span>
            <button onClick={handleLogout} className="logout-button">
              Logout
            </button>
          </div>
        </div>
      </header>

      <main className="dashboard-main">
        <div className="welcome-section">
          <h2>Welcome back, {user?.firstName}!</h2>
          <p>Manage your stokvel groups and track contributions.</p>
        </div>

        <div className="features-grid">
          <div className="feature-card">
            <div className="feature-icon">📊</div>
            <h3>Member Management</h3>
            <p>View and manage group members</p>
            <button className="feature-button">Manage Members</button>
          </div>

          <div className="feature-card">
            <div className="feature-icon">💰</div>
            <h3>Contribution Tracking</h3>
            <p>Monitor group contributions and payments</p>
            <button className="feature-button">View Contributions</button>
          </div>

          <div className="feature-card">
            <div className="feature-icon">✅</div>
            <h3>Payout Approval</h3>
            <p>Approve and manage group payouts</p>
            <button className="feature-button">Approve Payouts</button>
          </div>

          <div className="feature-card">
            <div className="feature-icon">📈</div>
            <h3>Reports & Export</h3>
            <p>Generate reports and export data</p>
            <button className="feature-button">View Reports</button>
          </div>
        </div>

        <div className="groups-section">
          <h2>Your Groups</h2>
          <p className="placeholder-text">
            Your stokvel groups will be displayed here once member management is implemented (Task 3.4.3).
          </p>
        </div>
      </main>
    </div>
  );
}
