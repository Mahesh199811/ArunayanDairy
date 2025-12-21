# 🥛 Arunayan Dairy - Full Stack Authentication System

A complete authentication system built with .NET MAUI (mobile app) and .NET 9 Web API (backend), featuring JWT authentication, SQLite database, and cross-platform support.

## ✨ Features

- ✅ **Full-stack authentication** - Login, Signup, Refresh tokens
- ✅ **JWT-based security** - Access tokens (15 min) + Refresh tokens (7 days)
- ✅ **SQLite database** - Serverless, persistent storage with EF Core
- ✅ **PBKDF2 password hashing** - 100K iterations with SHA256
- ✅ **Cross-platform mobile app** - iOS, Android, macOS, Windows
- ✅ **Secure token storage** - Keychain (iOS), Keystore (Android)
- ✅ **MVVM architecture** - Clean separation of concerns
- ✅ **Entity Framework Core** - Code-first migrations
- ✅ **Swagger API docs** - Interactive API documentation

## 🏗️ Architecture

```
MAUI Mobile App (Frontend)
    ↓ HTTP/JSON
.NET 9 Web API (Backend)
    ↓ EF Core
SQLite Database
```

## 🛠️ Tech Stack

### Backend
- .NET 9 Web API
- Entity Framework Core 9.0
- SQLite Database
- JWT Bearer Authentication
- Swagger/OpenAPI

### Frontend
- .NET 9 MAUI
- XAML
- MVVM Pattern
- HttpClient
- Platform-specific Secure Storage

## 📋 Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Visual Studio 2022 or VS Code with C# Dev Kit
- iOS Simulator (macOS) or Android Emulator

## 🚀 Quick Start

### 1. Clone the repository

```bash
git clone https://github.com/Mahesh199811/ArunayanDairy.git
cd ArunayanDairy
```

### 2. Start the Backend API

```bash
cd ArunayanDairy.API
dotnet restore
dotnet ef database update
dotnet run --urls "http://localhost:5001"
```


API will be available at: http://localhost:5001/swagger

### 3. Run the MAUI App

```bash
cd ../ArunayanDairy
dotnet restore

# For iOS Simulator
dotnet build -t:Run -f net9.0-ios

# For Android Emulator
dotnet build -t:Run -f net9.0-android
```

## 🔑 Default Credentials

- **Email**: admin@arunayan.com
- **Password**: admin123
- **Role**: admin

## 📱 Supported Platforms

- ✅ iOS (Simulator & Device)
- ✅ Android (Emulator & Device)
- ✅ macOS
- ✅ Windows

## 🗂️ Project Structure

```
ArunayanDairy/
├── ArunayanDairy.API/          # Backend Web API
│   ├── Controllers/            # REST endpoints
│   ├── Services/               # Business logic
│   ├── Security/               # JWT & password hashing
│   ├── Data/                   # EF Core DbContext
│   └── Models/                 # DTOs & entities
├── ArunayanDairy/              # MAUI Frontend
│   ├── Views/                  # XAML pages
│   ├── ViewModels/             # MVVM logic
│   ├── Services/               # HTTP client
│   ├── Models/                 # Request/Response models
│   └── Helpers/                # Secure storage
└── docs/                       # Documentation
    └── COMPLETE_IMPLEMENTATION_GUIDE.md
```

## 📚 API Endpoints

### Authentication

- `POST /api/auth/login` - Login with email/password
- `POST /api/auth/signup` - Create new account
- `POST /api/auth/refresh` - Refresh access token

## 🔒 Security Features

- **Password Hashing**: PBKDF2-SHA256 with 100,000 iterations
- **JWT Tokens**: HS256 algorithm
- **Secure Storage**: Platform-specific (Keychain/Keystore)
- **Token Refresh**: Automatic token renewal
- **Role-based Access**: User and Admin roles

## 📖 Documentation

Complete implementation guide available at: [docs/COMPLETE_IMPLEMENTATION_GUIDE.md](docs/COMPLETE_IMPLEMENTATION_GUIDE.md)

Includes:
- Detailed architecture diagrams
- Step-by-step setup instructions
- API reference
- Testing guide
- Troubleshooting

## 🧪 Testing

### Test Login (Swagger)
1. Open http://localhost:5001/swagger
2. Use POST /api/auth/login with admin credentials
3. Copy the access token

### Test MAUI App
1. Run the app on iOS/Android
2. Login with default credentials
3. Verify navigation to MainPage

## 🐛 Troubleshooting

### Port 5000 Conflict (macOS)
macOS AirPlay uses port 5000. The API is configured to use port 5001 instead.

### iOS Keychain Error
The app automatically falls back to Preferences if Keychain fails in iOS Simulator.

### Android Emulator Connection
Use `http://10.0.2.2:5001` instead of `localhost:5001` for Android Emulator.

See [COMPLETE_IMPLEMENTATION_GUIDE.md](docs/COMPLETE_IMPLEMENTATION_GUIDE.md) for more solutions.

## 🗺️ Roadmap

### Phase 2: Products Management
- Product CRUD operations
- Image upload
- Category management

### Phase 3: Orders Management
- Order creation and tracking
- Order history
- Status updates

### Phase 4: Advanced Features
- Push notifications
- Offline support
- Payment integration
- Analytics dashboard

## 👤 Author

**Mahesh Gadhave**
- GitHub: [@Mahesh199811](https://github.com/Mahesh199811)

## 📝 License

This project is available for educational purposes.

## 🙏 Acknowledgments

Built with:
- .NET 9 & .NET MAUI
- Entity Framework Core
- SQLite
- JWT Bearer Authentication

---

**⭐ If you find this project helpful, please give it a star!**

## 📄 License

© 2025 Arunayan Dairy. All rights reserved.

## 📞 Support

**Documentation:** See `/docs` folder  
**Quick Help:** [QUICK_REFERENCE.md](docs/QUICK_REFERENCE.md)  
**Testing Guide:** [END_TO_END_TESTING.md](docs/END_TO_END_TESTING.md)  

---

**Status:** ✅ Phase 1 Complete - Ready for Production Testing  
**Last Updated:** December 21, 2025  
**Version:** 1.0.0
