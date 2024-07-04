using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WholesaleDistributionApp.Areas.Identity.Data;
using WholesaleDistributionApp.Data;
using WholesaleDistributionApp.Models;

namespace WholesaleDistributionApp.Controllers
{
    //[Authorize(Roles = "Admin")]

    public class AdminController : Controller
    {
        private readonly WholesaleDistributionAppContext _context;
        private readonly UserManager<WholesaleDistributionAppUser> _userManager;
        private readonly IUserStore<WholesaleDistributionAppUser> _userStore;
        private readonly IUserEmailStore<WholesaleDistributionAppUser> _emailStore;
        private readonly ILogger<AdminController> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly FileService _fileService;
        private readonly StockService _stockService;

        // Constructor with dependency injection for WholesaleDistributionAppContext
        public AdminController(WholesaleDistributionAppContext context, UserManager<WholesaleDistributionAppUser> userManager,
        IUserStore<WholesaleDistributionAppUser> userStore,
        ILogger<AdminController> logger,
        IServiceProvider serviceProvider,
        FileService fileService)
        {
            _context = context;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _logger = logger;
            _serviceProvider = serviceProvider;
            _fileService = fileService;
        }

        public IActionResult AdminManagement()
        {
            return View();
        }


        public async Task<IActionResult> StockManagement(string searchString)
        {
            // Load Warehouse Stocks
            var stocks = _context.WarehouseStock.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                stocks = stocks.Where(s => s.ItemName.Contains(searchString) ||
                                           s.Description.Contains(searchString) ||
                                           s.StockDistributorId.Contains(searchString));
            }

            return View(stocks.ToList());
        }

        public IActionResult PurchaseStock(string searchString)
        {
            // Load Distributor Stocks for Admin Purchase
            var stocks = _context.DistributorStock
                                 .Include(s => s.Distributor)
                                 .Where(s => s.Quantity > 0)
                                 .AsQueryable();
            if (!string.IsNullOrEmpty(searchString))
            {
                stocks = stocks.Where(s => s.ItemName.Contains(searchString) ||
                                           s.Description.Contains(searchString) ||
                                           s.StockDistributorId.Contains(searchString));
            }

            return View(stocks.ToList());
        }

        public async Task<IActionResult> RefundManagement(string searchString)
        {
            // Load Warehouse Stocks
            var refunds = _context.RefundRequest.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                refunds = refunds.Where(s => s.OrderId.Contains(searchString));
            }

            return View(refunds.ToList());
        }

        public IActionResult UserManagement()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoadData()
        {
            try
            {
                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Convert.ToInt32(Request.Form["start"].FirstOrDefault());
                var length = Convert.ToInt32(Request.Form["length"].FirstOrDefault());
                var searchValue = Request.Form["search[value]"].FirstOrDefault();
                var sortColumn = Request.Form[$"columns[{Request.Form["order[0][column]"].FirstOrDefault()}][name]"].FirstOrDefault();
                var sortColumnDir = Request.Form["order[0][dir]"].FirstOrDefault();

                IQueryable<UserInfo> query = _context.UserInfo.Where(u => u.UserRole == "Admin");

                // Filtering
                if (!string.IsNullOrEmpty(searchValue))
                {
                    query = query.Where(u =>
                        u.UserName.Contains(searchValue) ||
                        u.Email.Contains(searchValue) ||
                        u.PhoneNumber.Contains(searchValue) ||
                        u.Address.Contains(searchValue));
                }

                // Sorting
                if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
                {
                    switch (sortColumn)
                    {
                        case "userName":
                            query = sortColumnDir == "asc" ? query.OrderBy(u => u.UserName) : query.OrderByDescending(u => u.UserName);
                            break;
                        case "email":
                            query = sortColumnDir == "asc" ? query.OrderBy(u => u.Email) : query.OrderByDescending(u => u.Email);
                            break;
                        case "phoneNumber":
                            query = sortColumnDir == "asc" ? query.OrderBy(u => u.PhoneNumber) : query.OrderByDescending(u => u.PhoneNumber);
                            break;
                        case "address":
                            query = sortColumnDir == "asc" ? query.OrderBy(u => u.Address) : query.OrderByDescending(u => u.Address);
                            break;
                        default:
                            break;
                    }
                }

                // Paging
                var recordsTotal = await query.CountAsync();
                var data = await query.Skip(start).Take(length).ToListAsync();

                var responseData = new
                {
                    draw = draw,
                    recordsTotal = recordsTotal,
                    recordsFiltered = recordsTotal,
                    data = data.Select((u, index) => new
                    {
                        no = start + index + 1,
                        userName = u.UserName,
                        email = u.Email,
                        phoneNumber = u.PhoneNumber,
                        address = u.Address,
                        userId = u.UserId,
                    })
                };

                return Json(responseData);
            }
            catch (Exception ex)
            {
                return BadRequest("Error loading data.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> LoadUser()
        {
            try
            {
                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Convert.ToInt32(Request.Form["start"].FirstOrDefault());
                var length = Convert.ToInt32(Request.Form["length"].FirstOrDefault());
                var searchValue = Request.Form["search[value]"].FirstOrDefault();
                var sortColumn = Request.Form[$"columns[{Request.Form["order[0][column]"].FirstOrDefault()}][name]"].FirstOrDefault();
                var sortColumnDir = Request.Form["order[0][dir]"].FirstOrDefault();

                IQueryable<UserInfo> query = _context.UserInfo.Where(u => u.UserRole != "Admin");

                // Filtering
                if (!string.IsNullOrEmpty(searchValue))
                {
                    query = query.Where(u =>
                        u.UserName.Contains(searchValue) ||
                        u.Email.Contains(searchValue) ||
                        u.PhoneNumber.Contains(searchValue) ||
                        u.Address.Contains(searchValue));
                }

                // Sorting
                if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
                {
                    switch (sortColumn)
                    {
                        case "userName":
                            query = sortColumnDir == "asc" ? query.OrderBy(u => u.UserName) : query.OrderByDescending(u => u.UserName);
                            break;
                        case "email":
                            query = sortColumnDir == "asc" ? query.OrderBy(u => u.Email) : query.OrderByDescending(u => u.Email);
                            break;
                        case "phoneNumber":
                            query = sortColumnDir == "asc" ? query.OrderBy(u => u.PhoneNumber) : query.OrderByDescending(u => u.PhoneNumber);
                            break;
                        case "address":
                            query = sortColumnDir == "asc" ? query.OrderBy(u => u.Address) : query.OrderByDescending(u => u.Address);
                            break;
                        default:
                            break;
                    }
                }

                // Paging
                var recordsTotal = await query.CountAsync();
                var data = await query.Skip(start).Take(length).ToListAsync();

                var responseData = new
                {
                    draw = draw,
                    recordsTotal = recordsTotal,
                    recordsFiltered = recordsTotal,
                    data = data.Select((u, index) => new
                    {
                        no = start + index + 1,
                        userName = u.UserName,
                        email = u.Email,
                        phoneNumber = u.PhoneNumber,
                        address = u.Address,
                        userId = u.UserId,
                    })
                };

                return Json(responseData);
            }
            catch (Exception ex)
            {
                return BadRequest("Error loading data.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddUser(InputModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    return Json(new { success = false, message = "Email already exists." });
                }

                var user = new WholesaleDistributionAppUser
                {
                    PhoneNumber = model.PhoneNumber
                };
                await _userStore.SetUserNameAsync(user, model.UserName, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, model.Email, CancellationToken.None);

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var roleResult = await _userManager.AddToRoleAsync(user, "Admin");
                    if (!roleResult.Succeeded)
                    {
                        return Json(new { success = false, message = "Failed to assign role." });
                    }

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<WholesaleDistributionAppContext>();
                        var userInfo = new UserInfo
                        {
                            UserId = user.Id,
                            UserName = user.UserName,
                            Email = user.Email,
                            PhoneNumber = model.PhoneNumber,
                            Address = model.Address,
                            Password = user.PasswordHash,
                            UserRole = "Admin"
                        };
                        _logger.LogInformation("Attempting to save UserInfo for user {UserId}", user.Id);
                        context.UserInfo.Add(userInfo);
                        await context.SaveChangesAsync();
                        _logger.LogInformation("UserInfo saved successfully for user {UserId}", user.Id);
                    }

                    return Json(new { success = true });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Json(new { success = false, message = "Invalid model state.", errors = ModelState.Values.SelectMany(v => v.Errors).ToList() });
        }

        private IUserEmailStore<WholesaleDistributionAppUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<WholesaleDistributionAppUser>)_userStore;
        }
        public class InputModel
        {
            [Required]
            [Display(Name = "UserName")]
            public string UserName { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Display(Name = "PhoneNumber")]
            public string PhoneNumber { get; set; }

            [Display(Name = "Address")]
            public string Address { get; set; }
        }

        [HttpGet]
        public async Task<IActionResult> GetUser(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found."+id });
                }

                var userInfo = await _context.UserInfo.FirstOrDefaultAsync(u => u.UserId == user.Id);

                var userData = new
                {
                    id = user.Id,
                    userName = user.UserName,
                    email = user.Email,
                    phoneNumber = user.PhoneNumber,
                    address = userInfo?.Address ?? ""
                };

                return Json(userData);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while fetching user.", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.FindByIdAsync(model.UserId);
                    if (user == null)
                    {
                        return Json(new { success = false, message = "User not found." });
                    }

                    // Check if UserName is being updated and if it already exists
                    if (user.UserName != model.UserName && await _userManager.FindByNameAsync(model.UserName) != null)
                    {
                        return Json(new { success = false, message = "Username already exists." });
                    }

                    // Check if Email is being updated and if it already exists
                    if (user.Email != model.Email && await _userManager.FindByEmailAsync(model.Email) != null)
                    {
                        return Json(new { success = false, message = "Email address already exists." });
                    }

                    user.UserName = model.UserName;
                    user.Email = model.Email;
                    user.PhoneNumber = model.PhoneNumber;

                    var updateResult = await _userManager.UpdateAsync(user);
                    if (!updateResult.Succeeded)
                    {
                        return Json(new { success = false, message = "Failed to update user." });
                    }

                    var userInfo = await _context.UserInfo.FirstOrDefaultAsync(u => u.UserId == user.Id);
                    if (userInfo != null)
                    {
                        userInfo.UserName = model.UserName;
                        userInfo.Email = model.Email;
                        userInfo.PhoneNumber = model.PhoneNumber;
                        userInfo.Address = model.Address;
                        _context.Entry(userInfo).State = EntityState.Modified;
                        await _context.SaveChangesAsync();
                    }

                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "An error occurred while updating user.", error = ex.Message });
                }
            }

            return Json(new { success = false, message = "Invalid model state.", errors = ModelState.Values.SelectMany(v => v.Errors).ToList() });
        }


        public class EditModel
        {
            [Required]
            [Display(Name = "UserName")]
            public string UserName { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Display(Name = "PhoneNumber")]
            public string PhoneNumber { get; set; }

            [Display(Name = "Address")]
            public string Address { get; set; }

            [Display(Name = "UserId")]
            public string UserId { get; set; }
        }
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                _logger.LogInformation("Attempting to delete user with ID: {UserId}", id);
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }

                _logger.LogInformation("DELETING {UserId}", user);
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    var userInfo = await _context.UserInfo.FirstOrDefaultAsync(u => u.UserId == user.Id);
                    if (userInfo != null)
                    {
                        _context.UserInfo.Remove(userInfo);
                        await _context.SaveChangesAsync();
                    }
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to delete user." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while deleting user.", error = ex.Message });
            }
        }

        // Stock Management

        // Not needed FOR NOW
        /*[HttpPost]
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

                    var stock = new WarehouseStock
                    {
                        StockId = Guid.NewGuid().ToString(),
                        DistributorStockId = , // Need to set here when purchase complete
                        ItemName = viewModel.ItemName,
                        Description = viewModel.Description,
                        Quantity = viewModel.Quantity,
                        SinglePrice = viewModel.SinglePrice,
                        StockDistributorId = viewModel.StockDistributorId,
                        ImgDownloadURL = imgDownloadURL,
                        ForRetailerPurchase = viewModel.ForRetailerPurchase,
                    };

                    _context.WarehouseStock.Add(stock);
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
            return Json(new { success = false, message = "Validation failed", errors });
        }*/

        public async Task<IActionResult> GetStockDetails(string stockIdentifier)
        {
            _logger.LogInformation($"GetStockDetails called with stockIdentifier: {stockIdentifier}");

            if (string.IsNullOrEmpty(stockIdentifier))
            {
                _logger.LogWarning("Stock identifier is required but was not provided.");
                return Json(new { success = false, message = "Stock identifier is required." });
            }

            stockIdentifier = stockIdentifier.ToLower(); // Convert to lower case for case-insensitive comparison

            var stock = await _context.WarehouseStock.FirstOrDefaultAsync(s =>
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

        public async Task<IActionResult> GetDistributorStockDetails(string stockIdentifier)
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

            var stock = await _context.WarehouseStock.FindAsync(model.StockId);
            if (stock == null)
            {
                return Json(new { success = false, message = "Stock not found." });
            }

            // Update stock details
            stock.ItemName = model.ItemName;
            stock.Description = model.Description;
            stock.SinglePrice = model.SinglePrice;
            stock.ForRetailerPurchase = (bool)model.ForRetailerPurchase;

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

                _context.WarehouseStock.Update(stock);
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
            return _context.WarehouseStock.Any(e => e.StockId.Equals(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStock(string stockId)
        {
            if (string.IsNullOrEmpty(stockId))
            {
                return Json(new { success = false, message = "Invalid stock ID" });
            }

            var stock = await _context.WarehouseStock.FindAsync(stockId);
            if (stock == null)
            {
                return Json(new { success = false, message = "Stock not found" });
            }

            _context.WarehouseStock.Remove(stock);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Stock deleted successfully" });
        }
        [HttpPost]
        public async Task<IActionResult> SubmitOrder([FromForm] string orderDetails, [FromForm] IFormFile proofOfPayment)
        {
            if (string.IsNullOrEmpty(orderDetails))
            {
                return Json(new { success = false, message = "No items selected for order." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "User is not authenticated." });
            }

            var orderDetailsList = JsonConvert.DeserializeObject<List<OrderDetails>>(orderDetails);

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

            if (proofOfPayment == null || proofOfPayment.Length == 0)
            {
                return Json(new { success = false, message = "Proof of payment is required." });
            }

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

            var order = new Order
            {
                OrderId = Guid.NewGuid(),
                OrderDate = DateTime.Now,
                WarehouseId = userId,
                TotalAmount = orderDetailsList.Sum(od => od.Subtotal),
                OrderStatus = "Pending",
                PaymentReceiptURL = "",
                StockDistributorId = "",
            };

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
            order.PaymentReceiptURL = imgDownloadURL;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var detail in orderDetailsList)
            {
                detail.OrderId = order.OrderId;
                _context.OrderDetails.Add(detail);
                order.StockDistributorId = detail.StockDistributorId;
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }


    }
}
