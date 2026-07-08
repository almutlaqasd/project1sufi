Incident Registration MVC Project

Technology:
- ASP.NET Core MVC .NET 8
- SQL Server
- Entity Framework Core
- Bootstrap 5
- Chart.js

Login:
- Users are authenticated from the Users table in HospitalDB.
- PasswordHash should contain an ASP.NET Core Identity password hash, or a SHA-256 hex hash.
- UserType controls access: Admin can open Dashboard and Registration; User can open Registration only.

Database setup:
1. Open SQL Server Management Studio.
2. Run Database/CreateDatabase.sql.
3. Confirm database name: HospitalDB.

Connection string:
File: appsettings.json
Default:
Server=.;Database=HospitalDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true

If SQL Server uses named instance, change Server=.; to your instance, for example:
Server=.\SQLEXPRESS;

Run project:
1. Open IncidentRegistrationMvc.csproj in Visual Studio 2022.
2. Restore NuGet packages.
3. Set as Startup Project.
4. Run.

Pages:
/login
/registration/create
/dashboard

Uploads:
Images are saved in wwwroot/uploads.

hospitalweb.runasp.net

Medical Equipment Maintenance & Reporting System
