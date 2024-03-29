using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using project.Data;
using project.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace project.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var orders = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .Where(o => o.UserId == userId && o.Status != OrderStatus.InCart)
                .ToListAsync();

            if (!orders.Any())
            {
                // Return an empty order list
                return View(Enumerable.Empty<project.Models.Order>());
            }

            return View(orders);
        }



        public async Task<IActionResult> Create()
        {
            ViewBag.Products = new SelectList(await _context.Products.ToListAsync(), "Id", "Name");
            return View(new OrderViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                var product = await _context.Products.FindAsync(model.ProductId);
                if (product == null)
                {
                    return NotFound();
                }

                var cart = await GetOrCreateCartAsync(userId);

                var orderItem = cart.Items.FirstOrDefault(i => i.ProductId == model.ProductId);
                if (orderItem != null)
                {
                    orderItem.Quantity += model.Quantity;
                }
                else
                {
                    cart.Items.Add(new OrderItem
                    {
                        ProductId = model.ProductId,
                        Quantity = model.Quantity,
                        PriceAtTimeOfOrder = product.Price
                    });
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("Cart");
            }

            ViewBag.Products = new SelectList(await _context.Products.ToListAsync(), "Id", "Name", model.ProductId);
            return View(model);
        }

        public async Task<IActionResult> Cart()
        {
            var userId = _userManager.GetUserId(User);
            var cart = await GetOrCreateCartAsync(userId);

            // We directly return the cart to the view, regardless of whether it's empty or not
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            var userId = _userManager.GetUserId(User);
            var cart = await GetOrCreateCartAsync(userId);

            var orderItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (orderItem != null)
            {
                orderItem.Quantity += quantity;
            }
            else
            {
                var priceAtTimeOfOrder = await _context.Products
                    .Where(p => p.Id == productId)
                    .Select(p => p.Price)
                    .FirstOrDefaultAsync();

                if (priceAtTimeOfOrder <= 0)
                {
                    // Handle the error appropriately if the product price is not found or is zero
                    // For example, return an error view or add a model error and return to the current view
                    return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
                }

                cart.Items.Add(new OrderItem
                {
                    ProductId = productId,
                    Quantity = quantity,
                    PriceAtTimeOfOrder = priceAtTimeOfOrder
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Cart");
        }


        public async Task<IActionResult> Checkout()
        {
            var userId = _userManager.GetUserId(User);
            var cart = await GetOrCreateCartAsync(userId);

            if (cart == null || !cart.Items.Any())
            {
                return View("Cart");
            }

            cart.Status = OrderStatus.Completed;
            cart.OrderUpdated = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return RedirectToAction("Cart");
        }

        public async Task<IActionResult> ClearCart()
        {
            var userId = _userManager.GetUserId(User);
            var cart = await GetOrCreateCartAsync(userId);

            _context.OrderItems.RemoveRange(cart.Items);
            _context.Orders.Remove(cart);
            await _context.SaveChangesAsync();

            return RedirectToAction("Cart");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCart(int productId, int quantity)
        {
            var userId = _userManager.GetUserId(User);
            var cart = await GetOrCreateCartAsync(userId);

            var orderItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (orderItem != null && quantity > 0)
            {
                orderItem.Quantity = quantity;
                _context.Update(orderItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Cart");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var userId = _userManager.GetUserId(User);
            var cart = await GetOrCreateCartAsync(userId);

            var orderItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (orderItem != null)
            {
                cart.Items.Remove(orderItem);
                _context.OrderItems.Remove(orderItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Cart");
        }

        

        private async Task<Order> GetOrCreateCartAsync(string userId)
        {
            var cart = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .SingleOrDefaultAsync(o => o.UserId == userId && o.Status == OrderStatus.InCart);

            if (cart == null)
            {
                cart = new Order
                {
                    UserId = userId,
                    OrderPlaced = DateTime.UtcNow,
                    Status = OrderStatus.InCart
                };
                _context.Orders.Add(cart);
            }

            return cart;
        }
    }
}
