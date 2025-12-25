# Login Test Guide

## API Credentials

### Admin User
- **Email**: `admin@arunayan.com`
- **Password**: `admin123`
- **Expected Navigation**: Should go to Admin Dashboard

### Customer User
- **Email**: `customer@test.com`
- **Password**: `customer123`
- **Expected Navigation**: Should go to Products page (TabBar)

## Testing Steps

1. **API is Running**: http://localhost:5001
   - Swagger: http://localhost:5001/swagger/index.html
   - Status: ✅ Running (Process ID: 97305)

2. **Test API Login**:
   ```bash
   # Admin login
   curl -X POST http://localhost:5001/api/auth/login \
     -H "Content-Type: application/json" \
     -d '{"email":"admin@arunayan.com","password":"admin123"}'
   
   # Customer login
   curl -X POST http://localhost:5001/api/auth/login \
     -H "Content-Type: application/json" \
     -d '{"email":"customer@test.com","password":"customer123"}'
   ```

3. **Launch MAUI App**:
   - The app should start on Login page
   - Enter credentials above
   - After login, should navigate to respective page

## Potential Issues

### Issue 1: Navigation Routes
The Shell navigation uses `//` prefix for absolute routes:
- `//AdminDashboard` - Should work (defined in AppShell.xaml)
- `//Products` - Should work (defined in TabBar)

### Issue 2: Base URL Configuration
For iOS Simulator/Mac: `http://localhost:5001`
For Android Emulator: `http://10.0.2.2:5001`

### Issue 3: Initial Page
App starts with AppShell which shows tabs by default. Need to check if LoginPage is set as initial route.

## Solution

The AppShell.xaml needs to be updated to:
1. Start with LoginPage as the default
2. Hide TabBar when on auth pages
3. Show TabBar only after successful login
