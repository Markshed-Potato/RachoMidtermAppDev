using System.ComponentModel.DataAnnotations;

namespace MidtermProject.Models;

public class Order
{
    public string Id { get; set; } = "";
    public DateTime Date { get; set; } = DateTime.Now;
    public List<CartItem> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }
    public decimal Shipping { get; set; }
    public decimal Total { get; set; }
    public string PromoCode { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string Address { get; set; } = "";
    public string PaymentMethod { get; set; } = "";
}

public class CheckoutModel : IValidatableObject
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "Enter your full name.")]
    public string FullName { get; set; } = "";

    [Required(ErrorMessage = "Phone number is required.")]
    [RegularExpression(@"^[+]?[0-9\s\-]{7,15}$", ErrorMessage = "Enter a valid phone number.")]
    public string Phone { get; set; } = "";

    [Required(ErrorMessage = "Street address is required.")]
    [StringLength(120, MinimumLength = 5, ErrorMessage = "Address looks too short.")]
    public string Address { get; set; } = "";

    [Required(ErrorMessage = "City is required.")]
    public string City { get; set; } = "";

    [Required(ErrorMessage = "Postal code is required.")]
    [RegularExpression(@"^[A-Za-z0-9\- ]{3,10}$", ErrorMessage = "Enter a valid postal code.")]
    public string PostalCode { get; set; } = "";

    public string PaymentMethod { get; set; } = "Card";
    public string CardNumber { get; set; } = "";
    public string Expiry { get; set; } = "";
    public string Cvv { get; set; } = "";

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (PaymentMethod != "Card") yield break;

        var digits = new string(CardNumber.Where(char.IsDigit).ToArray());
        if (digits.Length is < 13 or > 19)
            yield return new ValidationResult("Enter a valid card number.", new[] { nameof(CardNumber) });

        if (!System.Text.RegularExpressions.Regex.IsMatch(Expiry.Trim(), @"^(0[1-9]|1[0-2])\/\d{2}$"))
            yield return new ValidationResult("Expiry must be MM/YY.", new[] { nameof(Expiry) });

        if (Cvv.Length is < 3 or > 4 || !Cvv.All(char.IsDigit))
            yield return new ValidationResult("Enter a valid CVV.", new[] { nameof(Cvv) });
    }
}
