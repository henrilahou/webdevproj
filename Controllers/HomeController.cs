using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project.Data;
using project.Models;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace project.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager; // Corrected UserManager assignment
        }


        // Extend the Index action to include a search feature
        public async Task<IActionResult> Index(string searchString)
        {
            // Add user information to the view via ViewBag
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                ViewBag.CurrentUserName = user.UserName;
                ViewBag.IsAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            }

            IQueryable<Product> productsQuery = _context.Products;

            if (!string.IsNullOrEmpty(searchString))
            {
                productsQuery = productsQuery.Where(p => p.Name.Contains(searchString));
            }

            var products = await productsQuery.ToListAsync();
            return View(products);
        }
        public IActionResult About()
        {
            // Informatie over 'The Breadpit' en eventueel het team achter de broodjeszaak
            return View();
        }

        public IActionResult Contact()
        {
            // Een eenvoudige contactpagina. Voor een contactformulier zijn verdere uitbreidingen nodig
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
    }
}
