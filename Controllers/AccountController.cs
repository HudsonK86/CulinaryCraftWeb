using CulinaryCraftWeb.Data;
using CulinaryCraftWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace CulinaryCraftWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string userId, string password, string returnUrl = null)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user != null)
            {
                if (user.Status != "active")
                {
                    ModelState.AddModelError("", "Your account is blocked. Please contact the administrator for assistance.");
                    return View();
                }

                if (BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    TempData["SuccessMessage"] = "Login successful! Welcome, " + user.Name + ".";

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Name),
                        new Claim(ClaimTypes.NameIdentifier, user.Id),
                        new Claim(ClaimTypes.Role, user.Role == "admin" ? "admin" : "user"),
                        new Claim("ProfileImage", user.ProfileImage)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, "MyCookieAuth");
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true
                    };

                    await HttpContext.SignInAsync("MyCookieAuth", new ClaimsPrincipal(claimsIdentity), authProperties);

                    // Use the helper to check if returnUrl is allowed
                    if (IsReturnUrlAllowed(returnUrl))
                        return Redirect(returnUrl);

                    return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError("", "Invalid User ID or Password");
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if the User ID already exists
                if (_context.Users.Any(u => u.Id == model.Id))
                {
                    ModelState.AddModelError("Id", "User ID is already taken.");
                    return View(model);
                }

                // Check if the Email already exists
                if (_context.Users.Any(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email is already registered.");
                    return View(model);
                }

                // Add the user to the database
                var user = new User
                {
                    Id = model.Id,
                    Name = model.Name,
                    Email = model.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password), // Hash the password
                    ProfileImage = "default-profile.jpg", // Default profile image
                    RegisteredDate = DateTime.Now,
                    Status = "active",
                    Role = "user"
                };

                _context.Users.Add(user);
                _context.SaveChanges();

                // Set success message
                TempData["SuccessMessage"] = "Your account has been created successfully.";

                // Redirect to the login page
                return RedirectToAction("Login", "Account", new { returnUrl = Request.Path + Request.QueryString });
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        public IActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if the new password and confirm password match
            if (model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
                return View(model);
            }

            // Verify if the user ID and email match a user in the database
            var user = _context.Users.FirstOrDefault(u => u.Id == model.UserId && u.Email == model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid User ID or Email.");
                return View(model);
            }

            // Update the user's password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            _context.SaveChanges();

            // Set success message and redirect to login page
            TempData["SuccessMessage"] = "Your password has been reset successfully.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public async Task<IActionResult> Logout(string returnUrl = null)
        {
            await HttpContext.SignOutAsync("MyCookieAuth");
            TempData["SuccessMessage"] = "You have been logged out successfully.";

            // Use the helper to check if returnUrl is allowed
            if (IsReturnUrlAllowed(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public IActionResult Dashboard()
        {
            return View();
        }

        private bool IsReturnUrlAllowed(string returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                return false;

            // List of pages you want to ignore as returnUrl
            var ignoredPaths = new[] { "/Account/Register", "/Account/ForgotPassword" };

            // You can add more ignored paths if needed
            return !ignoredPaths.Any(path => returnUrl.StartsWith(path, StringComparison.OrdinalIgnoreCase));
        }
    }
}