# Three-tier architecture

The solution is split into three independently compiled tiers:

1. **Presentation (`IncidentRegistrationMvc`)**
   - ASP.NET Core controllers, Razor views, static files, session and request handling.
   - Controllers depend on `IApplicationService`; they do not use EF Core or
     `ApplicationDbContext`.
2. **Business (`IncidentRegistrationMvc.Business`)**
   - Defines the application-facing service contract and shared client
     configuration contract.
   - Contains no database provider or EF Core dependency.
3. **Data (`IncidentRegistrationMvc.Data`)**
   - Owns the entity models, EF Core `ApplicationDbContext`, relationship loading,
     persistence operations and the application-service implementation.
   - This is the only project with the SQL Server EF Core package.

The web project's `Program.cs` is the composition root. It references the concrete
data implementation only to register it with dependency injection. Runtime feature
code follows this dependency flow:

`Controller -> IApplicationService -> ApplicationService -> ApplicationDbContext -> SQL Server`

## Boundary rules

- Do not inject `ApplicationDbContext` into a controller.
- Do not add EF Core packages or imports to the presentation or business projects.
- Add persistence behavior behind `IApplicationService` and implement it in the
  data project.
- Keep HTTP/session/redirect behavior in the presentation tier.

## Client isolation

Every Super Admin, Admin and User belongs to one `ConfigureClient` through
`User.ClientId`. Authentication stores that value in the session. The data-tier
application service then automatically:

- filters every `IClientEntity` query by the session's `ClientId`;
- stamps new entities with that `ClientId`;
- rejects attempts to write an entity for another client; and
- limits client branding/configuration to the same client.

Login is the sole unscoped lookup because it must discover the user's client before
the tenant session exists. Usernames and email addresses therefore remain globally
unique. Apply `Database/2026-07-04_AddClientIsolation.sql` once to upgrade an existing
database before deploying this application version.
