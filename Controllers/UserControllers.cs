using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CulinaryCraftWeb.Controllers
{
    [Route("[controller]")]
    public class UserControllers : Controller
    {
        private readonly ILogger<UserControllers> _logger;

        public UserControllers(ILogger<UserControllers> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }

        public IActionResult Dashboard()
        {
            // Optionally pass user info to the view
            return View();
        }

        public IActionResult AccountSettings()
        {
            // Load current user info for editing
            return View();
        }

        [HttpPost]
        
        
        public IActionResult MyRecipes()
        {
            // Load user's recipes
            return View();
        }

        public IActionResult SomeAction()
        {
            return View();
        }
    }
}