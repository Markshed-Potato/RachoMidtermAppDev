using MidtermProject.Models;

namespace MidtermProject.Services;

/// <summary>Per-session (per-circuit) state: cart + logged-in user. Data comes from the repositories.</summary>
public class KeylabStoreService
{
    private readonly ProductRepository _products;
    private readonly UserRepository _users;
    private readonly List<CartItem> _cart = new();

    public KeylabStoreService(ProductRepository products, UserRepository users)
    {
        _products = products;
        _users = users;
    }

    public event Action? OnChange;

    public string CurrentUser { get; private set; } = string.Empty;
    public bool IsLoggedIn => !string.IsNullOrEmpty(CurrentUser);

    public IReadOnlyList<CartItem> Cart => _cart;
    public Order? LastOrder { get; private set; }

    public List<Product> GetProducts() => _products.GetAll();
    public Product? GetProductById(int id) => _products.GetById(id);

    public void AddToCart(Product product, string color = "Black", string switchType = "Linear", int qty = 1)
    {
        var existing = _cart.FirstOrDefault(c => c.Product.Id == product.Id && c.SelectedColor == color && c.SelectedSwitch == switchType);
        if (existing != null) existing.Quantity += qty;
        else _cart.Add(new CartItem { Product = product, SelectedColor = color, SelectedSwitch = switchType, Quantity = qty });
        Notify();
    }

    public void RemoveFromCart(CartItem item) { _cart.Remove(item); Notify(); }

    public void ChangeQuantity(CartItem item, int delta)
    {
        item.Quantity += delta;
        if (item.Quantity <= 0) _cart.Remove(item);
        Notify();
    }

    public int GetCartCount() => _cart.Sum(c => c.Quantity);
    public decimal GetCartTotal() => _cart.Sum(c => c.Subtotal);

    public (bool ok, string error) Register(string fullName, string email, string password)
    {
        var result = _users.Register(fullName, email, password);
        if (result.ok) { CurrentUser = fullName.Trim(); Notify(); }
        return result;
    }

    public bool Login(string email, string password)
    {
        var user = _users.Validate(email, password);
        if (user == null) return false;
        CurrentUser = user.FullName;
        Notify();
        return true;
    }

    public void Logout()
    {
        CurrentUser = string.Empty;
        _cart.Clear();
        LastOrder = null;
        Notify();
    }

    public Order PlaceOrder(string name, string address, string paymentMethod)
    {
        var order = new Order
        {
            Id = "KL-" + DateTime.Now.ToString("yyMMdd") + "-" + Random.Shared.Next(1000, 9999),
            Date = DateTime.Now,
            Items = _cart.ToList(),
            Total = GetCartTotal(),
            CustomerName = name,
            Address = address,
            PaymentMethod = paymentMethod
        };
        _cart.Clear();
        LastOrder = order;
        Notify();
        return order;
    }

    private void Notify() => OnChange?.Invoke();
}
