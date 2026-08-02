# Component Development Guide

This guide helps you add new pages and components to the Axis Utah Agent Portal following established patterns.

## 📝 Page Component Template

### File Structure
```
src/pages/MyFeature.tsx
src/styles/my-feature.css
```

### TypeScript Page Component

```typescript
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import '../styles/my-feature.css'

export default function MyFeature() {
  const navigate = useNavigate()
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  const handleAction = async () => {
    try {
      setLoading(true)
      setError('')
      
      // Your logic here
      
    } catch (err: any) {
      setError(err.response?.data?.message || 'An error occurred')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="my-feature-container">
      <header className="my-feature-header">
        <h1>Feature Title</h1>
      </header>

      {error && (
        <div className="error-alert">
          <span>{error}</span>
        </div>
      )}

      <main className="my-feature-main">
        {/* Content here */}
      </main>
    </div>
  )
}
```

### CSS Styling Template

```css
/* My Feature Styling */

:root {
  --axis-gold: #d4af37;
  --axis-dark: #1a1a2e;
  --axis-darker: #0f0f1e;
  --axis-text-light: #e8e8e8;
  --axis-text-muted: #a0a0a0;
  --axis-border: #3a3a4e;
}

.my-feature-container {
  min-height: 100vh;
  background: linear-gradient(135deg, var(--axis-darker) 0%, var(--axis-dark) 100%);
  display: flex;
  flex-direction: column;
}

.my-feature-header {
  background: linear-gradient(135deg, var(--axis-dark) 0%, #2a3f5f 100%);
  border-bottom: 1px solid var(--axis-border);
  padding: 1.5rem;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
}

.my-feature-header h1 {
  font-size: 1.875rem;
  color: var(--axis-gold);
  margin: 0;
}

.error-alert {
  margin: 1rem;
  padding: 1rem;
  background-color: rgba(239, 68, 68, 0.1);
  border: 1px solid rgba(239, 68, 68, 0.3);
  border-radius: 8px;
  color: #fca5a5;
  font-size: 0.875rem;
}

.my-feature-main {
  flex: 1;
  padding: 2rem 1rem;
  overflow-y: auto;
}

@media (max-width: 640px) {
  .my-feature-header {
    padding: 1rem;
  }

  .my-feature-header h1 {
    font-size: 1.5rem;
  }
}
```

## 🛣️ Adding Routes

### Update App.tsx

```typescript
import MyFeature from './pages/MyFeature'

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/login" element={<Login />} />
        <Route path="/dashboard" element={<ProtectedRoute />}>
          <Route index element={<Dashboard />} />
          <Route path="my-feature" element={<MyFeature />} />
        </Route>
        {/* ... */}
      </Routes>
    </Router>
  )
}
```

## 🔄 API Usage Pattern

### Making Authenticated Requests

```typescript
import api from '../api/client'

// GET request
const fetchData = async () => {
  try {
    const response = await api.get('/endpoint')
    return response.data
  } catch (error) {
    throw error
  }
}

// POST request
const createItem = async (data: any) => {
  try {
    const response = await api.post('/endpoint', data)
    return response.data
  } catch (error) {
    throw error
  }
}

// PUT request
const updateItem = async (id: number, data: any) => {
  try {
    const response = await api.put(`/endpoint/${id}`, data)
    return response.data
  } catch (error) {
    throw error
  }
}

// DELETE request
const deleteItem = async (id: number) => {
  try {
    const response = await api.delete(`/endpoint/${id}`)
    return response.data
  } catch (error) {
    throw error
  }
}
```

## 🎯 Common Patterns

### Form Handling

```typescript
const [formData, setFormData] = useState({
  name: '',
  email: '',
  message: '',
})
const [errors, setErrors] = useState<Record<string, string>>({})

const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
  const { name, value } = e.target
  setFormData(prev => ({
    ...prev,
    [name]: value,
  }))
}

const handleSubmit = async (e: React.FormEvent) => {
  e.preventDefault()
  setErrors({})

  try {
    // Validation
    if (!formData.name) {
      setErrors(prev => ({ ...prev, name: 'Name is required' }))
      return
    }

    // API call
    const response = await api.post('/endpoint', formData)

    // Success handling
    console.log('Success:', response.data)
  } catch (error: any) {
    setErrors(prev => ({
      ...prev,
      submit: error.response?.data?.message || 'An error occurred',
    }))
  }
}
```

### List Loading with States

```typescript
const [items, setItems] = useState([])
const [loading, setLoading] = useState(true)
const [error, setError] = useState('')

useEffect(() => {
  fetchItems()
}, [])

const fetchItems = async () => {
  try {
    setLoading(true)
    setError('')
    const response = await api.get('/items')
    setItems(response.data)
  } catch (err: any) {
    setError('Failed to load items')
  } finally {
    setLoading(false)
  }
}

// In JSX
{loading && <div className="loading">Loading...</div>}
{error && <div className="error">{error}</div>}
{items.length === 0 && !loading && (
  <div className="empty-state">No items found</div>
)}
{items.map(item => (
  <div key={item.id}>{item.name}</div>
))}
```

### Modal/Dialog Pattern

```typescript
const [isOpen, setIsOpen] = useState(false)

const handleOpenModal = () => setIsOpen(true)
const handleCloseModal = () => setIsOpen(false)

// In JSX
{isOpen && (
  <div className="modal-overlay" onClick={handleCloseModal}>
    <div className="modal-content" onClick={e => e.stopPropagation()}>
      <h2>Modal Title</h2>
      <p>Modal content here</p>
      <button onClick={handleCloseModal}>Close</button>
    </div>
  </div>
)}
```

### Modal CSS

```css
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 999;
}

.modal-content {
  background: linear-gradient(135deg, #1a1a2e 0%, #242b3f 100%);
  border: 1px solid var(--axis-border);
  border-radius: 12px;
  padding: 2rem;
  max-width: 500px;
  width: 90%;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.5);
}
```

## 🎨 Reusable Styles

### Button Variants

```css
/* Primary Button */
.btn-primary {
  padding: 0.75rem 1.5rem;
  background: linear-gradient(135deg, var(--axis-gold) 0%, #e5c158 100%);
  color: var(--axis-dark);
  border: none;
  border-radius: 6px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
}

.btn-primary:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(212, 175, 55, 0.4);
}

/* Secondary Button */
.btn-secondary {
  padding: 0.75rem 1.5rem;
  background: transparent;
  color: var(--axis-gold);
  border: 1px solid var(--axis-gold);
  border-radius: 6px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
}

.btn-secondary:hover {
  background: rgba(212, 175, 55, 0.1);
}
```

### Input Variants

```css
.input-field {
  padding: 0.75rem 1rem;
  background-color: rgba(15, 15, 30, 0.6);
  border: 1px solid var(--axis-border);
  border-radius: 6px;
  color: var(--axis-text-light);
  font-size: 0.9375rem;
  transition: all 0.3s ease;
}

.input-field:focus {
  border-color: var(--axis-gold);
  background-color: rgba(15, 15, 30, 0.8);
  box-shadow: 0 0 0 3px rgba(212, 175, 55, 0.1);
}

.input-field:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
```

### Card Component

```css
.card {
  background: linear-gradient(135deg, #1a1a2e 0%, #242b3f 100%);
  border: 1px solid var(--axis-border);
  border-radius: 12px;
  padding: 1.5rem;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.3);
  transition: all 0.3s ease;
}

.card:hover {
  border-color: var(--axis-gold);
  transform: translateY(-4px);
  box-shadow: 0 15px 50px rgba(212, 175, 55, 0.1);
}
```

## 📋 Checklist for New Features

- [ ] Created page component in `src/pages/`
- [ ] Created CSS file in `src/styles/`
- [ ] Added route to `App.tsx`
- [ ] Imported necessary types and components
- [ ] Implemented error handling
- [ ] Added loading states
- [ ] Made responsive (tested on mobile)
- [ ] Used CSS variables for colors
- [ ] Added accessibility attributes
- [ ] Tested with backend API
- [ ] Documented usage

## 🧪 Testing Components

```typescript
// Example test structure (for future Jest/React Testing Library)
import { render, screen } from '@testing-library/react'
import MyFeature from '../MyFeature'

describe('MyFeature', () => {
  it('renders the feature title', () => {
    render(<MyFeature />)
    expect(screen.getByText('Feature Title')).toBeInTheDocument()
  })

  it('handles form submission', async () => {
    render(<MyFeature />)
    // Test implementation
  })
})
```

---

For more examples, refer to existing components:
- `src/pages/Login.tsx` - Form handling, error states
- `src/pages/Dashboard.tsx` - Layout structure, action cards
- `src/components/ProtectedRoute.tsx` - Route guarding
