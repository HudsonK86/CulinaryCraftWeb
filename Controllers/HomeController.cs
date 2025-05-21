using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CulinaryCraftWeb.Models;
using CulinaryCraftWeb.Data; // Add this using directive for the data context

namespace CulinaryCraftWeb.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context; // Add a field for the data context

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context) // Inject the data context
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult Contact()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult FeaturedRecipes(int page = 1, string search = "")
    {
        int pageSize = 6;
        var query = _context.Recipes
            .Where(r => r.Status == "approved");

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(r => r.Name.Contains(search) || r.Content.Contains(search));
        }

        var recipes = query
            .OrderByDescending(r => r.Created_At)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        int totalRecipes = query.Count();
        int totalPages = (int)Math.Ceiling(totalRecipes / (double)pageSize);

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;

        return PartialView("FeaturedRecipesPartial", recipes);
    }
}
