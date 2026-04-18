# HotelBooking Project Command Reference

## Project Management Commands

### Build Commands
```bash
# Build the entire solution
dotnet build

# Build in Release mode
dotnet build -c Release

# Build without restoring packages
dotnet build --no-restore
```

### Run Commands
```bash
# Run the main application
dotnet run --project HotelBooking

# Run in Development mode
dotnet run --project HotelBooking --environment Development

# Run with specific URL
dotnet run --project HotelBooking --urls "http://localhost:5000"
```

### Clean Commands
```bash
# Clean build outputs
dotnet clean

# Clean and rebuild
dotnet clean && dotnet build
```

## Database Commands

### Migration Commands
```bash
# Add new migration
dotnet ef migrations add <MigrationName> --project HotelBooking --startup-project HotelBooking

# Remove last migration
dotnet ef migrations remove --project HotelBooking --startup-project HotelBooking

# Update database to latest migration
dotnet ef database update --project HotelBooking --startup-project HotelBooking

# Update to specific migration
dotnet ef database update <MigrationName> --project HotelBooking --startup-project HotelBooking

# Generate SQL script for migration
dotnet ef migrations script --project HotelBooking --startup-project HotelBooking
```

### Database Operations
```bash
# Drop database
dotnet ef database drop --project HotelBooking --startup-project HotelBooking

# Create database
dotnet ef database update 0 --project HotelBooking --startup-project HotelBooking && dotnet ef database update --project HotelBooking --startup-project HotelBooking
```

## Testing Commands

### Unit Tests
```bash
# Run all unit tests
dotnet test HotelBooking.Tests/HotelBooking.Tests.csproj

# Run with coverage
dotnet test HotelBooking.Tests/HotelBooking.Tests.csproj --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test HotelBooking.Tests/HotelBooking.Tests.csproj --filter "ClassName~<ClassName>"

# Run specific test method
dotnet test HotelBooking.Tests/HotelBooking.Tests.csproj --filter "FullyQualifiedName~<Namespace.ClassName.MethodName>"

# Run tests in watch mode
dotnet watch --project HotelBooking.Tests test
```

### Integration Tests
```bash
# Run integration tests
dotnet test HotelBooking.Tests/HotelBooking.Tests.csproj --filter "Category=Integration"

# Run database tests
dotnet test HotelBooking.Tests/HotelBooking.Tests.csproj --filter "Category=Database"
```

### Performance Tests (k6)
```bash
# Install k6 (if not already installed)
npm install -g k6

# Run performance tests
k6 run HotelBooking.Tests/Performance/stress.js

# Run with custom base URL
k6 run HotelBooking.Tests/Performance/stress.js --env BASE_URL=http://localhost:5000

# Run with different VU settings
k6 run HotelBooking.Tests/Performance/stress.js --vus 10 --duration 30s

# Run with thresholds only (no output)
k6 run HotelBooking.Tests/Performance/stress.js --no-thresholds
```

## Development Commands

### Code Quality
```bash
# Format code
dotnet format

# Analyze code
dotnet analyze

# List package updates
dotnet list package --outdated
```

### Package Management
```bash
# Restore packages
dotnet restore

# Add new package
dotnet add package <PackageName> --project HotelBooking

# Remove package
dotnet remove package <PackageName> --project HotelBooking
```

## Docker Commands (if using Docker)
```bash
# Build Docker image
docker build -t hotelbooking .

# Run Docker container
docker run -d -p 5000:80 --name hotelbooking hotelbooking

# Stop and remove container
docker stop hotelbooking && docker rm hotelbooking
```

## Database Management (PostgreSQL)

### Direct Database Access
```bash
# Connect to PostgreSQL database
psql -h localhost -p 5432 -U postgres -d HotelBookingDb

# Backup database
pg_dump -h localhost -p 5432 -U postgres HotelBookingDb > backup.sql

# Restore database
psql -h localhost -p 5432 -U postgres HotelBookingDb < backup.sql
```

## GitHub Actions (CI/CD)
```bash
# Run GitHub Actions locally (if act is installed)
act -j build
act -j test
act -j performance
```

## Common Development Workflow
```bash
# Full development cycle
dotnet restore
dotnet build
dotnet ef database update
dotnet run --project HotelBooking
```

## Testing Workflow
```bash
# Run all tests
dotnet test HotelBooking.Tests/HotelBooking.Tests.csproj

# Run with coverage and watch
dotnet watch --project HotelBooking.Tests test --collect:"XPlat Code Coverage"
```

## Performance Testing Workflow
```bash
# Start application
dotnet run --project HotelBooking

# Run performance tests
k6 run HotelBooking.Tests/Performance/stress.js
```

## Database Migration Workflow
```bash
# Make changes to models
# Add migration
dotnet ef migrations add <MigrationName> --project HotelBooking --startup-project HotelBooking
# Update database
dotnet ef database update --project HotelBooking --startup-project HotelBooking
```

## Environment Variables
```bash
# Set environment variables for development
set ASPNETCORE_ENVIRONMENT=Development
set ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=HotelBookingDb;Username=postgres;Password=postgres_password"
```

## Useful URLs
- Swagger UI: http://localhost:5000/swagger
- API Root: http://localhost:5000/api
- Health Check: http://localhost:5000/health