using Domain.Contracts;
using Domain.Models;
using Domain.Types;
using Infrastructure.DataBase.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataBase.Repositories;

public class OrderRepository : Repository<Order, Entities.Order>, IOrderRepository
{
    private readonly ApplicationDbContext _context;

    public OrderRepository(ApplicationDbContext context)
        : base(context.Orders, EntityMapper.ToEntity, EntityMapper.ToDomain)
    {
        _context = context;
    }

    public async Task<Page<Order>> GetByUserIdAsync(Guid userId, QueryParams queryParams, CancellationToken cancellationToken = default)
    {
        var query = _context.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.Id);

        return await Paging.ToPageAsync(query, queryParams, EntityMapper.ToDomain, e => e.Id, descending: true, cancellationToken: cancellationToken);
    }

    public async Task<Page<Order>> GetByStatusAsync(Status status, QueryParams queryParams, CancellationToken cancellationToken = default)
    {
        var query = _context.Orders
            .AsNoTracking()
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.Id);

        return await Paging.ToPageAsync(query, queryParams, EntityMapper.ToDomain, e => e.Id, descending: true, cancellationToken: cancellationToken);
    }

    /// <summary>Все заказы, новые сверху. Опциональный фильтр по статусу.</summary>
    public async Task<Page<Order>> GetAllAsync(QueryParams queryParams, Status? status = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Orders.AsNoTracking();

        if (status is not null)
            query = query.Where(o => o.Status == status.Value);

        query = query.OrderByDescending(o => o.Id);

        return await Paging.ToPageAsync(query, queryParams, EntityMapper.ToDomain, e => e.Id, descending: true, cancellationToken: cancellationToken);
    }

    public async Task<Page<Order>> GetByRecipientCityAsync(string recipientCity, QueryParams queryParams, CancellationToken cancellationToken = default)
    {
        var query = _context.Orders
            .AsNoTracking()
            .Where(o => o.RecipientCity == recipientCity)
            .OrderBy(o => o.Id);

        return await Paging.ToPageAsync(query, queryParams, EntityMapper.ToDomain, e => e.Id, cancellationToken: cancellationToken);
    }
}