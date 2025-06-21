# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

#### Testing Infrastructure

- Implemented central package management for all test projects
- Updated all test projects to use centralized package versions
- Added missing package references to Directory.Packages.props
- Standardized test project configurations
- Added common package versions for ASP.NET Core, EF Core, and testing libraries
- Added comprehensive test structure following DDD and Vertical Slice patterns
- Created test projects for unit, integration, and E2E testing
- Added shared test common and test fixtures projects
- Implemented TokenService unit tests with full coverage
- Added xUnit test projects with Moq and FluentAssertions
- Set up test coverage with coverlet
- Added Bogus for test data generation
- Set up test database fixtures for PostgreSQL integration testing
- Added PostgreSQL test container support

### Changed

#### Package Management

- Migrated all test projects to use central package management
- Updated Microsoft.AspNetCore.Identity to version 9.0.6
- Removed explicit package versions from individual project files
- Standardized package references across all test projects
- Updated project dependencies to latest stable versions

#### Test Infrastructure

- Added common test project properties and configurations
- Standardized using directives across test projects
- Improved test project structure and organization
- Added common package references to TestCommon project
- Enhanced test coverage reporting

#### Database

- Migrated integration tests from SQL Server LocalDB to PostgreSQL
- Updated connection strings to use PostgreSQL
- Modified TestDatabaseFixture to use NpgsqlConnection and UseNpgsql
- Updated Respawn configuration for PostgreSQL compatibility
- Removed SQL Server specific packages (Testcontainers.MsSql, Microsoft.EntityFrameworkCore.SqlServer)

#### Documentation

- Updated ROADMAP.md with testing strategy and progress
- Added detailed README.md in tests directory
- Improved code documentation in test projects

### Fixed

- Resolved package version conflicts in test projects
- Fixed build issues related to missing package references
- Addressed security vulnerabilities by updating package versions
- Fixed test project references and dependencies
- Fixed potential null reference issues in domain models
- Addressed code analysis warnings in test projects

## [0.1.0] - 2025-06-14

### Added
- Initial project setup
- Basic domain models and infrastructure
- Repository implementations
- Domain event infrastructure
