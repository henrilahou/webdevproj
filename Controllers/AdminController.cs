using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project.Models;
using project.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;


namespace project.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            // Potentially show an admin dashboard with statistics, user count, latest orders, etc.
            return View();
        }

        // In AdminController.cs

        // This is the missing ManageOrders action
        public async Task<IActionResult> ManageOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Where(o => o.Status == OrderStatus.Completed) // or whatever condition you want
                .ToListAsync();

            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View("NotFound");
            }

            user.EmailConfirmed = true; // Assuming EmailConfirmed is used for approval
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return RedirectToAction("UserApproval");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View("UserApproval");
        }

        [HttpPost]
        public async Task<IActionResult> AddRoleToUser(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && !await _userManager.IsInRoleAsync(user, roleName))
            {
                var result = await _userManager.AddToRoleAsync(user, roleName);
                if (result.Succeeded)
                {
                   // return Json(new { success = true });
                    return RedirectToAction(nameof(UserApproval));
                }
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveRoleFromUser(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && await _userManager.IsInRoleAsync(user, roleName))
            {
                var result = await _userManager.RemoveFromRoleAsync(user, roleName);
                if (result.Succeeded)
                {
                    //return Json(new { success = true });
                    return RedirectToAction(nameof(UserApproval));
                }
            }
            return Json(new { success = false });
        }

        public async Task<IActionResult> CreateUser(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Assign role here if necessary
                    await _userManager.AddToRoleAsync(user, "Admin");
                }
            }
            // Return a view if model state is not valid or after creating a user
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveAdmin(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Admin");
            }
            return RedirectToAction(nameof(UserApproval));
        }



        public async Task<IActionResult> ManageRoles()
        {
            var roles = _roleManager.Roles.Select(r => r.Name).ToList();
            var users = _userManager.Users.ToList();
            var userRolesViewModel = new List<UserRolesViewModel>();

            foreach (ApplicationUser user in users)
            {
                var thisViewModel = new UserRolesViewModel
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Roles = await GetUserRoles(user),
                    AvailableRoles = roles,
                    IsAwaitingApproval = !user.EmailConfirmed
                };
                userRolesViewModel.Add(thisViewModel);
            }
            return View(userRolesViewModel);
        }


        private async Task<List<string>> GetUserRoles(ApplicationUser user)
        {
            return new List<string>(await _userManager.GetRolesAsync(user));
        }

        public async Task<IActionResult> EditRoles(string userId)
        {
            ViewBag.userId = userId;
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {userId} cannot be found";
                return View("NotFound");
            }

            var model = new List<ManageUserRolesViewModel>();

            foreach (var role in _roleManager.Roles)
            {
                var userRolesViewModel = new ManageUserRolesViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name
                };
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    userRolesViewModel.Selected = true;
                }
                else
                {
                    userRolesViewModel.Selected = false;
                }

                model.Add(userRolesViewModel);
            }

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CompletedOrders()
        {
            var completedOrders = await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Where(o => o.Status == OrderStatus.Completed)
                .ToListAsync();

            return View(completedOrders);
        }


        [HttpPost]
        public async Task<IActionResult> EditRoles(List<ManageUserRolesViewModel> model, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View("NotFound");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, roles);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove user existing roles");
                return View(model);
            }

            result = await _userManager.AddToRolesAsync(user, model.Where(x => x.Selected).Select(y => y.RoleName));

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot add selected roles to user");
                return View(model);
            }

            return RedirectToAction("ManageRoles");
        }

        public async Task<IActionResult> Products()
        {
            var products = await _context.Products.ToListAsync();
            return View(products);
        }

        public IActionResult AddProduct()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ManageProducts));
            }
            // If we got this far, something failed; redisplay the form.
            return View(product);
        }

        public async Task<IActionResult> EditProduct(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        public async Task<IActionResult> ManageProducts()
        {
            var products = await _context.Products.ToListAsync();
            return View(products);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var productToUpdate = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
                if (productToUpdate != null)
                {
                    productToUpdate.Name = product.Name;
                    productToUpdate.Price = product.Price;
                    productToUpdate.Description = product.Description;
                    productToUpdate.Stock = product.Stock;
                    productToUpdate.Category = product.Category;
                    // Add any additional fields you need to update here

                    try
                    {
                        _context.Update(productToUpdate);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(ManageProducts)); // Redirect to the ManageProducts page
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        ModelState.AddModelError("", "Unable to save changes. " +
                            "Try again, and if the problem persists, " +
                            "see your system administrator.");
                    }
                }
            }

            // If we get here, something went wrong, re-show form
            return View(product);
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }


        public async Task<IActionResult> UserApproval()
        {
            // var users = await _userManager.Users.ToListAsync();
            var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            var users = _userManager.Users.ToList();
            var userRolesViewModelList = new List<UserRolesViewModel>();
            //foreach (var user in users)
            //{
            //    var thisViewModel = new UserRolesViewModel
            //    {
            //        UserId = user.Id,
            //        Email = user.Email,
            //        Roles = await GetUserRoles(user),
            //        AvailableRoles = new List<string>(), // You need to populate this if you use it in the view
            //        IsAwaitingApproval = !user.EmailConfirmed // Assuming this is what you mean by awaiting approval
            //    };
            //    userRolesViewModelList.Add(thisViewModel);
            //}
            foreach (ApplicationUser user in users)
            {
                var thisViewModel = new UserRolesViewModel
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Roles = await GetUserRoles(user),
                    AvailableRoles = roles, // Make sure this list is populated
                    IsAwaitingApproval = !user.EmailConfirmed
                };
                userRolesViewModelList.Add(thisViewModel);
            }
            return View(userRolesViewModelList); // Make sure to pass the correct model type
        }




        [HttpGet]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    // Add logic for a successful deletion (redirect or a success message)
                    return RedirectToAction(nameof(UserApproval));
                }
            }
            // Add logic for a failed deletion
            return View("Error");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ManageProducts");
        }


        public async Task<IActionResult> ViewOrders()
        {
            var orders = await _context.Orders.Include(o => o.Items).ThenInclude(i => i.Product).ToListAsync();
            return View(orders);
        }
    }
}
