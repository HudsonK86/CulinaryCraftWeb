using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Security.Claims;
using CulinaryCraftWeb.Data;
using CulinaryCraftWeb.Models; // Add this using directive for the ViewModel

namespace CulinaryCraftWeb.Controllers
{
    [Authorize(Roles = "user")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public UserController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public IActionResult UserDashboard()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ChangeProfileImage()
        {
            return PartialView("ChangeProfileImagePartial");
        }

        [HttpPost]
        public IActionResult ChangeProfileImage(IFormFile profileImage)
        {
            if (profileImage != null && profileImage.Length > 0)
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = _context.Users.FirstOrDefault(u => u.Id == userId);

                // Save new image
                var uploads = Path.Combine(_env.WebRootPath, "Images");
                var fileName = $"{userId}_{Path.GetFileName(profileImage.FileName)}";
                var filePath = Path.Combine(uploads, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    profileImage.CopyTo(stream);
                }

                // Delete old image if not default
                if (!string.IsNullOrEmpty(user.ProfileImage) && user.ProfileImage != "default-profile.jpg")
                {
                    var oldImagePath = Path.Combine(uploads, user.ProfileImage);
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                user.ProfileImage = fileName;
                _context.SaveChanges();

                ViewBag.SuccessMessage = "Profile image updated!";
            }

            // If not AJAX, redirect to dashboard
            if (!Request.Headers["X-Requested-With"].Equals("XMLHttpRequest"))
            {
                TempData["SuccessMessage"] = "Profile image updated!";
                return RedirectToAction("UserDashboard");
            }

            return PartialView("ChangeProfileImagePartial");
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return PartialView("ChangePasswordPartial", new ChangePasswordViewModel());
        }

        [HttpPost]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            if (!ModelState.IsValid)
            {
                return PartialView("ChangePasswordPartial", model);
            }

            if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash))
            {
                ViewBag.ErrorMessage = "Current password is incorrect.";
                return PartialView("ChangePasswordPartial", model);
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            _context.SaveChanges();

            ViewBag.SuccessMessage = "Password changed successfully!";

            // If not AJAX, redirect to dashboard
            if (!Request.Headers["X-Requested-With"].Equals("XMLHttpRequest"))
            {
                TempData["ShowPartial"] = "ChangePassword";
                TempData["SuccessMessage"] = "Password changed successfully!";
                return RedirectToAction("UserDashboard");
            }

            return PartialView("ChangePasswordPartial", new ChangePasswordViewModel());
        }
    }
}