using IncidentRegistrationMvc.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace IncidentRegistrationMvc.Data;

public class ApplicationDbContext : DbContext
{
    private readonly IHttpContextAccessor? _httpContextAccessor;
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor? httpContextAccessor = null) : base(options) => _httpContextAccessor = httpContextAccessor;

    public DbSet<FormResponse> FormResponses => Set<FormResponse>();
    public DbSet<User> Users => Set<User>();
    public DbSet<DropdownMaster> DropdownMasters => Set<DropdownMaster>();
    public DbSet<LocationMachineMaster> LocationMachineMasters => Set<LocationMachineMaster>();
    public DbSet<UserAccessRight> UserAccessRights => Set<UserAccessRight>();
    public DbSet<UserAccessRightMapping> UserAccessRightMappings => Set<UserAccessRightMapping>();
    public DbSet<UserActivityLog> UserActivityLogs => Set<UserActivityLog>();
    public DbSet<UserAccessRightMappingLog> UserAccessRightMappingLogs => Set<UserAccessRightMappingLog>();
    public DbSet<ConfigureClient> ConfigureClients => Set<ConfigureClient>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<ReportMessage> ReportMessages => Set<ReportMessage>();
    public DbSet<UserNotification> UserNotifications => Set<UserNotification>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        StampClientEntries();
        AddAuditEntries();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void StampClientEntries()
    {
        int? clientId = _httpContextAccessor?.HttpContext?.Session.GetInt32("ClientId");
        if (!clientId.HasValue) return;

        foreach (var entry in ChangeTracker.Entries<IClientEntity>().Where(x => x.State == EntityState.Added))
        {
            if (entry.Entity.ClientId != 0 && entry.Entity.ClientId != clientId.Value)
                throw new InvalidOperationException("Cross-client writes are not allowed.");
            entry.Entity.ClientId = clientId.Value;
        }
    }

    private void AddAuditEntries()
    {
        var session = _httpContextAccessor?.HttpContext?.Session;
        int? actorId = session?.GetInt32("UserId");
        if (!actorId.HasValue) return;
        string actor = session?.GetString("Username") ?? "Unknown";
        var entries = ChangeTracker.Entries().Where(e => e.Entity is not AuditLog && e.State is EntityState.Modified or EntityState.Deleted).ToList();
        foreach (var entry in entries)
        {
            var changes = entry.Properties.Where(p => entry.State == EntityState.Deleted || p.IsModified)
                .Select(p => $"{p.Metadata.Name}: '{p.OriginalValue}' -> '{(entry.State == EntityState.Deleted ? "[deleted]" : p.CurrentValue)}'");
            string id = string.Join(",", entry.Properties.Where(p => p.Metadata.IsPrimaryKey()).Select(p => p.CurrentValue?.ToString()));
            AuditLogs.Add(new AuditLog { ClientId = session!.GetInt32("ClientId") ?? 0, ActorUserId = actorId, ActorName = actor, Action = entry.State == EntityState.Deleted ? "Deleted" : "Updated", EntityType = entry.Metadata.ClrType.Name, EntityId = id, Changes = string.Join(Environment.NewLine, changes) });
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.Property(x => x.ActorName).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Action).HasMaxLength(20).IsRequired();
            entity.Property(x => x.EntityType).HasMaxLength(50).IsRequired();
            entity.Property(x => x.EntityId).HasMaxLength(50).IsRequired();
            entity.HasOne(x => x.ActorUser).WithMany().HasForeignKey(x => x.ActorUserId).OnDelete(DeleteBehavior.SetNull);
        });
        modelBuilder.Entity<ReportMessage>(entity =>
        {
            entity.Property(x => x.Message).HasMaxLength(2000).IsRequired();
            entity.HasOne(x => x.FormResponse).WithMany().HasForeignKey(x => x.FormResponseId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.SenderUser).WithMany().HasForeignKey(x => x.SenderUserId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(x => x.RecipientUser).WithMany().HasForeignKey(x => x.RecipientUserId).OnDelete(DeleteBehavior.NoAction);
        });
        modelBuilder.Entity<UserNotification>(entity =>
        {
            entity.Property(x => x.Title).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Message).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Url).HasMaxLength(500);
            entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<DropdownMaster>(entity =>
        {
            entity.ToTable("DropdownMasters");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Category).HasMaxLength(50).IsUnicode(false).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.SortOrder).HasDefaultValue(0);
        });

        modelBuilder.Entity<LocationMachineMaster>(entity =>
        {
            entity.ToTable("LocationMachineMasters");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EchoMachine).HasMaxLength(100).IsRequired();
            entity.Property(x => x.ClinicalEngNumber).HasMaxLength(50).IsUnicode(false).IsRequired();
            entity.Property(x => x.Location).HasMaxLength(100).IsRequired();
            // These columns contain nulls in legacy records. They remain optional in
            // persistence while the registration form enforces its own Required rules.
            entity.Property(x => x.SerialNumber).HasMaxLength(100).IsUnicode(false).IsRequired(false);
            entity.Property(x => x.PropertyTagNumber).HasMaxLength(100).IsUnicode(false).IsRequired();
            entity.Property(x => x.DateAcquired).HasMaxLength(50).IsRequired(false);
            entity.Property(x => x.Problems).HasMaxLength(500);
            entity.Property(x => x.DateReported).HasMaxLength(50).IsRequired(false);
            entity.Property(x => x.ActionTaken).HasMaxLength(500);
            entity.Property(x => x.FinalActionRecommendation).HasMaxLength(500);
            entity.Property(x => x.WeeklyFilterCleaningDone).HasMaxLength(20).IsUnicode(false).IsRequired(false);
            entity.Property(x => x.AnnualDueDate).HasMaxLength(50).IsRequired(false);
            entity.Property(x => x.UserAccessRightId).IsRequired(false);
            entity.Property(x => x.AnnualPreventiveDueDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.HasOne(x => x.UserAccessRight)
                .WithMany()
                .HasForeignKey(x => x.UserAccessRightId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ConfigureClient>(entity =>
        {
            entity.ToTable("ConfigureClients");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ProjectTitle).HasMaxLength(150).IsRequired();
            entity.Property(x => x.ClientName).HasMaxLength(150);
            entity.Property(x => x.ContactNo).HasMaxLength(50).IsUnicode(false);
            entity.Property(x => x.Email).HasMaxLength(150).IsUnicode(false);
            entity.Property(x => x.Website).HasMaxLength(200).IsUnicode(false);
            entity.Property(x => x.Address).HasMaxLength(500);
            entity.Property(x => x.City).HasMaxLength(100);
            entity.Property(x => x.Country).HasMaxLength(100);
            entity.Property(x => x.SupportContact).HasMaxLength(50).IsUnicode(false);
            entity.Property(x => x.SupportEmail).HasMaxLength(150).IsUnicode(false);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreatedOn).HasDefaultValueSql("GETDATE()");
        });

        modelBuilder.Entity<UserAccessRight>(entity =>
        {
            entity.ToTable("UserAccessRights");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.ClientId, x.Name }).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreatedOn).HasDefaultValueSql("GETDATE()");
        });

        modelBuilder.Entity<UserAccessRightMapping>(entity =>
        {
            entity.ToTable("UserAccessRightMappings", table => table.HasTrigger("TR_UserAccessRightMappings_ActivityLog"));
            entity.HasKey(x => new { x.UserId, x.UserAccessRightId });
            entity.HasOne(x => x.User)
                .WithMany(x => x.AccessRights)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.UserAccessRight)
                .WithMany(x => x.Users)
                .HasForeignKey(x => x.UserAccessRightId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserActivityLog>(entity =>
        {
            entity.ToTable("UserActivityLogs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ActivityType).HasMaxLength(20).IsUnicode(false).IsRequired();
            entity.Property(x => x.OldUsername).HasMaxLength(50).IsUnicode(false);
            entity.Property(x => x.NewUsername).HasMaxLength(50).IsUnicode(false);
            entity.Property(x => x.OldEmail).HasMaxLength(100).IsUnicode(false);
            entity.Property(x => x.NewEmail).HasMaxLength(100).IsUnicode(false);
            entity.Property(x => x.OldUserType).HasMaxLength(20).IsUnicode(false);
            entity.Property(x => x.NewUserType).HasMaxLength(20).IsUnicode(false);
            entity.Property(x => x.ActivityBy).HasMaxLength(128);
            entity.Property(x => x.HostName).HasMaxLength(128);
            entity.Property(x => x.ApplicationName).HasMaxLength(128);
            entity.Property(x => x.ActivityOn).HasDefaultValueSql("GETDATE()");
        });

        modelBuilder.Entity<UserAccessRightMappingLog>(entity =>
        {
            entity.ToTable("UserAccessRightMappingLogs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AccessRightName).HasMaxLength(100);
            entity.Property(x => x.ActivityType).HasMaxLength(20).IsUnicode(false).IsRequired();
            entity.Property(x => x.ActivityBy).HasMaxLength(128);
            entity.Property(x => x.HostName).HasMaxLength(128);
            entity.Property(x => x.ApplicationName).HasMaxLength(128);
            entity.Property(x => x.ActivityOn).HasDefaultValueSql("GETDATE()");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users", table => table.HasTrigger("TR_Users_UserActivityLog"));
            entity.HasKey(x => x.UserId);
            // Login does not ask for a client, so usernames and email addresses
            // remain globally unique and unambiguously identify the user's tenant.
            entity.HasIndex(x => x.Username).IsUnique();
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.Username).HasMaxLength(50).IsUnicode(false).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(100).IsUnicode(false).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(255).IsUnicode(false).IsRequired();
            entity.Property(x => x.UserType).HasMaxLength(20).IsUnicode(false).HasDefaultValue("User").IsRequired();
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");
        });

        modelBuilder.Entity<FormResponse>(entity =>
        {
            entity.ToTable("FormResponses");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EmailAddress).HasMaxLength(150);
            entity.Property(x => x.ReporterName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Type).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Device).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Location).HasMaxLength(100);
            entity.Property(x => x.EchoMachine).HasMaxLength(100);
            entity.Property(x => x.ClinicalEngNumber).HasMaxLength(50).IsUnicode(false);
            entity.Property(x => x.SerialNumber).HasMaxLength(100).IsUnicode(false);
            entity.Property(x => x.PropertyTagNumber).HasMaxLength(100).IsUnicode(false);
            entity.Property(x => x.AnnualPreventiveDueDate).HasColumnType("date");
            entity.Property(x => x.ImagePath).HasMaxLength(500);
            entity.Property(x => x.Priority).HasMaxLength(20).IsRequired();
            entity.Property(x => x.ResolvedStatus).HasMaxLength(50).HasDefaultValue("pending");
            entity.Property(x => x.CreatedOn).HasDefaultValueSql("GETDATE()");
            entity.HasOne(x => x.ReportedByUser)
                .WithMany()
                .HasForeignKey(x => x.ReportedBy)
                .OnDelete(DeleteBehavior.SetNull);
        });

        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                     .Where(x => typeof(IClientEntity).IsAssignableFrom(x.ClrType)))
        {
            modelBuilder.Entity(entityType.ClrType)
                .HasOne(typeof(ConfigureClient))
                .WithMany()
                .HasForeignKey(nameof(IClientEntity.ClientId))
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity(entityType.ClrType).HasIndex(nameof(IClientEntity.ClientId));
        }
    }
}
