using System.Linq.Expressions;

namespace IncidentRegistrationMvc.Business;

/// <summary>
/// Business-tier boundary used by the presentation tier. Persistence details
/// are implemented by the data tier and are never exposed as EF Core types.
/// </summary>
public interface IApplicationService
{
    IQueryable<T> Query<T>() where T : class;
    Task<T?> FindUserForLoginAsync<T>(string usernameOrEmail) where T : class;
    Task<T?> FindAsync<T>(params object[] keyValues) where T : class;
    void Add<T>(T entity) where T : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<int> MarkUnreadNotificationsReadAsync(int userId, DateTime readOn);
}

public static class ApplicationQueryExtensions
{
    public static Task<List<T>> ToListAsync<T>(this IQueryable<T> query) =>
        Task.FromResult(query.ToList());

    public static Task<T?> FirstOrDefaultAsync<T>(this IQueryable<T> query) =>
        Task.FromResult(query.FirstOrDefault());

    public static Task<T?> FirstOrDefaultAsync<T>(this IQueryable<T> query, Expression<Func<T, bool>> predicate) =>
        Task.FromResult(query.FirstOrDefault(predicate));

    public static Task<bool> AnyAsync<T>(this IQueryable<T> query) =>
        Task.FromResult(query.Any());

    public static Task<bool> AnyAsync<T>(this IQueryable<T> query, Expression<Func<T, bool>> predicate) =>
        Task.FromResult(query.Any(predicate));
}
