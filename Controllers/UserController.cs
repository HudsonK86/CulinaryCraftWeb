using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Security.Claims;
using CulinaryCraftWeb.Data;

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
                if (user != null)
                {
                    var uploads = Path.Combine(_env.WebRootPath, "Images");
                    var fileName = $"{userId}_{Path.GetFileName(profileImage.FileName)}";
                    var filePath = Path.Combine(uploads, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        profileImage.CopyTo(stream);
                    }

                    user.ProfileImage = fileName;
                    _context.SaveChanges();

                    ViewBag.SuccessMessage = "Profile image updated!";
                }
            }
            return PartialView("ChangeProfileImagePartial");
        }
    }
}