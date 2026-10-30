using MidtermProject.Models;

namespace MidtermProject.Services;

public enum ToastKind { Success, Info, Error }

public record ToastMessage(Guid Id, string Text, ToastKind Kind);

/// <summary>
/// Per-circuit state: cart, promo, session user and toast notifications.
/// Components subscribe to <see cref="OnChange"/> / <see cref="OnToast"/> and must unsubscribe in Dispose.
/// </summary>
public class KeylabStoreService
{
    public const decimal TaxRate = 0.12m;
    public const decimal FreeShippingThreshold = 150m;
    public const decimal FlatShipping = 9.99m;

    private static readonly Dictionary<string, decimal> PromoCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["KEYLAB10"] = 0.10m,
        ["THOCK20"] = 0.20m,
    };

    private readonly ProductRepository _products;
    private readonly UserRepository _users;
    private readonly List<CartItem> _cart = new();

    public KeylabStoreService(ProductRepository products, UserRepository users)
    {
        _products = products;
        _users = users;
    }

    // ---- Events ---------------------------------------------------------
    public event Action? OnChange;
    public event Action<ToastMessage>? OnToast;

    private void NotifyStateChanged() => OnChange?.Invoke();

    public void ShowToast(string text, ToastKind kind = ToastKind.Success) =>
        OnToast?.Invoke(new ToastMessage(Guid.NewGuid(), text, kind));

    // ---- Session --------------------------------------------------------
    public string CurrentUser { get; private set; } = string.Empty;
    public string CurrentEmail { get; private set; } = string.Empty;
    public bool IsLoggedIn => !string.IsNullOrEmpty(CurrentUser);
    public string Initials => string.Concat(CurrentUser.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(w => char.ToUpperInvariant(w[0])));

    public (bool ok, string error) Register(string fullName, string email, string password)
    {
        var result = _users.Register(fullName, email, password);
        if (result.ok)
        {
            CurrentUser = fullName.Trim();
            CurrentEmail = email.Trim();
            NotifyStateChanged();
        }
        return result;
    }

    public bool Login(string email, string password)
    {
        var user = _users.Validate(email, password);
        if (user == null) return false;
        CurrentUser = user.FullName;
        CurrentEmail = user.Email;
        NotifyStateChanged();
        return true;
    }

    public void Logout()
    {
        CurrentUser = CurrentEmail = string.Empty;
        _cart.Clear();
        AppliedPromo = string.Empty;
        LastOrder = null;
        NotifyStateChanged();
    }

    // ---- Catalog --------------------------------------------------------
    public List<Product> GetProducts() => _products.GetAll();
    public Task<List<Product>> GetProductsAsync() => Task.FromResult(_products.GetAll());
    public Product? GetProductById(int id) => _products.GetById(id);
    public Task<Product?> GetProductByIdAsync(int id) => Task.FromResult(_products.GetById(id));

    // ---- Cart -----------------------------------------------------------
    public IReadOnlyList<CartItem> Cart => _cart;
    public Order? LastOrder { get; private set; }
    public string AppliedPromo { get; private set; } = string.Empty;

    public int GetCartCount() => _cart.Sum(c => c.Quantity);
    public decimal GetCartTotal() => _cart.Sum(c => c.Subtotal); // items subtotal (kept for compatibility)

    public decimal Subtotal => GetCartTotal();
    public decimal DiscountRate => PromoCodes.TryGetValue(AppliedPromo, out var r) ? r : 0m;
    public decimal Discount => Math.Round(Subtotal * DiscountRate, 2);
    public decimal Tax => Math.Round((Subtotal - Discount) * TaxRate, 2);
    public decimal Shipping => _cart.Count == 0 || Subtotal - Discount >= FreeShippingThreshold ? 0m : FlatShipping;
    public decimal GrandTotal => Subtotal - Discount + Tax + Shipping;
    public decimal AmountToFreeShipping => Math.Max(0m, FreeShippingThreshold - (Subtotal - Discount));

    /// <summary>Adds an item; quantity is clamped to available stock.</summary>
    public bool AddToCart(Product product, string color = "Black", string switchType = "Linear", int qty = 1)
    {
        if (!product.InStock)
        {
            ShowToast($"{product.Name} is out of stock.", ToastKind.Error);
            return false;
        }

        qty = Math.Max(1, qty);
        var existing = _cart.FirstOrDefault(c => c.Product.Id == product.Id && c.SelectedColor == color && c.SelectedSwitch == switchType);
        var inCart = _cart.Where(c => c.Product.Id == product.Id).Sum(c => c.Quantity);
        var room = product.Stock - inCart;
        if (room <= 0)
        {
            ShowToast($"You already have all {product.Stock} in your cart.", ToastKind.Info);
            return false;
        }

        var toAdd = Math.Min(qty, room);
        if (existing != null) existing.Quantity += toAdd;
        else _cart.Add(new CartItem { Product = product, SelectedColor = color, SelectedSwitch = switchType, Quantity = toAdd });

        ShowToast(toAdd < qty ? $"Added {toAdd} × {product.Name} (stock limit reached)." : $"{product.Name} added to cart.");
        NotifyStateChanged();
        return true;
    }

    public void RemoveFromCart(CartItem item)
    {
        if (_cart.Remove(item)) NotifyStateChanged();
    }

    public void ChangeQuantity(CartItem item, int delta) => SetQuantity(item, item.Quantity + delta);

    public void SetQuantity(CartItem item, int quantity)
    {
        if (quantity <= 0) { _cart.Remove(item); NotifyStateChanged(); return; }

        var others = _cart.Where(c => c.Product.Id == item.Product.Id && c != item).Sum(c => c.Quantity);
        var max = Math.Max(1, item.Product.Stock - others);
        if (quantity > max) ShowToast($"Only {item.Product.Stock} in stock.", ToastKind.Info);
        item.Quantity = Math.Min(quantity, max);
        NotifyStateChanged();
    }

    public void ClearCart()
    {
        _cart.Clear();
        AppliedPromo = string.Empty;
        NotifyStateChanged();
    }

    public (bool ok, string message) ApplyPromo(string code)
    {
        code = (code ?? "").Trim();
        if (!PromoCodes.TryGetValue(code, out var rate))
            return (false, "Invalid promo code.");
        AppliedPromo = code.ToUpperInvariant();
        NotifyStateChanged();
        return (true, $"{rate:P0} discount applied.");
    }

    public void RemovePromo()
    {
        AppliedPromo = string.Empty;
        NotifyStateChanged();
    }

    // ---- Orders ---------------------------------------------------------
    public Order PlaceOrder(string name, string address, string paymentMethod)
    {
        var order = new Order
        {
            Id = "KL-" + DateTime.Now.ToString("yyMMdd") + "-" + Random.Shared.Next(1000, 9999),
            Date = DateTime.Now,
            Items = _cart.ToList(),
            Subtotal = Subtotal,
            Discount = Discount,
            Tax = Tax,
            Shipping = Shipping,
            Total = GrandTotal,
            PromoCode = AppliedPromo,
            CustomerName = name,
            Address = address,
            PaymentMethod = paymentMethod
        };
        _cart.Clear();
        AppliedPromo = string.Empty;
        LastOrder = order;
        NotifyStateChanged();
        return order;
    }
}
