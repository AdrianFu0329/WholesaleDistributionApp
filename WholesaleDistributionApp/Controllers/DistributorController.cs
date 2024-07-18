using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using WholesaleDistributionApp.Areas.Identity.Data;
using WholesaleDistributionApp.Data;
using WholesaleDistributionApp.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WholesaleDistributionApp.Controllers
{
    public class DistributorController : Controller
    {
        private readonly WholesaleDistributionAppContext _context;
        private readonly UserManager<WholesaleDistributionAppUser> _userManager;
        private readonly ILogger<DistributorController> _logger;
        
        public DistributorController(WholesaleDistributionAppContext context, ILogger<DistributorController> logger, UserManager<WholesaleDistributionAppUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var userId = user!.Id;
                // Calculate total overall sales asynchronously
                var totalOverallSales = await _context.Orders
                    .Where(o => o.OrderType == "Warehouse" && o.OrderStatus == "Completed" && o.StockDistributorId == userId)
                    .SumAsync(o => o.TotalAmount);

                // Calculate monthly sales for chart asynchronously
                var monthlySales = await _context.Orders
                    .Where(o => o.OrderType == "Warehouse" && o.OrderStatus == "Completed" && o.StockDistributorId == userId)
                    .GroupBy(o => new { Year = o.OrderDate.Year, Month = o.OrderDate.Month })
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        TotalSales = g.Sum(o => o.TotalAmount),
                        DateLabel = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy")
                    })
                    .OrderBy(g => g.Year)
                    .ThenBy(g => g.Month)
                    .ToListAsync();

                // Prepare data for chart
                var chartData = new
                {
                    xaxis = monthlySales.Select(g => g.DateLabel),
                    series = new List<object> { new { name = "Total Sales", data = monthlySales.Select(g => g.TotalSales) } }
                };

                // Dashboard Cards
                var toAcceptCount = await _context.Orders.CountAsync(o => o.OrderType == "Warehouse" && o.OrderStatus == "Pending" && o.StockDistributorId == userId);
                var toShipCount = await _context.Orders.CountAsync(o => o.OrderType == "Warehouse" && o.OrderStatus == "Accepted" && o.StockDistributorId == userId);
                var pendingRefundCount = await _context.Orders.CountAsync(o => o.OrderStatus == "Pending Refund");
                var completedOrdersCount = await _context.Orders.CountAsync(o => o.OrderStatus == "Completed");
                var lowStockItems = await _context.WarehouseStock
                .Where(ws => ws.Quantity < 50)
                .Select(ws => new { ws.ItemName, ws.Quantity })
                .ToListAsync();
                var shippedItems = await _context.Orders
                .Where(o => o.OrderType == "Warehouse" && o.OrderStatus == "Shipped" && o.StockDistributorId == userId)
                .Select(o => new
                {
                    o.OrderId,
                    o.OrderDate
                })
                .ToListAsync();

                ViewBag.TotalOverallSales = totalOverallSales;
                ViewBag.ChartData = chartData;
                ViewBag.ToAcceptCount = toAcceptCount;
                ViewBag.ToShipCount = toShipCount;
                ViewBag.PendingRefundCount = pendingRefundCount;
                ViewBag.CompletedOrdersCount = completedOrdersCount;
                ViewBag.LowStockItems = lowStockItems;
                ViewBag.StockShippedItems = shippedItems;

                return View();
            }
            catch (Exception ex)
            {
                // Handle the exception (log it, etc.)
                // You can log the error using your logging mechanism here
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
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
                                 s.StockDistributorId == userId)
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

        public async Task<IActionResult> RefundManagement(string searchString)
        {
            // Load Distributor Refund Requests
            var refunds = _context.RefundRequest.Where(s =>
                                 s.RefundType == "Warehouse")
                                 .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                refunds = refunds.Where(s => s.OrderId.Contains(searchString));
            }

            // Sort by RequestDate in descending order
            refunds = refunds.OrderByDescending(r => r.RequestDate);

            return View(await refunds.ToListAsync());
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
                StockId = order.StockId != null ? order.StockId : "No id found",
                StockName = order.DistributorStock != null ? order.DistributorStock.ItemName : "Stock Name Not Available",
                StockImage = order.DistributorStock != null ? order.DistributorStock.ImgDownloadURL : null,
                order.Quantity,
                order.Subtotal
            }).ToList();

            _logger.LogInformation($"Order details retrieved successfully for orderIdentifier: {orderIdentifier}");
            return Json(new { success = true, orderDetails });
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
                    if (status == "Accepted" || status == "Cancelled")
                    {
                        var stockItems = _context.OrderDetails.Where(od => od.OrderId == orderId).ToList();
                        foreach (var item in stockItems)
                        {
                            var stock = _context.DistributorStock.Find(item.StockId);
                            if (status == "Accepted")
                            {
                                stock.Quantity -= item.Quantity;
                            }
                            else if (status == "Cancelled")
                            {
                                stock.Quantity += item.Quantity;
                            }
                            _context.DistributorStock.Update(stock);
                        }
                    }

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
        public IActionResult SubmitRefundRequest(double refundAmount, string orderId, string currentStatus)
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
                    RefundType = "Warehouse",
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRefundStatus([FromBody] UpdateRefundStatusRequest request)
        {
            var refund = await _context.RefundRequest.FindAsync(request.RefundId);
            if (refund == null)
            {
                return Json(new { success = false, message = "Refund not found." });
            }

            var order = await _context.Orders.FindAsync(Guid.Parse(refund.OrderId));
            if (order == null)
            {
                return Json(new { success = false, message = "Order not found." });
            }

            var orderDetails = await _context.OrderDetails
                .Where(od => od.OrderId == order.OrderId)
                .ToListAsync();
            if (orderDetails == null || !orderDetails.Any())
            {
                return Json(new { success = false, message = "Order Details not found." });
            }

            object stock = null;
            bool isStockFound = true;

            if (order.OrderType == "Warehouse")
            {
                stock = await _context.DistributorStock.FindAsync(orderDetails.FirstOrDefault()?.StockId);
                if (stock == null)
                {
                    isStockFound = false;
                }
            }
            else if (order.OrderType == "Retailer")
            {
                stock = await _context.WarehouseStock.FindAsync(orderDetails.FirstOrDefault()?.WarehouseStockId);
                if (stock == null)
                {
                    isStockFound = false;
                }
            }

            if (!isStockFound)
            {
                return Json(new { success = false, message = $"{order.OrderType} Stock not found." });
            }

            // Update refund status
            refund.RefundStatus = request.Status;
            //_logger.LogWarning($"{order.OrderStatus}");
            if (order.OrderStatus == "Refund Pending Accepted")
            {
                // Update order status based on refund status
                if (request.Status == "Approved")
                {
                    order.OrderStatus = "Cancelled";

                    // Update stock quantity
                    if (order.OrderType == "Warehouse")
                    {
                        var distributorStock = stock as DistributorStock;
                        if (distributorStock != null)
                        {
                            distributorStock.Quantity += orderDetails.Sum(od => od.Quantity);
                            _context.DistributorStock.Update(distributorStock);
                        }
                    }
                    else if (order.OrderType == "Retailer")
                    {
                        var warehouseStock = stock as WarehouseStock;
                        if (warehouseStock != null)
                        {
                            warehouseStock.Quantity += orderDetails.Sum(od => od.Quantity);
                            _context.WarehouseStock.Update(warehouseStock);
                        }
                    }
                }else if (request.Status == "Denied"){
                    order.OrderStatus = "Accepted";
                }
            } else if (order.OrderStatus == "Refund Pending Pending")
            {
                _logger.LogWarning($"Not Hehe");
                // Update order status based on refund status
                if (request.Status == "Approved")
                {
                    order.OrderStatus = "Cancelled";
                }
                else if (request.Status == "Denied")
                {
                    order.OrderStatus = "Accepted";
                }
            }



                try
            {
                _context.RefundRequest.Update(refund);
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error updating status: {ex.Message}" });
            }
        }

        public class UpdateRefundStatusRequest
        {
            public string RefundId { get; set; }
            public string Status { get; set; }
        }

        [HttpGet]
        public async Task<IActionResult> GetRefundStockDetails(string refundId)
        {
            // Assuming you have a context named _context
            var refundRequest = await _context.RefundRequest.FindAsync(refundId);
            if (refundRequest == null)
            {
                return Json(new { success = false, message = "Refund request not found." });
            }

            var orderDetails = await _context.OrderDetails
                .Where(od => od.OrderId.ToString() == refundRequest.OrderId)
                .FirstOrDefaultAsync();

            if (orderDetails == null)
            {
                return Json(new { success = false, message = "Order details not found." });
            }

            var stock = await _context.DistributorStock
                .Where(ds => ds.StockId == orderDetails.StockId)
                .FirstOrDefaultAsync();

            if (stock == null)
            {
                return Json(new { success = false, message = "Stock details not found." });
            }

            var order = await _context.Orders
                .Where(o => o.OrderId.ToString() == refundRequest.OrderId)
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return Json(new { success = false, message = "Order not found." });
            }

            string bankName = "Dummy Bank";
            string bankAccNum = "17834983746";
            string qrImage = "../assets/images/warehouse/QR-code.jpg";

            var response = new
            {
                success = true,
                stock = new
                {
                    itemName = stock.ItemName,
                    description = stock.Description,
                    imgDownloadURL = stock.ImgDownloadURL
                },
                orderDetails = new
                {
                    quantity = orderDetails.Quantity,
                    totalAmount = orderDetails.Subtotal
                },
                refundRequest = new
                {
                    refundBank = bankName,
                    bankAccNum = bankAccNum,
                    qrImage = qrImage
                }
            };

            return Json(response);
        }
    }
}
