# Property Rental Management System

A comprehensive ASP.NET Core MVC application designed to streamline the relationship between property owners, managers, and tenants. This system provides a robust platform for managing buildings, apartments, appointments, and communication.

## 🚀 Tech Stack

- **Framework:** ASP.NET Core 9.0 (MVC)
- **Language:** C#
- **Database:** SQL Server
- **ORM:** Entity Framework Core
- **Frontend:** HTML5, CSS3 (Bootstrap 5), JavaScript (jQuery for validation)
- **Security:** Cookie-based Authentication & Role-based Authorization (Admin, Manager, Tenant)
- **API:** RESTful Web API controllers included

## 🛠️ How to Run Locally

1. **Clone the Repository:**
   ```bash
   git clone [repository-url]
   cd PropertyRentalManagement
   ```

2. **Configure Connection String:**
   Open `appsettings.json` and update the `DefaultConnection` string to point to your local SQL Server instance.
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PropertyRentalDb;Trusted_Connection=True;MultipleActiveResultSets=true"
   }
   ```

3. **Update Database:**
   Run the following command to apply migrations and seed initial data:
   ```bash
   dotnet ef database update
   ```

4. **Run the Application:**
   ```bash
   dotnet run
   ```
   Access the app at `https://localhost:7147` (or the port specified in your output).

## 👥 User Roles & Permissions

### Admin / Property Owner
- Full system control.
- Manage (CRUD) Manager and Tenant accounts.
- View and search all user profiles.
- Full access to all property and communication data.

### Property Manager
- CRUD operations for Buildings and Apartments.
- Manage apartment status (Available, Rented, Maintenance).
- Schedule and manage tenant appointments.
- Read and reply to messages from tenants.
- Report maintenance events or issues to the owner.

### Tenant
- Self-registration and login.
- Search and browse available apartments with price and room filters.
- Book viewing appointments directly with managers.
- Send messages/inquiries to the management team.

## ✨ Implemented Features

- **Auth System:** Secure login/logout and self-registration for tenants.
- **Role-Based UI:** Navbar and action buttons dynamically adapt based on user roles.
- **Advanced Search:** Multi-parameter filtering for apartments (Price, Rooms, Status).
- **Communication Hub:** Full messaging system with "Reply" functionality.
- **Task Management:** Appointment scheduling and event/issue reporting.
- **Data Integrity:** Server-side validation (Data Annotations) and Client-side JS validation.
- **API Access:** Exposes programmatic endpoints for apartments, buildings, and more.

## ⚠️ Known Limitations

- **Image Uploads:** Currently uses text-based placeholders; file storage for apartment photos is planned for future releases.
- **Real-time Notifications:** Messages rely on page refreshes; SignalR integration is not yet implemented.
- **Reports:** Financial reporting module is currently a manual SQL view rather than a dynamic dashboard.
