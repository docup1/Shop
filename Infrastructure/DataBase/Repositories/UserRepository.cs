using Domain.Contracts;
using Domain.Models;
using Infrastructure.DataBase.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataBase.Repositories;

public class UserRepository : Repository<User, Entities.User>, IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
        : base(context.Users, EntityMapper.ToEntity, EntityMapper.ToDomain)
    {
        _context = context;
    }

    public async Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.UserName == userName, cancellationToken);

        return entity is null ? null : EntityMapper.ToDomain(entity);
    }

    public async Task<bool> ExistsByUserNameAsync(string userName, CancellationToken cancellationToken = default)
        => await _context.Users.AnyAsync(u => u.UserName == userName, cancellationToken);

    public async Task<Page<User>> GetAdminsAsync(QueryParams queryParams, CancellationToken cancellationToken = default)
    {
        var query = _context.Users
            .AsNoTracking()
            .Where(u => u.IsAdmin)
            .OrderBy(u => u.Id);

        return await Paging.ToPageAsync(query, queryParams, EntityMapper.ToDomain, e => e.Id, cancellationToken: cancellationToken);
    }

    public async Task<Page<User>> GetAllAsync(QueryParams queryParams, CancellationToken cancellationToken = default)
    {
        var query = _context.Users
            .AsNoTracking()
            .OrderBy(u => u.Id);

        return await Paging.ToPageAsync(query, queryParams, EntityMapper.ToDomain, e => e.Id, cancellationToken: cancellationToken);
    }
}