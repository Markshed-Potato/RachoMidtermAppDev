using Microsoft.EntityFrameworkCore;
using MidtermProject.Data;
using MidtermProject.Models;

namespace MidtermProject.Services;

public class ProductRepository
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public ProductRepository(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public List<Product> GetAll()
    {
        using var db = _factory.CreateDbContext();
        return db.Products.AsNoTracking().ToList();
    }

    public Product? GetById(int id)
    {
        using var db = _factory.CreateDbContext();
        return db.Products.AsNoTracking().FirstOrDefault(p => p.Id == id);
    }
}
