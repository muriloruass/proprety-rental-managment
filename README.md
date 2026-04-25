# Property Rental Management

Property Rental Management is an ASP.NET Core MVC web application for managing residential rental operations. It supports role-based workflows for owners, managers, and tenants, with modules for properties, appointments, messaging, and maintenance events.

This project was developed as a final-semester academic project at Faculdade La Salle.

## Tech Stack

- ASP.NET Core 9 (MVC)
- C# 13
- Entity Framework Core 9
- SQL Server
- Razor Views + Bootstrap
- Cookie Authentication + Role-based Authorization

## Key Features

- Role-based access control for Owner, Manager, and Tenant users
- Apartment and building CRUD operations
- Apartment search and filtering (status, room count, rent range)
- Tenant appointment booking and manager assignment
- In-app messaging between users
- Event reporting for maintenance and operational tracking
- REST API endpoints under API controllers for key resources

## Getting Started

### Prerequisites

- .NET SDK 9.0+
- SQL Server (or LocalDB)
- Entity Framework Core CLI tools

Install EF CLI (if not installed yet):

```bash
dotnet tool install --global dotnet-ef
```

### Installation

1. Clone the repository.
2. Go to the project directory.
3. Configure the database connection string.
4. Apply migrations.
5. Run the app.

```bash
git clone <your-repo-url>
cd proprety-rental-managment/PropertyRentalManagement
dotnet restore
dotnet ef database update
dotnet run
```

By default, the app will start on localhost using the launch profile settings.

### Connection String

Update the DefaultConnection value in appsettings.Development.json (recommended for local setup) or appsettings.json:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PropertyRentalDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

## How To Use

### Web App Flow

1. Open the app in your browser.
2. Register as a tenant, or log in with a seeded user.
3. Navigate by role:

- Owner: manage users, monitor operations.
- Manager: manage buildings/apartments, appointments, and events.
- Tenant: browse apartments, book appointments, and send messages.

### Seeded Demo Users

The application seeds starter users automatically on first run (during startup migration):

- owner@propertyrental.local / Owner@123
- manager@propertyrental.local / Manager@123
- tenant@propertyrental.local / Tenant@123

### API Usage Example

Example request to list apartments:

```bash
curl -X GET "https://localhost:7147/api/apartmentsapi"
```

Note: Some API endpoints may require authentication/authorization depending on configuration.

## Project Structure

```text
PropertyRentalManagement/
├── Controllers/            # MVC + API controllers
├── Data/                   # EF Core DbContext
├── Migrations/             # EF Core migrations
├── Models/                 # Domain models and view models
├── Views/                  # Razor views
├── wwwroot/                # Static assets (CSS, JS, libraries)
├── Program.cs              # Application startup and middleware pipeline
├── appsettings.json        # Base configuration
└── appsettings.Development.json
```

## Development Notes

- Database migrations are applied at startup.
- Initial demo data is seeded if key records are missing.
- Build command:

```bash
dotnet build
```

## License

This project is licensed under the MIT License.

See the LICENSE file in the project root for full terms.
