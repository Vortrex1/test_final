Steps

Set up new ASP.NET Core Web API project with EF Core, PostgreSQL, and test packages (xUnit, AutoFixture, Testcontainers, k6).

Define entities (Room, Guest, Reservation) with EF Core DbContext and migrations.

Implement repositories and services for CRUD operations and business logic (date overlap checks, pricing calculations).

Create API controllers for rooms and reservations endpoints with filters and validations.

Write unit tests for services (date overlaps, pricing) using xUnit and AutoFixture.

Develop integration tests with WebApplicationFactory for full booking flow and availability checks.

Add database tests with Testcontainers for constraints and history tracking.

Create performance tests with k6 for load and stress scenarios.

Seed database with 10,000+ realistic records using AutoFixture/Bogus. ✅

Configure GitHub Actions CI pipeline for automated testing on push/PR. ✅

Create public GitHub repo, push code, and submit PRs for each major feature.