# CulinaryCraftWeb

CulinaryCraftWeb is an ASP.NET Core MVC web application for sharing and managing recipes. Users can register, log in, post recipes, and browse a collection of culinary creations.

## Features

- User registration and authentication (cookie-based)
- Post, edit, and view recipes
- Admin management for users and recipes
- Responsive UI with Razor Views and Bootstrap
- Entity Framework Core with SQL Server

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)

### Setup

1. **Clone the repository:**
   ```sh
   git clone https://github.com/yourusername/CulinaryCraftWeb.git
   cd CulinaryCraftWeb
   ```

2. **Configure the database:**
   - Update the `DefaultConnection` string in `appsettings.Development.json` with your SQL Server details.

3. **Apply migrations:**
   ```sh
   dotnet ef database update
   ```

4. **Run the application:**
   ```sh
   dotnet run
   ```

5. **Open in browser:**
   - Navigate to `https://localhost:5001` or the URL shown in the terminal.

## Project Structure

- `Controllers/` — MVC controllers for user, admin, recipes, etc.
- `Models/` — ViewModels and data models
- `Data/ApplicationDbContext.cs` — Entity Framework Core DB context
- `Views/` — Razor views for UI
- `wwwroot/` — Static files (CSS, JS, images)

*Happy cooking and coding!*
