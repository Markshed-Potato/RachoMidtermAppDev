using System.Security.Cryptography;
using System.Text.Json;
using MidtermProject.Models;

namespace MidtermProject.Services;

public class UserRepository
{
    private readonly string _path;
    private readonly object _lock = new();
    private readonly List<User> _users;

    public UserRepository(IWebHostEnvironment env)
    {
        var dir = Path.Combine(env.ContentRootPath, "Data");
        Directory.CreateDirectory(dir);
        _path = Path.Combine(dir, "users.json");
        _users = File.Exists(_path)
            ? JsonSerializer.Deserialize<List<User>>(File.ReadAllText(_path)) ?? new()
            : new();
    }

    private static string Hash(string password, string salt) =>
        Convert.ToBase64String(Rfc2898DeriveBytes.Pbkdf2(
            password, Convert.FromBase64String(salt), 100_000, HashAlgorithmName.SHA256, 32));

    public (bool ok, string error) Register(string fullName, string email, string password)
    {
        lock (_lock)
        {
            email = email.Trim();
            if (_users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
                return (false, "An account with that email already exists.");

            var salt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
            _users.Add(new User { FullName = fullName.Trim(), Email = email, Salt = salt, PasswordHash = Hash(password, salt) });
            File.WriteAllText(_path, JsonSerializer.Serialize(_users, new JsonSerializerOptions { WriteIndented = true }));
            return (true, "");
        }
    }

    public User? Validate(string email, string password)
    {
        lock (_lock)
        {
            var user = _users.FirstOrDefault(u => u.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase));
            if (user == null) return null;
            var attempt = Hash(password, user.Salt);
            return CryptographicOperations.FixedTimeEquals(
                Convert.FromBase64String(attempt), Convert.FromBase64String(user.PasswordHash)) ? user : null;
        }
    }
}
