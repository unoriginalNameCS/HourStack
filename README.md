# HourStack

HourStack is a simple full-stack application to track overtime hours for my partner using:

- Backend: .NET (located in backend/)
- Frontend: React (Vite, located in frontend/)
- Database: PostgreSQL from Supabase (interacted with Entity Framework Core)
- Deployment: Azure, F1 Tier (so it will cold start and take 20-30 seconds to re-start after 20 mins of inactivity).

## Getting Started

### Backend

1. Navigate to the backend folder:
   ```bash
   cd backend
   ```
2. Run the backend server:
   ```bash
   dotnet run
   ```

### Frontend

1. Navigate to the frontend folder:
   ```bash
   cd frontend
   ```
2. Install dependencies:
   ```bash
   npm install
   ```
3. Start the development server:
   ```bash
   npm run dev
   ```

## Project Structure

```
hourstack/
├── backend/
│   ├── Data/
│   ├── Migrations/
│   ├── Models/
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── backend.csproj
│   ├── Dockerfile
│   ├── Program.cs
│   └── ...
├── backend.Tests/
│   ├── UnitTest1.cs
│   ├── backend.Tests.csproj
│   └── ...
├── frontend/
│   ├── src/
│   ├── public/
│   ├── index.html
│   ├── package.json
│   ├── vite.config.js
│   └── ...
├── .github/
│   └── workflows/
│       └── ci-cd.yml
└── README.md
```

## CI/CD

GitHub Actions workflow is defined in `.github/workflows/ci-cd.yml`.

## Deployment

The app is available at:
https://gray-river-014108700.4.azurestaticapps.net
