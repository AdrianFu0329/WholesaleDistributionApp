using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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

        // Constructor with dependency injection for WholesaleDistributionAppContext
        public AdminController(WholesaleDistributionAppContext context, UserManager<WholesaleDistributionAppUser> userManager,
        IUserStore<WholesaleDistributionAppUser> userStore,
        ILogger<AdminController> logger,
        IServiceProvider serviceProvider)
        {
            _context = context;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public IActionResult AdminManagement()
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
                    query = query.OrderBy(u => sortColumn == "userName" ? u.UserName :
                                               sortColumn == "email" ? u.Email :
                                               sortColumn == "phoneNumber" ? u.PhoneNumber :
                                               sortColumn == "address" ? u.Address :
                                               u.UserRole);
                }

                // Paging
                var recordsTotal = await query.CountAsync();
                var data = await query.Skip(start).Take(length).ToListAsync();

                return Json(new
                {
                    draw = draw,
                    recordsTotal = recordsTotal,
                    recordsFiltered = recordsTotal,
                    data = data.Select((u, index) => new
                    {
                        no = index + 1,
                        userName = u.UserName,
                        email = u.Email,
                        phoneNumber = u.PhoneNumber,
                        address = u.Address
                    })
                });
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

                var user = new WholesaleDistributionAppUser();
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

    }
}
