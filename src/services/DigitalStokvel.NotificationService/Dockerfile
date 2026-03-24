# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# Copy solution and project files
COPY ["DigitalStokvel.slnx", "./"]
COPY ["Directory.Build.props", "./"]
COPY ["global.json", "./"]

# Copy shared projects
COPY ["src/shared/DigitalStokvel.Common/DigitalStokvel.Common.csproj", "src/shared/DigitalStokvel.Common/"]
COPY ["src/shared/DigitalStokvel.Domain/DigitalStokvel.Domain.csproj", "src/shared/DigitalStokvel.Domain/"]
COPY ["src/shared/DigitalStokvel.Infrastructure/DigitalStokvel.Infrastructure.csproj", "src/shared/DigitalStokvel.Infrastructure/"]

# Copy Notification Service project
COPY ["src/services/DigitalStokvel.NotificationService/DigitalStokvel.NotificationService.csproj", "src/services/DigitalStokvel.NotificationService/"]

# Restore dependencies
RUN dotnet restore "src/services/DigitalStokvel.NotificationService/DigitalStokvel.NotificationService.csproj"

# Copy source code
COPY ["src/shared/", "src/shared/"]
COPY ["src/services/DigitalStokvel.NotificationService/", "src/services/DigitalStokvel.NotificationService/"]

# Build and publish
WORKDIR "/src/src/services/DigitalStokvel.NotificationService"
RUN dotnet publish "DigitalStokvel.NotificationService.csproj" -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app

# Create non-root user
RUN addgroup -g 1000 appuser && adduser -u 1000 -G appuser -s /bin/sh -D appuser

# Copy published application
COPY --from=build /app/publish .

# Set ownership
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
  CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

ENTRYPOINT ["dotnet", "DigitalStokvel.NotificationService.dll"]
