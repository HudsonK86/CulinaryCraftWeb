using CulinaryCraftWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace CulinaryCraftWeb.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 10;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult AdminDashboard()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ManageUsers(int page = 1, string search = "")
        {
            var usersQuery = _context.Users
                .Where(u => u.Role != "admin"); // Exclude admin accounts

            if (!string.IsNullOrWhiteSpace(search))
            {
                usersQuery = usersQuery.Where(u =>
                    u.Name.Contains(search) ||
                    u.Email.Contains(search) ||
                    u.Id.Contains(search));
            }

            var totalUsers = usersQuery.Count();
            var users = usersQuery
                .OrderBy(u => u.Name)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalUsers / (double)PageSize);
            ViewBag.Search = search;

            return PartialView("ManageUsersPartial", users);
        }

        [HttpPost]
        public IActionResult ToggleUserStatus(string id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                user.Status = user.Status == "active" ? "inactive" : "active";
                _context.SaveChanges();
            }
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult DeleteUser(string id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
            return Json(new { success = true });
        }

        public IActionResult ManageRecipes()
        {
            // You can pass a list of recipes from the database here
            return PartialView("ManageRecipesPartial");
        }
    }
}