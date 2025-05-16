using CulinaryCraftWeb.Data;
using CulinaryCraftWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

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
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string userId, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                // Check if the user has a profile image; if not, use the default image
                TempData["UserName"] = user.Name;
                TempData["ProfileImage"] = string.IsNullOrEmpty(user.ProfileImage) 
                    ? "/Images/default-profile.jpg" 
                    : user.ProfileImage;

                // Set success message for login
                TempData["SuccessMessage"] = "Login successful! Welcome, " + user.Name + ".";

                // Redirect to Home page after successful login
                return RedirectToAction("Index", "Home");
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
                    ProfileImage = null, // Default profile image
                    RegisteredDate = DateTime.Now,
                    Status = "active",
                    Role = "user"
                };

                _context.Users.Add(user);
                _context.SaveChanges();

                // Set success message
                TempData["SuccessMessage"] = "Your account has been created successfully.";

                // Redirect to the login page
                return RedirectToAction("Login");
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
        public IActionResult Logout()
        {
            // Clear TempData and any session/cookie if used
            TempData.Clear();
            // Optionally, clear authentication cookies or session here

            TempData["SuccessMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Index", "Home");
        }
    }
}