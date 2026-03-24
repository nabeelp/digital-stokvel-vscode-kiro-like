import { BrowserRouter as Router, Routes, Route } from 'react-router-dom'
import './App.css'

function App() {
  return (
    <Router>
      <div className="app">
        <header className="app-header">
          <h1>Digital Stokvel</h1>
          <p className="subtitle">Chairperson Dashboard</p>
        </header>
        <main className="app-main">
          <Routes>
            <Route path="/" element={<Home />} />
          </Routes>
        </main>
      </div>
    </Router>
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
