# 🚀 Deployment Guide - DigitalOcean App Platform

This guide explains how to deploy the Mess Management System to DigitalOcean App Platform using Docker with SQLite (no external database required).

## 📋 Prerequisites

- A DigitalOcean account
- GitHub repository connected to DigitalOcean
- Docker (optional, for local testing)

## 🏗️ Project Structure

```
mess_management/
├── Dockerfile           # Docker configuration
├── .dockerignore        # Files to exclude from Docker build
├── app.yaml             # DigitalOcean App Platform spec
├── Program.cs           # Application entry point with SQLite setup
├── Controllers/
│   └── HealthController.cs  # Health check endpoint
└── Data/
    └── mess.db          # SQLite database (created at runtime)
```

## 👥 Seed Data (Pakistani Names)

The application automatically seeds the following users on first run:

| Email | Name | Password | Role |
|-------|------|----------|------|
| admin@mess.pk | Admin User | Admin@123 | Admin |
| hamna@mess.pk | Hamna Ahmed | Password123! | Teacher |
| hanan@mess.pk | Hanan Khan | Password123! | Teacher |
| tayyab@mess.pk | Tayyab Ali | Password123! | Teacher |
| danish@mess.pk | Danish Malik | Password123! | Teacher |
| sufwan@mess.pk | Sufwan Wahla | Password123! | Teacher |

## 🔧 Deployment Steps

### Option 1: Deploy via DigitalOcean Dashboard

1. **Push your code to GitHub**
   ```bash
   git add .
   git commit -m "Add Docker and SQLite support for DigitalOcean"
   git push origin main
   ```

2. **Create App in DigitalOcean**
   - Go to [DigitalOcean Apps](https://cloud.digitalocean.com/apps)
   - Click "Create App"
   - Select your GitHub repository
   - Choose the branch (main)

3. **Configure Build Settings**
   - Build Method: **Dockerfile**
   - Dockerfile Path: `Dockerfile`

4. **Configure Run Command**
   - Port: `8080`
   
5. **Environment Variables** (optional)
   - `ASPNETCORE_ENVIRONMENT`: `Production`

6. **Deploy!**

### Option 2: Deploy via doctl CLI

1. **Install doctl**
   ```bash
   # Windows (using scoop)
   scoop install doctl
   
   # Or download from: https://docs.digitalocean.com/reference/doctl/how-to/install/
   ```

2. **Authenticate**
   ```bash
   doctl auth init
   ```

3. **Create App from spec**
   ```bash
   doctl apps create --spec app.yaml
   ```

## 🧪 Testing Locally with Docker

```bash
# Build the image
docker build -t mess-management .

# Run the container
docker run -p 8080:8080 -e ASPNETCORE_ENVIRONMENT=Production mess-management

# Access the app
# Open http://localhost:8080 in your browser
```

## ✅ Health Check

The application exposes health check endpoints:

- **Basic**: `GET /health`
  ```json
  {
    "status": "healthy",
    "timestamp": "2026-01-06T16:30:00Z",
    "database": "connected",
    "version": "1.0.0"
  }
  ```

- **Detailed**: `GET /health/detailed`
  ```json
  {
    "status": "healthy",
    "timestamp": "2026-01-06T16:30:00Z",
    "database": {
      "connected": true,
      "users": 6,
      "admins": 1,
      "teachers": 5
    },
    "environment": "Production",
    "version": "1.0.0"
  }
  ```

## 📊 Monitoring Logs

To view application logs in DigitalOcean:

```bash
doctl apps logs <app-id> --follow
```

Or view in the dashboard under your app's "Runtime Logs" tab.

## ⚠️ Important Notes

### SQLite in Containers

- SQLite database is stored in `/app/Data/mess.db`
- **Data Persistence**: In DigitalOcean App Platform, container filesystem is ephemeral
- For production with persistent data, consider:
  - Using DigitalOcean Managed Databases
  - Or mounting a volume for the Data directory

### Container Exit Codes

The application handles errors gracefully:
- Database initialization errors are logged but won't crash the app
- Health check endpoint returns 503 if database is unavailable
- All startup phases are logged with emojis for easy debugging

## 🔄 Updating the App

Push changes to your GitHub repository and DigitalOcean will automatically rebuild and deploy (if auto-deploy is enabled).

```bash
git add .
git commit -m "Your changes"
git push origin main
```

## 🆘 Troubleshooting

### App won't start

1. Check the build logs in DigitalOcean dashboard
2. Verify Dockerfile is at the root
3. Check the runtime logs for startup errors

### Database errors

1. Check if `/app/Data` directory exists (should be created by Dockerfile)
2. Verify SQLite package is correctly installed
3. Check health endpoint: `/health/detailed`

### Port issues

- Ensure `ASPNETCORE_URLS` is set to `http://+:8080`
- DigitalOcean expects port 8080 by default

---

**Deployed with ❤️ for Pakistani educators**
