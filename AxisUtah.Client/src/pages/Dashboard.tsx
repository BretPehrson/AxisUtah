import { useNavigate, Link } from 'react-router-dom'
import { logout } from '../api/client'
import '../styles/dashboard.css'

export default function Dashboard() {
  const navigate = useNavigate()

  const handleLogout = async () => {
    await logout()
  }

  return (
    <div className="dashboard-container">
      <header className="dashboard-header">
        <div className="header-content">
          <h1 className="header-title">Agent Portal</h1>
          <div className="header-actions">
            <Link to="/dashboard/logs" className="logs-link">
              View Logs
            </Link>
            <button onClick={handleLogout} className="logout-button">
              Sign Out
            </button>
          </div>
        </div>
      </header>

      <main className="dashboard-main">
        <div className="welcome-card">
          <h2>Welcome to Axis Realty Group Agent Portal</h2>
          <p>
            You have successfully authenticated. This is your secure dashboard where you
            can manage your listings and account information.
          </p>

          <div className="action-grid">
            <div className="action-card">
              <h3>My Listings</h3>
              <p>View and manage your active property listings</p>
            </div>

            <div className="action-card">
              <h3>Sales History</h3>
              <p>Review completed transactions and sales performance</p>
            </div>

            <div className="action-card">
              <h3>Clients</h3>
              <p>Manage your buyer and seller relationships</p>
            </div>

            <div className="action-card">
              <h3>Account Settings</h3>
              <p>Update your profile and preferences</p>
            </div>
          </div>
        </div>
      </main>
    </div>
  )
}
