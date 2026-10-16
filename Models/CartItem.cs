namespace MidtermProject.Models;

public class CartItem
{
    public Product Product { get; set; } = new();
    public string SelectedColor { get; set; } = "Black";
    public string SelectedSwitch { get; set; } = "Linear";
    public int Quantity { get; set; } = 1;
    public decimal Subtotal => Product.Price * Quantity;
}