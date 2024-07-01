// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WholesaleDistributionApp.Areas.Identity.Data;
using WholesaleDistributionApp.Data;
using WholesaleDistributionApp.Models;

namespace WholesaleDistributionApp.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<WholesaleDistributionAppUser> _signInManager;
        private readonly UserManager<WholesaleDistributionAppUser> _userManager;
        private readonly IUserStore<WholesaleDistributionAppUser> _userStore;
        private readonly IUserEmailStore<WholesaleDistributionAppUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IServiceProvider _serviceProvider;
        private readonly IWebHostEnvironment _environment;

        public RegisterModel(
            UserManager<WholesaleDistributionAppUser> userManager,
            IUserStore<WholesaleDistributionAppUser> userStore,
            SignInManager<WholesaleDistributionAppUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            IServiceProvider serviceProvider,
            IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _serviceProvider = serviceProvider;
            _environment = environment;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Username")]
            public string UserName { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email Address")]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm Password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            [Display(Name = "Account Type")]
            public string Role { get; set; }

            [Required]
            [Display(Name = "Bank Name")]
            public string BankName { get; set; }

            [Required]
            [Display(Name = "Bank Account Number")]
            public string BankAccNo { get; set; }

            [Required]
            [Display(Name = "Address")]
            public string Address { get; set; }

            [Display(Name = "Bank QR Image")]
            public IFormFile ImageFile { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(Input.Email);

                if (existingUser != null)
                {
                    ModelState.AddModelError(string.Empty, "Email already exists.");
                    return Page();
                }

                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.UserName, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    if (!string.IsNullOrEmpty(Input.Role) && (Input.Role == "Distributor" || Input.Role == "Retailer"))
                    {
                        await _userManager.AddToRoleAsync(user, Input.Role);
                    }

                    // Handle file upload
                    string imagePath = null;

                    if (Input.ImageFile != null && Input.ImageFile.Length > 0)
                    {
                        try
                        {
                            var fileName = $"{user.Id}{Path.GetExtension(Input.ImageFile.FileName)}";
                            var directoryPath = Path.Combine(_environment.WebRootPath, "images", "qr");

                            // Ensure the directory exists
                            if (!Directory.Exists(directoryPath))
                            {
                                Directory.CreateDirectory(directoryPath);
                            }

                            var filePath = Path.Combine(directoryPath, fileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await Input.ImageFile.CopyToAsync(stream);
                            }

                            imagePath = $"/images/qr/{fileName}";
                            _logger.LogInformation("Image path:" + imagePath);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error uploading image.");
                            ModelState.AddModelError(string.Empty, "Error uploading image.");
                            return Page();
                        }
                    }

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<WholesaleDistributionAppContext>();

                        // Check for existing user info to prevent duplicates
                        var existingUserInfo = await context.UserInfo
                            .SingleOrDefaultAsync(ui => ui.UserId == user.Id);

                        if (existingUserInfo == null)
                        {
                            var userInfo = new UserInfo
                            {
                                UserId = user.Id,
                                UserName = user.UserName,
                                Email = user.Email,
                                PhoneNumber = user.PhoneNumber,
                                Password = user.PasswordHash,
                                UserRole = Input.Role,
                                BankName = Input.BankName,
                                BankAccNo = Input.BankAccNo,
                                Address = Input.Address,
                                QRImgURL = imagePath
                            };
                            context.UserInfo.Add(userInfo);
                            await context.SaveChangesAsync();
                            _logger.LogInformation("User Info Table updated");
                        }
                        else
                        {
                            _logger.LogWarning("UserInfo already exists for user Id {UserId}", user.Id);
                        }
                    }

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private WholesaleDistributionAppUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<WholesaleDistributionAppUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(WholesaleDistributionAppUser)}'. " +
                    $"Ensure that '{nameof(WholesaleDistributionAppUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<WholesaleDistributionAppUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<WholesaleDistributionAppUser>)_userStore;
        }
    }
}
