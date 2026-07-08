using IncidentRegistrationMvc.Business;
using IncidentRegistrationMvc.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace IncidentRegistrationMvc.Data;

public sealed class ApplicationService : IApplicationService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApplicationService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public IQueryable<T> Query<T>() where T : class
    {
        IQueryable<T> query = _context.Set<T>();

        // Navigation loading belongs to the data tier. Callers receive complete
        // business objects without knowing about EF Core's Include API.
        if (typeof(T) == typeof(User))
            query = (IQueryable<T>)_context.Users.Include(x => x.AccessRights).ThenInclude(x => x.UserAccessRight);
        else if (typeof(T) == typeof(LocationMachineMaster))
            query = (IQueryable<T>)_context.LocationMachineMasters.Include(x => x.UserAccessRight);
        else if (typeof(T) == typeof(FormResponse))
            query = (IQueryable<T>)_context.FormResponses.Include(x => x.ReportedByUser);
        else if (typeof(T) == typeof(AuditLog))
            query = (IQueryable<T>)_context.AuditLogs.Include(x => x.ActorUser);
        else if (typeof(T) == typeof(ReportMessage))
            query = (IQueryable<T>)_context.ReportMessages
                .Include(x => x.SenderUser).Include(x => x.RecipientUser).Include(x => x.FormResponse);

        int? clientId = CurrentClientId;
        if (!clientId.HasValue)
            return query.Where(_ => false);

        if (typeof(IClientEntity).IsAssignableFrom(typeof(T)))
            query = query.Where(BuildClientPredicate<T>(clientId.Value));
        else if (typeof(T) == typeof(ConfigureClient))
            query = (IQueryable<T>)_context.ConfigureClients.Where(x => x.Id == clientId.Value);

        return query;
    }

    public async Task<T?> FindAsync<T>(params object[] keyValues) where T : class
    {
        T? entity = await _context.Set<T>().FindAsync(keyValues);
        if (entity == null || !CurrentClientId.HasValue) return null;
        if (entity is IClientEntity clientEntity && clientEntity.ClientId != CurrentClientId.Value) return null;
        if (entity is ConfigureClient client && client.Id != CurrentClientId.Value) return null;
        return entity;
    }

    public async Task<T?> FindUserForLoginAsync<T>(string usernameOrEmail) where T : class
    {
        if (typeof(T) != typeof(User))
            throw new InvalidOperationException("The unscoped login lookup is only valid for users.");

        User? user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IsActive && (x.Username == usernameOrEmail || x.Email == usernameOrEmail));
        return (T?)(object?)user;
    }

    public void Add<T>(T entity) where T : class
    {
        if (entity is IClientEntity clientEntity)
            clientEntity.ClientId = CurrentClientId ?? throw new InvalidOperationException("A client-scoped session is required.");
        _context.Set<T>().Add(entity);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);

    public Task<int> MarkUnreadNotificationsReadAsync(int userId, DateTime readOn) =>
        Query<UserNotification>()
            .Where(x => x.UserId == userId && x.ReadOn == null)
            .ExecuteUpdateAsync(x => x.SetProperty(n => n.ReadOn, readOn));

    private int? CurrentClientId => _httpContextAccessor.HttpContext?.Session.GetInt32("ClientId");

    private static System.Linq.Expressions.Expression<Func<T, bool>> BuildClientPredicate<T>(int clientId) where T : class
    {
        var parameter = System.Linq.Expressions.Expression.Parameter(typeof(T), "entity");
        var property = System.Linq.Expressions.Expression.Property(parameter, nameof(IClientEntity.ClientId));
        var equality = System.Linq.Expressions.Expression.Equal(property, System.Linq.Expressions.Expression.Constant(clientId));
        return System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(equality, parameter);
    }
}
