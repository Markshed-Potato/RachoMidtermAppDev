namespace MidtermProject.Models;

public class Order
{
    public string Id { get; set; } = "";
    public DateTime Date { get; set; } = DateTime.Now;
    public List<CartItem> Items { get; set; } = new();
    public decimal Total { get; set; }
    public string CustomerName { get; set; } = "";
    public string Address { get; set; } = "";
    public string PaymentMethod { get; set; } = "";
}

public class CheckoutModel
{
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Full name is required.")]
    public string FullName { get; set; } = "";

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Address is required.")]
    public string Address { get; set; } = "";

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "City is required.")]
    public string City { get; set; } = "";

    public string PaymentMethod { get; set; } = "Card";
    public string CardNumber { get; set; } = "";
    public string Expiry { get; set; } = "";
    public string Cvv { get; set; } = "";
}
