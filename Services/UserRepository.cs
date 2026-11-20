using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using MidtermProject.Data;
using MidtermProject.Models;

namespace MidtermProject.Services;

public class UserRepository
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public UserRepository(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    private static string Hash(string password, string salt) =>
        Convert.ToBase64String(Rfc2898DeriveBytes.Pbkdf2(
            password, Convert.FromBase64String(salt), 100_000, HashAlgorithmName.SHA256, 32));

    public (bool ok, string error) Register(string fullName, string email, string password)
    {
        try
        {
            email = email.Trim();
            using var db = _factory.CreateDbContext();
            if (db.Users.Any(u => u.Email.ToLower() == email.ToLower()))
                return (false, "An account with that email already exists.");

            var salt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
            db.Users.Add(new User
            {
                FullName = fullName.Trim(),
                Email = email,
                Salt = salt,
                PasswordHash = Hash(password, salt)
            });
            db.SaveChanges();
            return (true, "");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Register error: {ex.Message}");
            return (false, "Unable to register at this time. Please try again.");
        }
    }

    public User? Validate(string email, string password)
    {
        try
        {
            using var db = _factory.CreateDbContext();
            var user = db.Users.FirstOrDefault(u => u.Email.ToLower() == email.Trim().ToLower());
            if (user == null) return null;
            var attempt = Hash(password, user.Salt);
            return CryptographicOperations.FixedTimeEquals(
                Convert.FromBase64String(attempt), Convert.FromBase64String(user.PasswordHash)) ? user : null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Validate error: {ex.Message}");
            return null;
        }
    }
}
