# Axis Utah Client - Setup Guide

## 🎯 What Was Built

A professional React/TypeScript login page for the Axis Realty Group Agent Portal that matches the luxury branding of www.axisutah.com. The implementation follows the authentication pattern from the PennySaver.IO project.

## 📋 Files Created/Modified

### New Files Created
- `src/pages/Login.tsx` - Login/Registration page component
- `src/pages/Dashboard.tsx` - Authenticated dashboard (placeholder)
- `src/components/ProtectedRoute.tsx` - Route guard component
- `src/api/client.ts` - Axios configuration with JWT interceptors
- `src/styles/login.css` - Login page styling (Axis branding)
- `src/styles/dashboard.css` - Dashboard styling
- `src/styles/protected-route.css` - Protected route styling
- `.env.example` - Environment configuration template

### Modified Files
- `package.json` - Added axios, react-router-dom, tailwindcss dependencies
- `App.tsx` - Updated with React Router setup
- `src/index.css` - Global styles with Axis color scheme

## 🚀 Quick Start

### Step 1: Install Dependencies

```bash
cd /Users/bretpehrson/Projects/AxisUtah/AxisUtah.Client
npm install
```

### Step 2: Configure Environment

```bash
cp .env.example .env.local
```

Then edit `.env.local` to point to your backend API:

```env
VITE_API_URL=http://localhost:5000/api
```

### Step 3: Start Development Server

```bash
npm run dev
```

The application will be available at `http://localhost:5173`

## 🎨 Design Details

### Color Palette (Inspired by axisutah.com)

| Color | Hex | Usage |
|-------|-----|-------|
| Gold | #d4af37 | Buttons, accents, hover states |
| Dark Background | #1a1a2e | Main background |
| Darker Background | #0f0f1e | Deep backgrounds |
| Light Text | #e8e8e8 | Primary text |
| Muted Text | #a0a0a0 | Secondary text |
| Borders | #3a3a4e | UI borders |

### Logo Design

- Geometric cube icon in gold
- Represents the professional, modern nature of Axis Realty Group
- Placed prominently on the login page

## 🔐 Authentication Flow

```
1. User enters email/password
   ↓
2. POST /api/auth/issuetoken
   ↓
3. Backend returns { token, refresh_token }
   ↓
4. JWT stored in localStorage
   ↓
5. Refresh token stored in httpOnly cookie
   ↓
6. Redirect to /dashboard
   ↓
7. All subsequent requests include JWT in Authorization header
   ↓
8. On 401 error, axios interceptor auto-refreshes token
   ↓
9. If refresh fails, redirect to login
```

## 🧪 Testing the Application

### Test Login Flow

1. Navigate to http://localhost:5173/login
2. Click "Register" to create a test account
3. Enter email: `test@axisutah.com`
4. Enter password: `TestPassword123!`
5. Click "Create Account"
6. Should auto-login and redirect to dashboard

### Test Protected Routes

1. Logout from dashboard
2. Try accessing http://localhost:5173/dashboard
3. Should see "Access Restricted" page
4. Click "Return to Sign In" to go back to login

### Test Token Refresh

1. After login, check browser DevTools Network tab
2. Make any API request
3. Verify Authorization header contains Bearer token
4. Wait for token to expire and make another request
5. Should see refresh request to `/api/auth/refresh`

## 📱 Features Implemented

### Login Page
- ✅ Email/password input fields
- ✅ Toggle between login and registration modes
- ✅ Form validation
- ✅ Error message display
- ✅ Loading states
- ✅ Responsive design (mobile/tablet/desktop)
- ✅ Professional Axis branding
- ✅ Smooth animations

### Authentication
- ✅ JWT token management
- ✅ Automatic token refresh
- ✅ Secure logout
- ✅ Password validation (frontend)
- ✅ Email validation

### Protected Routes
- ✅ Token-based access control
- ✅ Access denied page
- ✅ Automatic redirect to login
- ✅ Protected route component wrapper

### Dashboard
- ✅ Welcome message for authenticated users
- ✅ Sign out button
- ✅ Quick action cards (extensible)
- ✅ Header with authentication status

## 🔧 API Requirements

Your backend API must implement these endpoints:

```
POST /api/auth/register
  Request: { email, password }
  Response: { message: "Registration successful." }

POST /api/auth/issuetoken
  Request: { email, password }
  Response: { token, token_type: "Bearer", expires_utc }

POST /api/auth/refresh
  Request: (empty, cookie-based)
  Response: { token, token_type: "Bearer", expires_utc }

POST /api/auth/logout
  Request: (empty, cookie-based)
  Response: { message: "Logged out successfully." }
```

## 📦 Build for Production

```bash
npm run build
```

Creates optimized production build in `dist/` directory.

## 🌍 Deployment

### Vercel / Netlify
1. Push to GitHub
2. Connect repo to Vercel/Netlify
3. Set environment variable: `VITE_API_URL=https://your-api.com/api`
4. Deploy

### Traditional Server
1. Build: `npm run build`
2. Copy `dist/` contents to web server
3. Configure web server to route all requests to index.html (SPA requirement)
4. Set environment: `VITE_API_URL` via environment or build-time substitution

## 🔍 Troubleshooting

### "Cannot connect to API"
- Verify backend is running on configured port
- Check CORS configuration on backend
- Ensure VITE_API_URL is correct in .env.local

### "Tokens not persisting"
- Verify localStorage is enabled in browser
- Check browser DevTools → Application → Cookies (for refresh token)
- Ensure backend sets httpOnly, Secure, SameSite cookie flags

### "Redirect loop on login"
- Check if token is being stored in localStorage
- Verify API response includes `token` field
- Check browser console for error messages

## 📚 Project Structure

```
AxisUtah.Client/
├── src/
│   ├── api/
│   │   └── client.ts              # Axios + JWT interceptors
│   ├── pages/
│   │   ├── Login.tsx              # Login/Register
│   │   └── Dashboard.tsx          # Authenticated dashboard
│   ├── components/
│   │   └── ProtectedRoute.tsx     # Route guard
│   ├── styles/
│   │   ├── login.css              # Login styling
│   │   ├── dashboard.css          # Dashboard styling
│   │   └── protected-route.css    # Protected route styling
│   ├── App.tsx                    # Router setup
│   ├── main.tsx                   # React entry point
│   └── index.css                  # Global styles
├── vite.config.ts                 # Vite configuration
├── package.json                   # Dependencies
├── .env.example                   # Environment template
└── README.md                       # Project documentation
```

## 🎓 Key Technologies

- **React 19.2.7** - UI library
- **TypeScript 6.0.2** - Type safety
- **Vite 8.1.1** - Build tool (lightning-fast)
- **React Router 7.1.0** - Client-side routing
- **Axios 1.7.2** - HTTP client with interceptors
- **CSS Variables** - Themeable styling system

## 📖 Additional Resources

- [React Router Documentation](https://reactrouter.com/)
- [Axios Documentation](https://axios-http.com/)
- [Vite Documentation](https://vitejs.dev/)
- [JWT Best Practices](https://tools.ietf.org/html/rfc8725)

## 🤝 Next Steps

1. **Customize Dashboard**: Add real components for agent portal features
2. **Add User Profile**: Implement profile page and settings
3. **Extend API Calls**: Add more endpoints for listings, clients, etc.
4. **Add Tests**: Implement unit and integration tests
5. **Styling**: Enhance with more Axis branding elements
6. **Performance**: Add code splitting and lazy loading

## 📞 Support

For issues or questions about the implementation, refer to:
- The inline code comments
- PennySaver.IO implementation (similar auth pattern)
- React Router and Axios documentation

---

**Last Updated**: July 31, 2024
**Status**: Ready for Development
