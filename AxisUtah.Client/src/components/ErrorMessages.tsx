import React from 'react'
import { AxiosError } from 'axios'

interface ErrorMessageProps {
  error: unknown
  fallbackMessage?: string
}

const parseErrorMessage = (err: unknown, fallback: string): string => {
  if (typeof err === 'string') return err

  const axiosErr = err as AxiosError<any>

  const apiMessage = axiosErr.response?.data?.message || axiosErr.response?.data?.title
  if (apiMessage) return apiMessage

  if (axiosErr.response?.data?.errors) {
    return Object.values(axiosErr.response.data.errors).flat().join(', ')
  }

  if (import.meta.env.DEV) {
    if (typeof axiosErr.response?.data === 'string' && axiosErr.response.data.trim()) {
      return `[DEV ${axiosErr.response.status}]: ${axiosErr.response.data}`
    }
    if (axiosErr.message) {
      return `[DEV]: ${axiosErr.message}`
    }
  }

  return fallback
}

export const ErrorMessage: React.FC<ErrorMessageProps> = ({
  error,
  fallbackMessage = 'An unexpected error occurred.',
}) => {
  if (!error) return null

  const displayMessage = parseErrorMessage(error, fallbackMessage)

  return (
    <div className="error-alert" role="alert">
      <svg className="error-icon" fill="currentColor" viewBox="0 0 20 20">
        <path
          fillRule="evenodd"
          d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z"
          clipRule="evenodd"
        />
      </svg>
      <span>{displayMessage}</span>
    </div>
  )
}