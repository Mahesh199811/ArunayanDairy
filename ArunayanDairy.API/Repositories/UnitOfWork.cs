using Microsoft.EntityFrameworkCore.Storage;
using ArunayanDairy.API.Data;
using ArunayanDairy.API.Models;

namespace ArunayanDairy.API.Repositories;

/// <summary>
/// Unit of Work implementation
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    private IRepository<User>? _users;
    private IRepository<Category>? _categories;
    private IRepository<Product>? _products;
    private IRepository<ProductAvailability>? _productAvailabilities;
    private IRepository<Order>? _orders;
    private IRepository<OrderItem>? _orderItems;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IRepository<User> Users => 
        _users ??= new Repository<User>(_context);

    public IRepository<Category> Categories => 
        _categories ??= new Repository<Category>(_context);

    public IRepository<Product> Products => 
        _products ??= new Repository<Product>(_context);

    public IRepository<ProductAvailability> ProductAvailabilities => 
        _productAvailabilities ??= new Repository<ProductAvailability>(_context);

    public IRepository<Order> Orders => 
        _orders ??= new Repository<Order>(_context);

    public IRepository<OrderItem> OrderItems => 
        _orderItems ??= new Repository<OrderItem>(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
            }
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
