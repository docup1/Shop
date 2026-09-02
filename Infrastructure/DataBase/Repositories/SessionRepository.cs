using Domain.Contracts;
using Domain.Models;
using Infrastructure.DataBase.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataBase.Repositories;

public class SessionRepository : Repository<Session, Entities.Session>, ISessionRepository
{
    private readonly ApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public SessionRepository(ApplicationDbContext context, TimeProvider timeProvider)
        : base(context.Sessions, EntityMapper.ToEntity, EntityMapper.ToDomain)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<Session?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var entity = await _context.Sessions
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.Id == id && s.ExpiresAt > now, cancellationToken);

        return entity is null ? null : EntityMapper.ToDomain(entity);
    }

    public async Task<Page<Session>> GetActiveByUserIdAsync(Guid userId, QueryParams queryParams, CancellationToken cancellationToken = default)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var query = _context.Sessions
            .AsNoTracking()
            .Where(s => s.UserId == userId && s.ExpiresAt > now)
            .OrderByDescending(s => s.Id);

        return await Paging.ToPageAsync(query, queryParams, EntityMapper.ToDomain, e => e.Id, descending: true, cancellationToken: cancellationToken);
    }

    public async Task<Page<Session>> GetExpiredAsync(QueryParams queryParams, CancellationToken cancellationToken = default)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var query = _context.Sessions
            .AsNoTracking()
            .Where(s => s.ExpiresAt <= now)
            .OrderByDescending(s => s.Id);

        return await Paging.ToPageAsync(query, queryParams, EntityMapper.ToDomain, e => e.Id, descending: true, cancellationToken: cancellationToken);
    }
}