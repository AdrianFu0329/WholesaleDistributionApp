// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using WholesaleDistributionApp.Areas.Identity.Data;
using WholesaleDistributionApp.Data;
using WholesaleDistributionApp.Models;

namespace WholesaleDistributionApp.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly WholesaleDistributionAppContext _context;
        private readonly UserManager<WholesaleDistributionAppUser> _userManager;
        private readonly SignInManager<WholesaleDistributionAppUser> _signInManager;
        private readonly IWebHostEnvironment _environment;

        public IndexModel(
            UserManager<WholesaleDistributionAppUser> userManager,
            SignInManager<WholesaleDistributionAppUser> signInManager,
            WholesaleDistributionAppContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _environment = environment;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            [Required]
            [DataType(DataType.MultilineText)]
            [Display(Name = "Your Address")]
            public string Address { get; set; }

            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Display(Name = "Bank Name")]
            public string BankName { get; set; }

            [Display(Name = "Bank Account Number")]
            public string BankAccNo { get; set; }

            public IFormFile ImageFile { get; set; }
            public string QRImgURL { get; set; }
        }

        private async Task LoadAsync(UserInfo userInfo)
        {
            var user = await _userManager.GetUserAsync(User);
            Username = user.UserName;

            Input = new InputModel
            {
                PhoneNumber = userInfo.PhoneNumber,
                Address = userInfo.Address,
                Email = userInfo.Email,
                BankName = userInfo.BankName,
                BankAccNo = userInfo.BankAccNo,
                QRImgURL = userInfo.QRImgURL
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var userInfo = await _context.UserInfo.FirstOrDefaultAsync(ui => ui.UserId == user.Id);
            if (userInfo == null)
            {
                return NotFound($"Unable to load user info for user with ID '{user.Id}'.");
            }

            await LoadAsync(userInfo);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var userInfo = await _context.UserInfo.SingleOrDefaultAsync(ui => ui.UserId == user.Id);
            if (userInfo == null)
            {
                return NotFound($"Unable to load user info for user with ID '{user.Id}'.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Handle file upload
            if (Input.ImageFile != null)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "qr");
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Input.ImageFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await Input.ImageFile.CopyToAsync(fileStream);
                }
                userInfo.QRImgURL = "/images/qr" + uniqueFileName;
            }

            // Update other fields
            userInfo.PhoneNumber = Input.PhoneNumber;
            userInfo.Address = Input.Address;
            userInfo.Email = Input.Email;
            userInfo.BankName = Input.BankName;
            userInfo.BankAccNo = Input.BankAccNo;

            _context.UserInfo.Update(userInfo);
            await _context.SaveChangesAsync();

            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
