import { Navigate, Outlet } from 'react-router-dom'
import '../styles/protected-route.css'

export default function ProtectedRoute() {
  const token = localStorage.getItem('token')

  if (!token) {
    return (
      <div className="protected-container">
        <div className="protected-card">
          {/* Warning Icon */}
          <div className="protected-icon">
            <svg
              xmlns="http://www.w3.org/2000/svg"
              fill="none"
              viewBox="0 0 24 24"
              strokeWidth={2}
              stroke="currentColor"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                d="M12 9v3.75m-9.303 3.376c.865.865 2.291 1.382 3.821 1.382H15a4.5 4.5 0 1 0 0-9H6.518c-1.53 0-2.956.517-3.821 1.382A4.505 4.505 0 0 0 1.5 12c0 1.046.215 2.053.581 2.952Z"
              />
            </svg>
          </div>

          <h1 className="protected-title">Access Restricted</h1>
          <p className="protected-message">
            You do not have permission to view this page. Please sign in to access your Axis agent portal.
          </p>

          {/* Action Button */}
          <a href="/login" className="protected-button">
            Return to Sign In
          </a>
        </div>
      </div>
    )
  }

  return <Outlet />
}
