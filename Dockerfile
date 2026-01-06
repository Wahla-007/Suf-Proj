# ============================================
# Dockerfile for Mess Management System
# Optimized for DigitalOcean App Platform
# Uses SQLite (no external database required)
# ============================================

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["mess_management.csproj", "./"]
RUN dotnet restore "mess_management.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "mess_management.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "mess_management.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create Data directory for SQLite database
RUN mkdir -p /app/Data && chmod 755 /app/Data

# Copy published app
COPY --from=publish /app/publish .

# Create a non-root user for security
RUN adduser --disabled-password --gecos "" appuser && \
    chown -R appuser:appuser /app
USER appuser

# Expose port 8080 (DigitalOcean App Platform default)
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Start the application
ENTRYPOINT ["dotnet", "mess_management.dll"]
