using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CulinaryCraftWeb.Models
{
    public class RegisterViewModel
    {
        [Required]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Name cannot contain numeric values or special characters.")]
        public string Name { get; set; }

        [Required]
        [RegularExpression(@"^\S+$", ErrorMessage = "User ID cannot contain whitespaces.")]
        public string Id { get; set; } // User ID

        [Required]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
}