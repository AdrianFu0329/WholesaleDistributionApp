using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;
using WholesaleDistributionApp.Areas.Identity.Data;
using WholesaleDistributionApp.Data;
using WholesaleDistributionApp.Models;
using WholesaleDistributionApp.Services;

namespace WholesaleDistributionApp.Controllers
{
    public class RetailerController : Controller
    {
        private readonly WholesaleDistributionAppContext _context;
        private readonly UserManager<WholesaleDistributionAppUser> _userManager;
        private readonly ILogger<RetailerController> _logger;
        private readonly S3Service _s3Service;
        public RetailerController(WholesaleDistributionAppContext context, UserManager<WholesaleDistributionAppUser> userManager, ILogger<RetailerController> logger, S3Service s3Service)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _s3Service = s3Service;
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
                .Include(od => od.WarehouseStock)
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
                StockId = order.StockId != null ? order.StockId : "No id found",
                StockName = order.WarehouseStock != null ? order.WarehouseStock.ItemName : "Stock Name Not Available",
                StockImage = order.WarehouseStock != null ? order.WarehouseStock.ImgDownloadURL : null,
                order.Quantity,
                order.Subtotal
            }).ToList();

            _logger.LogInformation($"Order details retrieved successfully for orderIdentifier: {orderIdentifier}");
            return Json(new { success = true, orderDetails });
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
                var fileName = Path.GetFileName(proofOfPayment.FileName);
                string filePath = null;

                if (proofOfPayment != null && proofOfPayment.Length > 0)
                {
                    _logger.LogInformation("Image file is not null, proceeding with upload.");

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + fileName;
                    var s3Key = Path.Combine("proofOfPayment", uniqueFileName).Replace("\\", "/");

                    var tempFilePath = Path.Combine(Path.GetTempPath(), uniqueFileName);
                    using (var stream = new FileStream(tempFilePath, FileMode.Create))
                    {
                        await proofOfPayment.CopyToAsync(stream);
                    }

                    _logger.LogInformation($"Uploading file {tempFilePath} to S3 key {s3Key}");

                    var uploadSuccess = await _s3Service.UploadFileAsync(s3Key, tempFilePath);
                    if (uploadSuccess)
                    {
                        var s3Url = _s3Service.GetFileUrl(s3Key);
                        filePath = s3Url;
                        _logger.LogInformation($"File uploaded successfully to {filePath}");

                        if (System.IO.File.Exists(tempFilePath))
                        {
                            System.IO.File.Delete(tempFilePath);
                        }
                    }
                    else
                    {
                        _logger.LogError("Failed to upload file to S3.");
                    }
                }

                // Create new Order entity
                var order = new Order
                {
                    OrderId = Guid.NewGuid(),
                    OrderDate = DateTime.Now,
                    RetailerId = userId,
                    TotalAmount = orderDetailsList.Sum(od => od.Subtotal),
                    OrderStatus = "Pending",
                    OrderType = "Retailer",
                    StockDistributorId = "-"
                };

                if (filePath != null)
                {
                    order.PaymentReceiptURL = filePath;
                }

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

        public async Task<IActionResult> OrderManagement(string searchString, string category)
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user!.Id;

            if (userId == null)
            {
                return Unauthorized();
            }

            // Load Stocks for the current Distributor
            var orders = _context.Orders
                                 .Where(s =>
                                 s.RetailerId == userId)
                                 .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                orders = orders.Where(od => od.OrderId.ToString().Contains(searchString));
            }

            if (!string.IsNullOrEmpty(category) && category != "All")
            {
                orders = orders.Where(o => o.OrderStatus == category);
            }

            return View(orders.ToList());
        }

        [HttpPost]
        public IActionResult UpdateOrderStatus(Guid orderId, string status)
        {
            try
            {
                // Retrieve the order from the database based on orderId
                var orderToUpdate = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);

                if (orderToUpdate != null)
                {
                    /*if (status == "Completed")
                    {
                        var stockItems = _context.OrderDetails.Where(od => od.OrderId == orderId).ToList();
                        foreach (var item in stockItems)
                        {
                            var stock = _context.WarehouseStock.FirstOrDefault(s => s.DistributorStockId == item.StockId);
                            if (status == "Completed")
                            {
                                if (stock == null)
                                {
                                    //get stock details by stock id
                                    var newStockDetails = _context.DistributorStock.Find(item.StockId);
                                    //add new stock
                                    var newStock = new WarehouseStock
                                    {
                                        StockId = Guid.NewGuid().ToString(),
                                        DistributorStockId = newStockDetails.StockDistributorId, // Need to set here when purchase complete
                                        ItemName = newStockDetails.ItemName,
                                        Description = newStockDetails.Description,
                                        Quantity = item.Quantity,
                                        SinglePrice = newStockDetails.SinglePrice,
                                        StockDistributorId = newStockDetails.StockId,
                                        ImgDownloadURL = newStockDetails.ImgDownloadURL,
                                        ForRetailerPurchase = false,
                                    };

                                    _context.WarehouseStock.Add(newStock);

                                }
                                else
                                {
                                    stock.Quantity += item.Quantity;
                                    _context.WarehouseStock.Update(stock);
                                }
                            }
                        }
                    }*/

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

        [HttpPost]
        public IActionResult SubmitRefundRequest(double refundAmount, string orderId)
        {
            try
            {
                // Generate new GUID for RefundId
                var refundId = Guid.NewGuid();

                // Create new RefundRequest object
                var refundRequest = new RefundRequest
                {
                    RefundId = refundId.ToString(),
                    RequestDate = DateTime.UtcNow,
                    RefundAmount = refundAmount,
                    RefundStatus = "Pending",
                    RefundType = "Retailer",
                    OrderId = orderId
                };

                // Add to DbContext and save changes
                _context.RefundRequest.Add(refundRequest);
                _context.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
