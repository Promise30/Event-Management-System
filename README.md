# Event-Management-System

A .NET 9.0 API service-based Event Management System using a University case study.

# Case Study
In a University setting, there are typically centers that can be booked for various events such as seminars, workshops, and social gatherings. 
The centers are often used by students, faculty, and external organizations for hosting events.
These event centers have different capacities, facilities, and availability schedules.
However, despite the variety of event centers, there is often a lack of a centralized system to manage bookings, track availability, and handle event details efficiently.
This project aims to develop an Event Management System that addresses these challenges by providing a backend solution to manage event centers, bookings, events and tickets effectively.
The project supports a robust and rich features which includes:
- ** Event Center Management**: CRUD operations for event center management
- ** Booking System**: Allows users to book event centers based on availability
- ** Event Management**: Create and manage events tied to bookings
- ** Ticketing System**: Issue and manage tickets for events
- ** User Roles**: Different roles for organizers and attendees with specific permissions
- ** Availability Tracking**: Monitor and update the availability of event centers
- ** Authentication & Authorization**: Secure access to the system based on user roles
- ** Notifications**: Send notifications for booking confirmations and event updates
- ** Reporting**: Generate reports on bookings, events, and attendance
- ** Integration**: Potential integration with calendar systems and payment gateways
- ** Scalability**: Designed to handle multiple users and events simultaneously
- ** API Documentation**: Comprehensive documentation for easy integration with front-end applications
- ** Testing**: Unit and integration tests to ensure system reliability
- ** CI/CD Pipeline**: Automated build, test, and deployment processes



## Getting Started

This repository is ready for .NET 9.0 development. When you add .NET projects:

1. The CI pipeline will automatically detect and build them
2. Test projects will be discovered and executed
3. Code quality checks will run on your codebase

### Prerequisites

- .NET 9.0 SDK
- Git
- GitHub account with appropriate permissions

### Local Development

```bash
# Clone the repository
git clone https://github.com/Promise30/Event-Management-System.git

# Navigate to project directory
cd Event-Management-System

# When .NET projects are added, you can:
# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run tests
dotnet test
```

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

Please ensure your PR includes:
- Clear description of changes
- Updated tests (if applicable)
- Passing CI checks
- Proper code review

## Documentation

- [Branch Protection Setup](.github/BRANCH_PROTECTION.md)
- [Pull Request Template](.github/pull_request_template.md)
- [CI/CD Workflow](.github/workflows/ci.yml)
