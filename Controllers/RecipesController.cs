using CulinaryCraftWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using CulinaryCraftWeb.Models; 

namespace CulinaryCraftWeb.Controllers
{
    [Authorize] // Only logged-in users can post
    public class RecipesController : Controller
    {
        private readonly ApplicationDbContext _context;
        public RecipesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // List recipes by cuisine
        public IActionResult Index(string cuisine)
        {
            if (string.IsNullOrEmpty(cuisine))
                return NotFound();

            var cuisineEntity = _context.Cuisines
                .Include(c => c.Recipes)
                .FirstOrDefault(c => c.Name == cuisine);

            if (cuisineEntity == null)
                return NotFound();

            var recipes = cuisineEntity.Recipes
                .Where(r => r.Status == "approved")
                .ToList();

            ViewBag.CuisineName = cuisineEntity.Name;
            ViewBag.CuisineImage = cuisineEntity.Image;
            return View(recipes);
        }

        // Optional: Details page for a recipe
        public IActionResult Details(int id)
        {
            var recipe = _context.Recipes
                .Include(r => r.Cuisine)
                .FirstOrDefault(r => r.Id == id);

            if (recipe == null)
                return NotFound();

            // If not approved, only allow the creator (or admin) to view
            if (recipe.Status != "approved")
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var isAdmin = User.IsInRole("admin");
                if (recipe.Created_By != userId && !isAdmin)
                    return Forbid();
            }

            return View("~/Views/Home/Recipes/Details.cshtml", recipe);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Cuisines = _context.Cuisines.ToList();
            return View("~/Views/Home/Recipes/Create.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(PostRecipeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Cuisines = _context.Cuisines.ToList();
                return View("~/Views/Home/Recipes/Create.cshtml", model);
            }

            if (!string.IsNullOrEmpty(model.Youtube_Link))
            {
                bool exists = _context.Recipes.Any(r => r.Youtube_Link == model.Youtube_Link);
                if (exists)
                {
                    ModelState.AddModelError("Youtube_Link", "This YouTube link has already been used for another recipe.");
                    ViewBag.Cuisines = _context.Cuisines.ToList();
                    return View("~/Views/Home/Recipes/Create.cshtml", model);
                }
            }

            // Handle image upload as before, using model.ImageFile
            var cuisine = _context.Cuisines.FirstOrDefault(c => c.Id == model.Cuisine_Id);
            var cuisineFolder = cuisine != null ? cuisine.Name : "Other";
            var uploadsFolder = Path.Combine("wwwroot", "Images", "Cuisines", cuisineFolder);

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid() + Path.GetExtension(model.ImageFile.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                model.ImageFile.CopyTo(stream);
            }

            // Create Recipe entity
            var recipe = new Recipe
            {
                Name = model.Name,
                Cuisine_Id = model.Cuisine_Id,
                Content = model.Content,
                Youtube_Link = model.Youtube_Link,
                Image = Path.Combine("Cuisines", cuisineFolder, fileName).Replace("\\", "/"),
                Status = "pending",
                Created_At = DateTime.Now,
                Created_By = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            };

            _context.Recipes.Add(recipe);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Recipe submitted! Awaiting approval.";
            return RedirectToAction("Index", "Home");
        }

        [Authorize(Roles = "user")]
        public IActionResult MyRecipes(int page = 1)
        {
            int pageSize = 10;
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var query = _context.Recipes
                .Where(r => r.Created_By == userId)
                .OrderByDescending(r => r.Created_At);

            var totalRecipes = query.Count();
            var totalPages = (int)Math.Ceiling(totalRecipes / (double)pageSize);

            var recipes = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return PartialView("~/Views/User/MyRecipesPartial.cshtml", recipes);
        }
    }
}