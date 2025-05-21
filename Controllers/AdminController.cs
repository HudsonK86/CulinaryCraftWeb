using CulinaryCraftWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            var usersQuery = _context.Users
                .Where(u => u.Role != "admin");

            var totalUsers = usersQuery.Count();
            var users = usersQuery
                .OrderBy(u => u.Name)
                .Take(PageSize)
                .ToList();

            ViewBag.CurrentPage = 1;
            ViewBag.TotalPages = (int)Math.Ceiling(totalUsers / (double)PageSize);
            ViewBag.Search = "";

            return View(users);
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

        [HttpGet]
        public IActionResult ManageRecipes(int page = 1, string search = "")
        {
            int pageSize = 10;
            IQueryable<Recipe> query = _context.Recipes.Include(r => r.Cuisine);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(r => r.Name.Contains(search) || r.Content.Contains(search));
            }

            var totalRecipes = query.Count();
            var totalPages = (int)Math.Ceiling(totalRecipes / (double)pageSize);

            var recipes = query
                .OrderByDescending(r => r.Created_At)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Search = search;

            return PartialView("~/Views/Admin/ManageRecipesPartial.cshtml", recipes);
        }

        [HttpPost]
        public IActionResult ApproveRecipe(int id)
        {
            var recipe = _context.Recipes.Find(id);
            if (recipe != null && recipe.Status == "pending")
            {
                recipe.Status = "approved";
                _context.SaveChanges();
            }
            return Ok();
        }

        [HttpPost]
        public IActionResult RejectRecipe(int id)
        {
            var recipe = _context.Recipes.Find(id);
            if (recipe != null && recipe.Status == "pending")
            {
                _context.Recipes.Remove(recipe);
                _context.SaveChanges();
            }
            return Ok();
        }

        [HttpPost]
        public IActionResult DeleteRecipe(int id)
        {
            var recipe = _context.Recipes.Find(id);
            if (recipe != null)
            {
                _context.Recipes.Remove(recipe);
                _context.SaveChanges();
            }
            return Ok();
        }
    }
}