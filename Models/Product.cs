namespace MidtermProject.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // Keyboards, Switches, Keycaps, Accessories
    public decimal Price { get; set; }
    public double Rating { get; set; } = 5.0;
    public int ReviewCount { get; set; } = 128;
    public string PrimaryImageUrl { get; set; } = string.Empty;
    public List<string> AdditionalImages { get; set; } = new();
    public string Description { get; set; } = string.Empty;
    public string Layout { get; set; } = "65%"; // 60%, 65%, 75%, TKL, Full
    public string Color { get; set; } = "Black"; // Black, White, Grey, Blue
    public List<string> SwitchOptions { get; set; } = new() { "Linear", "Tactile", "Clicky" };
    public bool IsOnSale { get; set; }
    public string? BadgeText { get; set; }
}