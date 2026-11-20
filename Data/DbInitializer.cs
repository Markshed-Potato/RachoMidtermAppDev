using System.Text.Json;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using MidtermProject.Models;

namespace MidtermProject.Data;

public static class DbInitializer
{
    public static void Initialize(AppDbContext context, IWebHostEnvironment env)
    {
        var databaseCreator = (RelationalDatabaseCreator)context.Database.GetService<IDatabaseCreator>();
        try
        {
            databaseCreator.CreateTables();
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "42P07")
        {
            // Table already exists, safe to ignore
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CreateTables notice: {ex.Message}");
        }

        if (!context.Products.Any())
        {
            var productPath = Path.Combine(env.ContentRootPath, "Data", "products.json");
            if (File.Exists(productPath))
            {
                var json = File.ReadAllText(productPath);
                var products = JsonSerializer.Deserialize<List<Product>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (products != null && products.Count > 0)
                {
                    context.Products.AddRange(products);
                    context.SaveChanges();
                }
            }
        }
        else
        {
            var broken = context.Products
                .Where(p => p.PrimaryImageUrl.Contains("photo-1541140532154-b024d705b909"))
                .ToList();
            foreach (var p in broken)
            {
                if (p.Category == "Switches")
                {
                    p.PrimaryImageUrl = "https://images.unsplash.com/photo-1595044426077-d36d9236d54a?w=600&auto=format&fit=crop";
                    p.AdditionalImages = new List<string> { "https://images.unsplash.com/photo-1595044426077-d36d9236d54a?w=600&auto=format&fit=crop" };
                }
                else
                {
                    p.PrimaryImageUrl = "https://images.unsplash.com/photo-1511467687858-23d96c32e4ae?w=600&auto=format&fit=crop";
                    p.AdditionalImages = new List<string> { "https://images.unsplash.com/photo-1511467687858-23d96c32e4ae?w=600&auto=format&fit=crop" };
                }
            }
            if (broken.Count > 0)
            {
                context.SaveChanges();
            }
        }

        if (!context.Users.Any())
        {
            var userPath = Path.Combine(env.ContentRootPath, "Data", "users.json");
            if (File.Exists(userPath))
            {
                var json = File.ReadAllText(userPath);
                var users = JsonSerializer.Deserialize<List<User>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (users != null && users.Count > 0)
                {
                    context.Users.AddRange(users);
                    context.SaveChanges();
                }
            }
        }
    }
}
