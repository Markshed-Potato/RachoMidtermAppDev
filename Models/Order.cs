namespace MidtermProject.Models;

public class Order
{
    public string Id { get; set; } = "";
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public List<CartItem> Items { get; set; } = new();
    public decimal Total { get; set; }
    public string CustomerName { get; set; } = "";
    public string Address { get; set; } = "";
    public string PaymentMethod { get; set; } = "Cash on Delivery";
}

public class CheckoutModel
{
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Full name is required.")]
    public string FullName { get; set; } = "";

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Address is required.")]
    public string Address { get; set; } = "";

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "City is required.")]
    public string City { get; set; } = "";

    public string PaymentMethod { get; set; } = "Cash on Delivery";
}
