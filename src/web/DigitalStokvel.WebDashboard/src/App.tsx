import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom'
import { AuthProvider } from './contexts/AuthContext'
import { Login } from './components/Login'
import { Dashboard } from './components/Dashboard'
import MemberManagement from './components/MemberManagement'
import ContributionTracking from './components/ContributionTracking'
import PayoutApproval from './components/PayoutApproval'
import { ProtectedRoute } from './components/ProtectedRoute'
import './App.css'

function App() {
  return (
    <AuthProvider>
      <Router>
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route
            path="/dashboard"
            element={
              <ProtectedRoute>
                <Dashboard />
              </ProtectedRoute>
            }
          />
          <Route
            path="/dashboard/members/:groupId"
            element={
              <ProtectedRoute>
                <MemberManagement />
              </ProtectedRoute>
            }
          />
          <Route
            path="/dashboard/contributions/:groupId"
            element={
              <ProtectedRoute>
                <ContributionTracking />
              </ProtectedRoute>
            }
          />
          <Route
            path="/dashboard/payouts/:groupId"
            element={
              <ProtectedRoute>
                <PayoutApproval />
              </ProtectedRoute>
            }
          />
          <Route path="/" element={<Navigate to="/login" replace />} />
          <Route path="/home" element={<Home />} />
        </Routes>
      </Router>
    </AuthProvider>
  )
}

function Home() {
  return (
    <div className="home">
      <h2>Welcome to Digital Stokvel</h2>
      <p>This is the Chairperson Dashboard for managing your stokvel groups.</p>
      <div className="features">
        <div className="feature-card">
          <h3>📊 Member Management</h3>
          <p>View and manage group members</p>
        </div>
        <div className="feature-card">
          <h3>💰 Contribution Tracking</h3>
          <p>Monitor group contributions</p>
        </div>
        <div className="feature-card">
          <h3>✅ Payout Approval</h3>
          <p>Approve and manage payouts</p>
        </div>
        <div className="feature-card">
          <h3>📈 Reports & Export</h3>
          <p>Generate reports and export data</p>
        </div>
      </div>
    </div>
  )
}

export default App
