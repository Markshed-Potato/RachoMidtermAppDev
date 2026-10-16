using System.ComponentModel.DataAnnotations;

namespace MidtermProject.Models;

public class RegisterModel
{
    [Required(ErrorMessage = "Full name is required.")]
    public string FullName { get; set; } = "";

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Please confirm your password.")]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = "";

    [Range(typeof(bool), "true", "true", ErrorMessage = "You must agree to the Terms & Privacy Policy.")]
    public bool AgreeToTerms { get; set; }
}

public class LoginModel
{
    [Required(ErrorMessage = "Email is required.")]
    public string EmailOrUsername { get; set; } = "";

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = "";
}
