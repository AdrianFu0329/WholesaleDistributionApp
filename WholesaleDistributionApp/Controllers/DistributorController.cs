using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WholesaleDistributionApp.Areas.Identity.Data;
using WholesaleDistributionApp.Data;
using WholesaleDistributionApp.Models;

namespace WholesaleDistributionApp.Controllers
{
    public class DistributorController : Controller
    {
        private readonly WholesaleDistributionAppContext _context;
        private readonly UserManager<WholesaleDistributionAppUser> _userManager;
        private readonly ILogger<AdminController> _logger;

        public DistributorController(WholesaleDistributionAppContext context, ILogger<AdminController> logger, UserManager<WholesaleDistributionAppUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<IActionResult> MyStock(string searchString)
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user!.Id;

            if (userId == null)
            {
                return Unauthorized();
            }

            // Load Stocks for the current Distributor
            var stocks = _context.DistributorStock
                                 .Where(s => s.StockDistributorId == userId)
                                 .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                stocks = stocks.Where(s => s.ItemName.Contains(searchString) ||
                                           s.Description.Contains(searchString));
            }

            return View(stocks.ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStock(AddStockViewModel viewModel, IFormFile ImageFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string imgDownloadURL = null;

                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        var fileName = Path.GetFileName(ImageFile.FileName);
                        var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

                        // Ensure the directory exists
                        if (!Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }

                        var filePath = Path.Combine(directoryPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(stream);
                        }

                        imgDownloadURL = $"/images/{fileName}";
                    }

                    var user = await _userManager.GetUserAsync(User);
                    var userId = user!.Id;

                    var stock = new DistributorStock
                    {
                        StockId = Guid.NewGuid().ToString(),
                        ItemName = viewModel.ItemName,
                        Description = viewModel.Description,
                        Quantity = viewModel.Quantity,
                        SinglePrice = viewModel.SinglePrice,
                        StockDistributorId = userId,
                        ImgDownloadURL = imgDownloadURL,
                        DistributorDeliveryStatus = "Ready for Acquisition"
                    };

                    _context.DistributorStock.Add(stock);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    // Log the exception or handle it appropriately
                    return Json(new { success = false, message = ex.Message });
                }
            }

            // If ModelState is not valid, return the validation errors
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning($"Validation failed while adding stock. Errors: {string.Join("; ", errors)}");
            return Json(new { success = false, message = "Validation failed", errors });
        }

        public async Task<IActionResult> GetStockDetails(string stockIdentifier)
        {
            _logger.LogInformation($"GetStockDetails called with stockIdentifier: {stockIdentifier}");

            if (string.IsNullOrEmpty(stockIdentifier))
            {
                _logger.LogWarning("Stock identifier is required but was not provided.");
                return Json(new { success = false, message = "Stock identifier is required." });
            }

            stockIdentifier = stockIdentifier.ToLower(); // Convert to lower case for case-insensitive comparison

            var stock = await _context.DistributorStock.FirstOrDefaultAsync(s =>
                s.StockId.ToLower() == stockIdentifier ||
                s.ItemName.ToLower() == stockIdentifier ||
                s.StockDistributorId.ToLower() == stockIdentifier);

            if (stock == null)
            {
                _logger.LogWarning($"Stock not found for stockIdentifier: {stockIdentifier}");
                return Json(new { success = false, message = "Stock not found." });
            }

            _logger.LogInformation($"Stock details retrieved successfully for stockIdentifier: {stockIdentifier}");
            return Json(new { success = true, stock });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStock([FromForm] UpdateStockModel model, IFormFile ImageFile)
        {
            if (model == null || string.IsNullOrEmpty(model.StockId))
            {
                return Json(new { success = false, message = "Invalid stock data." });
            }

            var stock = await _context.DistributorStock.FindAsync(model.StockId);
            if (stock == null)
            {
                return Json(new { success = false, message = "Stock not found." });
            }

            // Update stock details
            stock.ItemName = model.ItemName;
            stock.Description = model.Description;
            if (model.Quantity.HasValue)
            {
                stock.Quantity = model.Quantity.Value;
            }
            else
            {
                stock.Quantity = 0;
            }
            stock.SinglePrice = model.SinglePrice;
            stock.DistributorDeliveryStatus = model.DistributorDeliveryStatus;

            try
            {
                // Handle image upload
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var fileName = Path.GetFileName(ImageFile.FileName);
                    var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

                    // Ensure the directory exists
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    var filePath = Path.Combine(directoryPath, fileName);

                    // Delete previous image if it exists
                    if (!string.IsNullOrEmpty(stock.ImgDownloadURL))
                    {
                        var previousImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", stock.ImgDownloadURL.TrimStart('/'));
                        if (System.IO.File.Exists(previousImagePath))
                        {
                            System.IO.File.Delete(previousImagePath);
                        }
                    }

                    // Save new image
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    var imgDownloadURL = $"/images/{fileName}";
                    stock.ImgDownloadURL = imgDownloadURL;
                }

                _context.DistributorStock.Update(stock);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                return Json(new { success = false, message = $"Error updating stock: {ex.Message}" });
            }
        }

        private bool StockExists(Guid id)
        {
            return _context.DistributorStock.Any(e => e.StockId.Equals(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStock(string stockId)
        {
            if (string.IsNullOrEmpty(stockId))
            {
                return Json(new { success = false, message = "Invalid stock ID" });
            }

            var stock = await _context.DistributorStock.FindAsync(stockId);
            if (stock == null)
            {
                return Json(new { success = false, message = "Stock not found" });
            }

            _context.DistributorStock.Remove(stock);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Stock deleted successfully" });
        }

        public async Task<IActionResult> OrderManagement()
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user!.Id;

            if (userId == null)
            {
                return Unauthorized();
            }

            // Load Stocks for the current Distributor
            var orders = _context.Orders
                                 .Where(s => s.StockDistributorId == userId)
                                 .AsQueryable();

            //if (!string.IsNullOrEmpty(searchString))
            //{
              //  orders= orders.Where(s => s.ItemName.Contains(searchString) ||
               //                            s.Description.Contains(searchString));
            //}

            return View(orders.ToList());
        }

        public async Task<IActionResult> GetOrderDetails(string orderIdentifier)
        {
            _logger.LogInformation($"GetOrderDetails called with orderIdentifier: {orderIdentifier}");

            if (string.IsNullOrEmpty(orderIdentifier))
            {
                _logger.LogWarning("Order identifier is required but was not provided.");
                return Json(new { success = false, message = "Order identifier is required." });
            }

            orderIdentifier = orderIdentifier.ToLower();

            var orders = await _context.OrderDetails
                .Include(od => od.DistributorStock)
                .Where(od => od.OrderId.ToString().ToLower() == orderIdentifier)
                .ToListAsync();

            if (orders == null || !orders.Any())
            {
                _logger.LogWarning($"Order not found for orderIdentifier: {orderIdentifier}");
                return Json(new { success = false, message = "Order not found." });
            }

            var orderDetails = orders.Select(order => new
            {
                order.OrderId,
                order.OrderDetailsId,
                order.StockId,
                StockName = order.DistributorStock != null ? order.DistributorStock.ItemName : "Stock Name Not Available",
                StockImage = order.DistributorStock != null ? order.DistributorStock.ImgDownloadURL : null,
                order.Quantity,
                order.Subtotal
            }).ToList();

            _logger.LogInformation($"Order details retrieved successfully for orderIdentifier: {orderIdentifier}");
            return Json(new { success = true, orderDetails });
        }

        [HttpPost]
        public IActionResult UpdateOrderStatus(int orderId, string status)
        {
            try
            {
                // Retrieve the order from the database based on orderId
                var orderToUpdate = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);

                if (orderToUpdate != null)
                {
                    // Update the order status
                    orderToUpdate.OrderStatus = status;

                    // Save changes to the database
                    _context.SaveChanges();

                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Order not found." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
