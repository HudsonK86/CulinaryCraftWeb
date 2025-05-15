using System.ComponentModel.DataAnnotations;

namespace CulinaryCraftWeb.Models;

public class ForgotPasswordViewModel
{
    [Required]
    [RegularExpression(@"^\S+$", ErrorMessage = "User ID cannot contain whitespaces.")]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}