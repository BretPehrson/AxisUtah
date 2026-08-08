import { useState } from 'react'
import type { SyntheticEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { authApi } from '../api/client'
import { ErrorMessage } from '../components/ErrorMessages'
import '../styles/login.css'

export default function Login() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<unknown>(null)
  const [loading, setLoading] = useState(false)
  const [showRegister, setShowRegister] = useState(false)
  const navigate = useNavigate()

  const handleSubmit = async (e: SyntheticEvent<HTMLFormElement>) => {
    e.preventDefault()
    setError(null)

    if (!email || !password) {
      setError('Email and password are required.')
      return
    }

    try {
      setLoading(true)
      const response = await authApi.login(email, password)
      const { token } = response.data
      localStorage.setItem('token', token)
      navigate('/dashboard')
    } catch (err: unknown) {
      // Pass the RAW caught error so response data isn't lost!
      setError(err)
    } finally {
      setLoading(false)
    }
  }

  const handleRegister = async (e: SyntheticEvent<HTMLFormElement>) => {
    e.preventDefault()
    setError(null)

    if (!email || !password) {
      setError('Email and password are required.')
      return
    }

    if (password.length < 6) {
      setError('Password must be at least 6 characters.')
      return
    }

    try {
      setLoading(true)
      await authApi.register(email, password)
      setError(null)
      setShowRegister(false)
      setEmail('')
      setPassword('')
      
      const loginResponse = await authApi.login(email, password)
      const { token } = loginResponse.data
      localStorage.setItem('token', token)
      navigate('/dashboard')
    } catch (err: unknown) {
      // Pass the RAW caught error
      setError(err)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="login-container">
      <div className="login-background" />

      <div className="login-card">
        <div className="login-header">
          <div className="logo-container">
            <div className="logo-icon">
              <svg viewBox="0 0 40 40" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M20 2 L38 10 L38 30 L20 38 L2 30 L2 10 Z" stroke="currentColor" strokeWidth="1.5" fill="none" />
                <path d="M20 2 L20 20 M20 20 L2 10 M20 20 L38 10" stroke="currentColor" strokeWidth="1.5" />
              </svg>
            </div>
          </div>
          <h1 className="login-title">AXIS</h1>
          <p className="login-subtitle">Realty Group</p>
          <p className="login-description">
            {showRegister ? 'Create your agent portal account' : 'Agent Portal Access'}
          </p>
        </div>

       <ErrorMessage error={error} fallbackMessage={showRegister ? 'An error occurred during registration.' : 'An error occurred during login.'} />

        <form className="login-form" onSubmit={showRegister ? handleRegister : handleSubmit}>
          <div className="form-group">
            <label htmlFor="email" className="form-label">Email Address</label>
            <input
              id="email"
              type="email"
              required
              placeholder="agent@axisutah.com"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="form-input"
              disabled={loading}
            />
          </div>

          <div className="form-group">
            <label htmlFor="password" className="form-label">Password</label>
            <input
              id="password"
              type="password"
              required
              placeholder="••••••••"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="form-input"
              disabled={loading}
            />
          </div>

          <button type="submit" disabled={loading} className="submit-button">
            {loading
              ? showRegister
                ? 'Creating Account...'
                : 'Signing In...'
              : showRegister
                ? 'Create Account'
                : 'Sign In'}
          </button>
        </form>

        <div className="form-footer">
          <span className="form-footer-text">
            {showRegister ? 'Already have an account?' : "Don't have an account?"}
          </span>
          <button
            type="button"
            onClick={() => {
              setShowRegister(!showRegister)
              setError(null)
              setEmail('')
              setPassword('')
            }}
            className="toggle-button"
            disabled={loading}
          >
            {showRegister ? 'Sign In' : 'Register'}
          </button>
        </div>
      </div>

      <div className="login-footer">
        <p>&copy; 2024 Axis Realty Group. All rights reserved.</p>
      </div>
    </div>
  )
}