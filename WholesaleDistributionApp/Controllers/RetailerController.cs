using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;
using WholesaleDistributionApp.Areas.Identity.Data;
using WholesaleDistributionApp.Data;
using WholesaleDistributionApp.Models;

namespace WholesaleDistributionApp.Controllers
{
    public class RetailerController : Controller
    {
        private readonly WholesaleDistributionAppContext _context;

        public RetailerController(WholesaleDistributionAppContext context)
        {
            _context = context;
        }
        public IActionResult ViewProducts(string searchString)
        {
            // Load Products
            var stocks = _context.WarehouseStock
                .Include(s => s.Distributor)
                .Where(s => s.ForRetailerPurchase == true)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                stocks = stocks.Where(s => s.ItemName.Contains(searchString) ||
                                           s.Description.Contains(searchString) ||
                                           s.StockDistributorId.Contains(searchString));
            }

            return View(stocks.ToList());
        }

        [HttpPost]
        public async Task<IActionResult> SubmitPayment([FromForm] string orderDetails, [FromForm] IFormFile proofOfPayment)
        {
            // Check if order details are provided
            if (string.IsNullOrEmpty(orderDetails))
            {
                return Json(new { success = false, message = "No items selected for order." });
            }

            // Retrieve current user's ID (assuming using Identity)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "User is not authenticated." });
            }

            // Deserialize order details JSON into a list of OrderDetails objects
            var orderDetailsList = JsonConvert.DeserializeObject<List<OrderDetails>>(orderDetails);

            // Validate order details
            if (orderDetailsList == null || orderDetailsList.Count == 0)
            {
                return Json(new { success = false, message = "No items selected for order." });
            }

            foreach (var detail in orderDetailsList)
            {
                if (detail.Quantity <= 0)
                {
                    return Json(new { success = false, message = "Quantity must be greater than 0." });
                }
            }

            // Check if proof of payment is provided
            if (proofOfPayment == null || proofOfPayment.Length == 0)
            {
                return Json(new { success = false, message = "Proof of payment is required." });
            }

            // Validate file type and size for proof of payment
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
            var fileExtension = Path.GetExtension(proofOfPayment.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return Json(new { success = false, message = "Invalid file type. Only JPG, JPEG, PNG, and PDF files are allowed." });
            }

            if (proofOfPayment.Length > 5 * 1024 * 1024) // 5MB size limit
            {
                return Json(new { success = false, message = "File size exceeds 5MB limit." });
            }

            try
            {
                // Save proof of payment file to the server
                var fileName = Path.GetFileName(proofOfPayment.FileName);
                var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "proofOfPayments");

                // Ensure the directory exists
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                var filePath = Path.Combine(directoryPath, fileName);

                // Save new proof of payment
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await proofOfPayment.CopyToAsync(stream);
                }

                var imgDownloadURL = $"/proofOfPayments/{fileName}";

                // Create new Order entity
                var order = new Order
                {
                    OrderId = Guid.NewGuid(),
                    OrderDate = DateTime.Now,
                    RetailerId = userId,
                    TotalAmount = orderDetailsList.Sum(od => od.Subtotal),
                    OrderStatus = "Pending",
                    OrderType = "Retailer",
                    PaymentReceiptURL = imgDownloadURL
                };

                // Save order to database
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Create OrderDetail entities and associate with the order
                foreach (var detail in orderDetailsList)
                {
                    detail.OrderId = order.OrderId;
                    _context.OrderDetails.Add(detail);
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Failed to submit order: {ex.Message}" });
            }
        }
    }
}
