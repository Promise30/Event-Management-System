# Eventify — Event Management System API

<p align="center">
  <img src="https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET 9.0"/>
  <img src="https://img.shields.io/badge/ASP.NET_Core-REST_API-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt="ASP.NET Core"/>
  <img src="https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white" alt="SQL Server"/>
  <img src="https://img.shields.io/badge/Azure-App_Service-0078D4?style=for-the-badge&logo=microsoftazure&logoColor=white" alt="Azure"/>
  <img src="https://img.shields.io/badge/JWT-Authentication-000000?style=for-the-badge&logo=jsonwebtokens&logoColor=white" alt="JWT"/>
  <img src="https://img.shields.io/badge/Redis-Caching-DC382D?style=for-the-badge&logo=redis&logoColor=white" alt="Redis"/>
</p>

> **🚀 Live API:** [eventify-api-a8gxeda3cfebexfk.uksouth-01.azurewebsites.net](https://eventify-api-a8gxeda3cfebexfk.uksouth-01.azurewebsites.net)

A production-ready, feature-rich **RESTful API** built with **.NET 9.0 and ASP.NET Core** for managing university event centers, bookings, events, and ticketing. The system serves as a centralized backend solution that streamlines how event centers are discovered, booked, and managed — complete with payment processing, analytics, background job scheduling, and a full authentication system.

---

## 📚 Table of Contents

- [Case Study & Motivation](#-case-study--motivation)
- [Features](#-features)
- [Tech Stack](#-tech-stack)
- [Project Architecture](#-project-architecture)
- [API Endpoints](#-api-endpoints)
- [Database Models](#-database-models)
- [Getting Started](#-getting-started)
- [Configuration & Environment Variables](#-configuration--environment-variables)
- [Running Tests](#-running-tests)
- [CI/CD Pipeline](#-cicd-pipeline)
- [Deployment (Azure)](#-deployment-azure)
- [Health Monitoring](#-health-monitoring)
- [Contributing](#-contributing)

---

## 🏫 Case Study & Motivation

In a university setting, event centers are used daily by students, faculty, and external organizations for seminars, workshops, social gatherings, and conferences. Despite the variety of centers — each with different capacities, facilities, and schedules — there is typically no centralized system to:

- Manage center bookings and check real-time availability
- Handle event creation tied to approved bookings
- Issue and track tickets for attendees
- Process payments for center hire or event tickets
- Provide analytics and reporting for organizers and administrators

**Eventify** solves all of these challenges by providing a robust, scalable backend API that any front-end application or third-party service can integrate with.

---

## ✨ Features

| Feature | Description |
|---|---|
| 🏛️ **Event Center Management** | Full CRUD for event centers including capacity, pricing, facilities, and soft-delete support |
| 📅 **Booking System** | Create and manage bookings with availability checking, status tracking, automatic expiration, and support for free/paid centers |
| 🎉 **Event Management** | Create events tied to confirmed bookings, with flyer management and attendee tracking |
| 🎟️ **Ticketing System** | Multiple ticket types per event, unique ticket numbers, status lifecycle (Available → Reserved → Sold → Cancelled), and automatic reservation expiration |
| 💳 **Payment Processing** | Paystack integration for initialising and verifying payments, transaction history, and webhook support |
| 👤 **Authentication & Authorization** | JWT bearer tokens, refresh tokens, email verification, password reset, two-factor authentication (2FA), and role-based access control |
| 🔑 **Role-Based Access Control** | Three roles — **Administrator**, **Organizer**, and **User** — each with specific permissions |
| 📊 **Analytics & Reporting** | Event, revenue, ticket, event center, and organizer analytics with period-based aggregations |
| 🔔 **Notifications** | Email notifications for booking confirmations, event updates, and account actions via MailKit/SMTP |
| ⚙️ **Background Jobs** | Hangfire-powered background services for automatic expiration of reserved tickets and bookings |
| ❤️ **Health Monitoring** | Health check endpoints and a visual Health UI dashboard |
| 📖 **API Documentation** | Interactive Swagger/OpenAPI documentation |
| 🧪 **Testing** | Unit and integration tests using NUnit and Moq |
| 🚀 **CI/CD** | GitHub Actions pipelines for automated build, test, and Azure deployment |

---

## 🛠️ Tech Stack

| Category | Technologies |
|---|---|
| **Framework** | .NET 9.0, ASP.NET Core |
| **Language** | C# |
| **Database** | SQL Server (Azure SQL), PostgreSQL (alternative) |
| **ORM** | Entity Framework Core 9.0 |
| **Authentication** | ASP.NET Core Identity, JWT Bearer Tokens |
| **Caching** | Redis (StackExchange.Redis) |
| **Payments** | Paystack (`Paystack.Net`) |
| **Background Jobs** | Hangfire (SQL Server storage) |
| **Email** | MailKit (SMTP) |
| **Logging** | Serilog (console + rolling file) |
| **API Documentation** | Swagger / Swashbuckle |
| **Object Mapping** | AutoMapper |
| **Testing** | NUnit 4, Moq |
| **Health Checks** | AspNetCore.HealthChecks.UI |
| **Deployment** | Azure App Service + Azure SQL Database |
| **CI/CD** | GitHub Actions |

---

## 🏗️ Project Architecture

The solution follows a **layered / clean-architecture** approach:

```
Event-Management-System/
├── Event_Management_System/
│   ├── Event_Management_System.API/
│   │   ├── Controllers/              # REST API controllers (auth, bookings, events, tickets, payments, analytics…)
│   │   │   └── Analytics/            # Dedicated analytics controllers
│   │   ├── Application/
│   │   │   ├── Interfaces/           # Service contracts
│   │   │   ├── Implementation/       # Service implementations (business logic)
│   │   │   ├── BackgroundServices/   # Hangfire recurring/background jobs
│   │   │   ├── Exceptions/           # Custom exception types
│   │   │   └── Payments/             # Payment provider abstractions
│   │   ├── Domain/
│   │   │   ├── Entities/             # EF Core domain entities
│   │   │   ├── DTOs/                 # Request/response data transfer objects
│   │   │   └── Enums/                # Enumeration types
│   │   ├── Infrastructures/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   └── Repositories/         # Data-access layer (repository pattern)
│   │   ├── Extensions/               # Service registration extension methods
│   │   ├── Helpers/                  # Utility/helper classes
│   │   ├── Migrations/               # EF Core database migrations
│   │   ├── Program.cs                # Application entry point
│   │   └── appsettings.json          # Application configuration
│   │
│   └── Event_Management_System.Tests/  # Unit & integration tests
│       ├── AuthServiceTests.cs
│       ├── BookingTests/
│       ├── EventCentreTests/
│       └── NotificationTests/
│
├── .github/
│   └── workflows/
│       ├── ci.yml                    # CI pipeline (build & test)
│       └── main_eventify-api.yml     # CD pipeline (deploy to Azure)
│
└── README.md
```

---

## 📡 API Endpoints

> **Base URL (Production):** `https://eventify-api-a8gxeda3cfebexfk.uksouth-01.azurewebsites.net`  
> Interactive documentation is available at: `{base-url}/swagger`

### 🔐 Authentication (`/auth`)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|:---:|
| `POST` | `/auth/register` | Register a new user account | ❌ |
| `POST` | `/auth/login` | Login and receive JWT + refresh token | ❌ |
| `POST` | `/auth/refresh-token` | Refresh an expired JWT token | ❌ |
| `GET`  | `/auth/verify-email` | Verify user email address | ❌ |
| `POST` | `/auth/forgot-password` | Request a password reset link | ❌ |
| `POST` | `/auth/reset-password` | Complete the password reset flow | ❌ |
| `POST` | `/auth/change-password` | Change password (authenticated) | ✅ |
| `POST` | `/auth/2fa/enable` | Enable two-factor authentication | ✅ |
| `POST` | `/auth/2fa/disable` | Disable two-factor authentication | ✅ |
| `POST` | `/auth/2fa/verify` | Verify a 2FA code | ✅ |
| `POST` | `/auth/users/resend-verification` | Resend email verification | ❌ |
| `GET`  | `/auth/users` | List all users | 🔒 Admin |
| `GET`  | `/auth/users/{email}` | Get user details by email | 🔒 Admin |
| `GET`  | `/auth/users/{email}/roles` | Get user roles | 🔒 Admin |
| `PUT`  | `/auth/users/update` | Update user profile | ✅ |
| `DELETE` | `/auth/users/{email}` | Delete a user | 🔒 Admin |

### 🏛️ Event Centers (`/event-centers`)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|:---:|
| `GET` | `/event-centers/get-all` | List all event centers (paginated) | ❌ |
| `GET` | `/event-centers/get-event-center` | Get a specific event center by ID | ❌ |
| `GET` | `/event-centers/get-available-centers` | Get centers available for a date range | ❌ |
| `POST` | `/event-centers/create-event-center` | Create a new event center | 🔒 Admin |
| `PUT` | `/event-centers/update-event-center` | Update an event center | 🔒 Admin |
| `PUT` | `/event-centers/reactivate-event-center` | Reactivate a deactivated center | 🔒 Admin |
| `DELETE` | `/event-centers/delete-event-center` | Soft-delete an event center | 🔒 Admin |
| `POST` | `/event-centers/add-event-center-availability` | Add an availability schedule | 🔒 Admin |
| `PUT` | `/event-centers/update-event-center-availability` | Update availability schedule | 🔒 Admin |
| `DELETE` | `/event-centers/delete-event-center-availability` | Delete an availability schedule | 🔒 Admin |

### 📅 Bookings (`/bookings`)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|:---:|
| `GET` | `/bookings/get-all` | List all bookings with filters (paginated) | ✅ |
| `GET` | `/bookings/get-by-id` | Get booking details by ID | ✅ |
| `POST` | `/bookings/create-booking` | Create a new booking | ✅ Organizer |
| `PUT` | `/bookings/update-booking-status` | Update booking status | 🔒 Admin |
| `DELETE` | `/bookings/delete-booking` | Cancel a booking | ✅ |

### 🎉 Events (`/events`)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|:---:|
| `GET` | `/events/get-all` | List all events (paginated) | ❌ |
| `GET` | `/events/get-by-id` | Get event details by ID | ❌ |
| `POST` | `/events/create-event` | Create an event for a confirmed booking | ✅ Organizer |
| `PUT` | `/events/update-event` | Update event details | ✅ Organizer |
| `DELETE` | `/events/delete-event` | Delete an event | ✅ Organizer |

### 🎟️ Tickets (`/tickets`)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|:---:|
| `GET` | `/tickets/get-all` | List all tickets (paginated, searchable, sortable) | 🔒 Admin |
| `GET` | `/tickets/get-by-id` | Get ticket details by ID | ✅ |
| `GET` | `/tickets/get-by-attendee` | Get all tickets for the current attendee | ✅ |
| `POST` | `/tickets/create-ticket` | Purchase / reserve a ticket | ✅ |
| `DELETE` | `/tickets/cancel-ticket` | Cancel a ticket | ✅ |

### 🏷️ Ticket Types (`/ticket-types`)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|:---:|
| `GET` | `/ticket-types/get-ticket-types` | List all ticket types | ❌ |
| `GET` | `/ticket-types/get-ticket-type-by-id` | Get ticket type by ID | ❌ |
| `GET` | `/ticket-types/by-type` | Get by type (Free / Paid) | ❌ |
| `POST` | `/ticket-types/create-ticket-type` | Create a ticket type | ✅ Organizer |
| `PUT` | `/ticket-types/update-ticket-type` | Update a ticket type | ✅ Organizer |
| `DELETE` | `/ticket-types/delete-ticket-type` | Delete a ticket type | ✅ Organizer |

### 💳 Payments (`/api/payments`)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|:---:|
| `POST` | `/api/payments/initialize` | Initialize a Paystack payment | ✅ |
| `GET` | `/api/payments/verify` | Verify payment after redirect (callback) | ❌ |
| `GET` | `/api/payments/{paymentId}` | Get payment record by ID | ✅ |

### 🙋 Organizer Requests (`/api/organizer-requests`)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|:---:|
| `POST` | `/api/organizer-requests` | Submit an organizer elevation request | ✅ |
| `GET` | `/api/organizer-requests` | List all organizer requests | 🔒 Admin |
| `PUT` | `/api/organizer-requests/{id}/approve` | Approve an organizer request | 🔒 Admin |
| `PUT` | `/api/organizer-requests/{id}/reject` | Reject an organizer request | 🔒 Admin |

### 📊 Analytics (`/api/analytics`)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|:---:|
| `GET` | `/api/analytics/events/popular` | Most popular events | ✅ |
| `GET` | `/api/analytics/events/upcoming` | Upcoming events with low availability | ✅ |
| `GET` | `/api/analytics/revenue/summary` | Total revenue summary | 🔒 Admin |
| `GET` | `/api/analytics/revenue/by-period` | Revenue breakdown by time period | 🔒 Admin |
| `GET` | `/api/analytics/revenue/by-event` | Revenue per event | ✅ Organizer |
| `GET` | `/api/analytics/tickets/sold` | Number of tickets sold | ✅ |
| `GET` | `/api/analytics/tickets/revenue` | Revenue from ticket sales | ✅ Organizer |
| `GET` | `/api/analytics/event-centers/utilization` | Event center utilization rates | 🔒 Admin |
| `GET` | `/api/analytics/event-centers/availability` | Center availability trends | ❌ |
| `GET` | `/api/analytics/organizers/events-hosted` | Events hosted per organizer | ✅ Organizer |
| `GET` | `/api/analytics/organizers/bookings/summary` | Organizer booking summary | ✅ Organizer |
| `GET` | `/api/analytics/organizers/bookings/conversion-rate` | Booking-to-event conversion rate | ✅ Organizer |
| `GET` | `/api/analytics/organizers/bookings/by-event` | Bookings grouped by event | ✅ Organizer |

---

## 🗄️ Database Models

The system is composed of the following core domain entities:

```
ApplicationUser          — Extends ASP.NET Core Identity user with profile fields
  │
  ├─── Booking ─────────── EventCentre
  │       │
  │       └─── Event
  │               │
  │               └─── TicketType ─── Ticket (purchased by ApplicationUser)
  │
  ├─── Payment
  └─── OrganizerRequest
```

| Entity | Key Fields |
|---|---|
| **ApplicationUser** | `FirstName`, `LastName`, `Email`, `PhoneNumber`, roles |
| **EventCentre** | `Name`, `Location`, `Capacity`, `PricePerDay`, `IsAvailable`, availability schedules |
| **EventCentreAvailability** | `EventCentreId`, `DayOfWeek`, `StartTime`, `EndTime`, `IsAvailable` |
| **Booking** | `EventCentreId`, `OrganizerId`, `StartDate`, `EndDate`, `Status`, `TotalPrice` |
| **Event** | `BookingId`, `Title`, `Description`, `StartTime`, `EndTime`, `FlyerUrl`, attendee count |
| **TicketType** | `EventId`, `Name`, `Price`, `Quantity`, `Type` (Free / Paid) |
| **Ticket** | `EventId`, `TicketTypeId`, `AttendeeId`, `TicketNumber`, `Status`, `ReservationExpiresAt` |
| **Payment** | `UserId`, `Amount`, `TransactionReference`, `PaymentType`, `PaymentStatus`, `PaymentProvider` |
| **OrganizerRequest** | `UserId`, `Status` (Pending / Approved / Rejected), `RequestDate`, `ReviewedBy` |
| **AuditLog** | `EntityType`, `EntityId`, `ActionType`, `UserId`, `Changes`, `Timestamp` |

**Entity Relationships:**
- An `EventCentre` can have many `Booking`s and many availability schedules
- A `Booking` has exactly one `Event`
- An `Event` can have many `TicketType`s; each `TicketType` can have many `Ticket`s
- A `Ticket` belongs to one `ApplicationUser` (attendee)
- `Payment`s are linked to a user and reference either a booking or a ticket

---

## 🚀 Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [SQL Server](https://www.microsoft.com/en-gb/sql-server/sql-server-downloads) (or Azure SQL)
- [Redis](https://redis.io/downloads/) (optional — used for caching)
- [Git](https://git-scm.com/)

### Local Setup

```bash
# 1. Clone the repository
git clone https://github.com/Promise30/Event-Management-System.git
cd Event-Management-System

# 2. Navigate to the API project
cd Event_Management_System/Event_Management_System.API

# 3. Create a local configuration file and populate it with your values (see section below)
# Linux/macOS:
cp appsettings.json appsettings.Development.json
# Windows (Command Prompt):
# copy appsettings.json appsettings.Development.json

# 4. Restore NuGet dependencies
dotnet restore

# 5. Apply database migrations
dotnet ef database update

# 6. Run the API
dotnet run
```

Once running, the API will be available at:
- **API Base:** `https://localhost:7004`
- **Swagger UI:** `https://localhost:7004/swagger`
- **Health UI:** `https://localhost:7004/health-ui`
- **Hangfire Dashboard:** `https://localhost:7004/hangfire`

---

## ⚙️ Configuration & Environment Variables

The application is configured via `appsettings.json` (with environment-specific overrides). The following sections are required:

```jsonc
{
  "ConnectionStrings": {
    // SQL Server connection string (Azure SQL or local)
    "DefaultConnection": "Server=<server>;Database=<db>;User Id=<user>;Password=<password>;"
  },

  "JwtSettings": {
    "ValidIssuer": "EventManagementSystem",
    "ValidAudience": "EventManagementSystemUsers",
    // A Base64-encoded secret key (min. 32 bytes recommended)
    "SecretKey": "<your-base64-secret>",
    // Token lifetime in seconds
    "Expires": 3600
  },

  "EmailSettings": {
    "DefaultFromEmail": "noreply@yourdomain.com",
    "DefaultFromName": "Eventify",
    "SMTPSetting": {
      "Host": "smtp.gmail.com"
    },
    "Port": 587,
    "UserName": "<smtp-username>",
    "Password": "<smtp-app-password>"
  },

  "CacheSettings": {
    // Redis connection details (optional — can be omitted for local development without caching)
    "Host": "localhost",
    "Port": 6379,
    "Password": ""
  },

  "PayStack": {
    // Your Paystack secret key (from the Paystack dashboard)
    "SecretKey": "<paystack-secret-key>"
  },

  "ServiceName": "EventManagementSystemAPI",

  "HealthChecksUI": {
    "HealthChecks": [
      {
        "Name": "Event_Management_System_API",
        "Uri": "/health"
      }
    ],
    "EvaluationTimeInSeconds": 5
  }
}
```

> ⚠️ **Never commit real secrets to source control.** Use [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) for local development or Azure App Service **Application Settings** / **Key Vault** references for production.

---

## 🧪 Running Tests

The test project uses **NUnit** as the test framework and **Moq** for mocking, with an in-memory EF Core database for integration-style tests.

```bash
# Run all tests from the solution root
dotnet test

# Run only the test project
dotnet test Event_Management_System/Event_Management_System.Tests/Event_Management_System.Tests.csproj

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run with code coverage (requires coverlet)
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover
```

**Test coverage areas:**

| Test Suite | Description |
|---|---|
| `AuthServiceTests` | User registration, login, token refresh, password reset |
| `BookingTests` | Booking creation, availability checks, status transitions |
| `EventCentreTests` | Event center CRUD, availability management |
| `NotificationTests` | Email notification dispatch |

---

## 🔄 CI/CD Pipeline

The repository uses **GitHub Actions** for automated build, test, and deployment.

### CI Pipeline (`ci.yml`)

Triggers on:
- Push to `develop`, `feature/*`, `hotfix/*` branches
- Pull requests targeting `main` or `develop`

Steps:
1. Checkout code
2. Setup .NET 9.0
3. Restore NuGet packages
4. Build solution (Release configuration)
5. Run all NUnit tests
6. Upload test results (`.trx` files) as artifacts

### CD Pipeline (`main_eventify-api.yml`)

Triggers on:
- Push to the `main` branch

Steps:
1. Build and publish application (`Release` configuration)
2. Upload build artifact
3. Authenticate with Azure (using OIDC / federated credentials)
4. Deploy to **Azure App Service** (`eventify-api`)

---

## ☁️ Deployment (Azure)

The API is deployed on **Microsoft Azure** using the following services:

| Service | Purpose |
|---|---|
| **Azure App Service** | Hosts the ASP.NET Core API (Windows plan) |
| **Azure SQL Database** | Managed relational database (SQL Server) |

**Live URL:** [https://eventify-api-a8gxeda3cfebexfk.uksouth-01.azurewebsites.net](https://eventify-api-a8gxeda3cfebexfk.uksouth-01.azurewebsites.net)

**Swagger UI (Production):** [https://eventify-api-a8gxeda3cfebexfk.uksouth-01.azurewebsites.net/swagger](https://eventify-api-a8gxeda3cfebexfk.uksouth-01.azurewebsites.net/swagger)

Production configuration (connection strings, JWT secrets, Paystack keys, SMTP credentials) is managed through **Azure App Service Application Settings**, keeping all secrets out of source control.

---

## ❤️ Health Monitoring

The API exposes the following health and monitoring endpoints:

| Endpoint | Description |
|---|---|
| `GET /health` | Raw JSON health status for all registered checks |
| `GET /health-ui` | Visual health dashboard (AspNetCore.HealthChecks.UI) |
| `GET /hangfire` | Hangfire background jobs dashboard |

---

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/your-feature`)
3. Make your changes and add/update tests
4. Ensure all tests pass (`dotnet test`)
5. Commit your changes (`git commit -m 'feat: add your feature'`)
6. Push to the branch (`git push origin feature/your-feature`)
7. Open a Pull Request against the `develop` branch

Please ensure your PR:
- Has a clear description of the changes
- Includes updated/new tests where applicable
- Passes all CI checks
- Follows the existing code style and patterns

---

## 📄 Further Documentation

- [CI/CD Workflow](.github/workflows/ci.yml)
- [Azure Deployment Workflow](.github/workflows/main_eventify-api.yml)
- [Pull Request Template](.github/pull_request_template.md)
