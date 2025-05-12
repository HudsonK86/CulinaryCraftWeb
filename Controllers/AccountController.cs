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
            // TODO: Add authentication logic here
            if (userId == "admin" && password == "password") // Example logic
            {
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
                    ProfileImage = "none", // Default value
                    RegisteredDate = DateTime.Now,
                    Status = "active",
                    Role = "user"
                };

                _context.Users.Add(user);
                _context.SaveChanges();

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

            if (model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
                return View(model);
            }

            // TODO: Add logic to verify the User ID and email, and update the password in the database.

            TempData["SuccessMessage"] = "Your password has been reset successfully.";
            return RedirectToAction("Login");
        }
    }
}