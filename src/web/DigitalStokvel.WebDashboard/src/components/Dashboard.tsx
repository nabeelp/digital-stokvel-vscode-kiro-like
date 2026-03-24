import { useAuth } from '../contexts/AuthContext';
import { useNavigate } from 'react-router-dom';
import { useState, useEffect } from 'react';
import { getUserGroups } from '../services/groupService';
import type { GroupSummary } from '../types/group';
import './Dashboard.css';

export function Dashboard() {
  const { user, token, logout } = useAuth();
  const navigate = useNavigate();
  const [groups, setGroups] = useState<GroupSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadGroups();
  }, [token]);

  const loadGroups = async () => {
    if (!token) return;

    try {
      setLoading(true);
      setError(null);
      const response = await getUserGroups(token, 'active');
      setGroups(response.groups);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load groups');
    } finally {
      setLoading(false);
    }
  };

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
            <button className="feature-button" disabled>Select a group below</button>
          </div>

          <div className="feature-card">
            <div className="feature-icon">💰</div>
            <h3>Contribution Tracking</h3>
            <p>Monitor group contributions and payments</p>
            <button className="feature-button" disabled>Select a group below</button>
          </div>

          <div className="feature-card">
            <div className="feature-icon">✅</div>
            <h3>Payout Approval</h3>
            <p>Approve and manage group payouts</p>
            <button className="feature-button" disabled>Select a group below</button>
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
          {loading && <p className="placeholder-text">Loading your groups...</p>}
          {error && <p className="error-text">{error}</p>}
          {!loading && !error && groups.length === 0 && (
            <p className="placeholder-text">You are not a member of any groups yet.</p>
          )}
          {!loading && !error && groups.length > 0 && (
            <div className="groups-grid">
              {groups.map((group) => (
                <div key={group.id} className="group-card">
                  <div className="group-header">
                    <h3>{group.name}</h3>
                    <span className="role-badge">
                      {group.role === 'chairperson' && '👑'}
                      {group.role === 'treasurer' && '💰'}
                      {group.role === 'secretary' && '📝'}
                      {group.role === 'member' && '👤'}
                      {' '}{group.role}
                    </span>
                  </div>
                  <div className="group-stats">
                    <div className="stat">
                      <span className="stat-label">Balance</span>
                      <span className="stat-value">R{group.balance.toFixed(2)}</span>
                    </div>
                    <div className="stat">
                      <span className="stat-label">Members</span>
                      <span className="stat-value">{group.memberCount}</span>
                    </div>
                  </div>
                  {group.nextContributionDue && (
                    <p className="next-contribution">
                      Next contribution due: {new Date(group.nextContributionDue).toLocaleDateString()}
                    </p>
                  )}
                  <button 
                    className="manage-button"
                    onClick={() => navigate(`/dashboard/members/${group.id}`)}
                  >
                    Manage Members
                  </button>
                  <button 
                    className="contributions-button"
                    onClick={() => navigate(`/dashboard/contributions/${group.id}`)}
                  >
                    View Contributions
                  </button>
                  <button 
                    className="payouts-button"
                    onClick={() => navigate(`/dashboard/payouts/${group.id}`)}
                  >
                    Approve Payouts
                  </button>
                  <button 
                    className="reports-button"
                    onClick={() => navigate(`/dashboard/reports/${group.id}`)}
                  >
                    View Reports
                  </button>
                </div>
              ))}
            </div>
          )}
        </div>
      </main>
    </div>
  );
}
