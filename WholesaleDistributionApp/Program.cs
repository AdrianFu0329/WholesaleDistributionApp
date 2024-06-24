using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WholesaleDistributionApp.Data;
using WholesaleDistributionApp.Areas.Identity.Data;
using WholesaleDistributionApp.Models;
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("WholesaleDistributionAppContextConnection") ?? throw new InvalidOperationException("Connection string 'WholesaleDistributionAppContextConnection' not found.");

builder.Services.AddDbContext<WholesaleDistributionAppContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<WholesaleDistributionAppUser>(options => options.SignIn.RequireConfirmedAccount = false) // Was true for email confirmation
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<WholesaleDistributionAppContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddScoped<StockService>();


builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddLogging(logging =>
{
    logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
    logging.AddConsole(); // Add console logging provider
    logging.AddDebug();   // Add debug output logging provider
    // Add other logging providers as needed (e.g., file logging, event log, etc.)
});

var app = builder.Build();

// Seed roles
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<WholesaleDistributionAppUser>>();
    var context = services.GetRequiredService<WholesaleDistributionAppContext>();


    await SeedRolesAsync(roleManager);
    await SeedAdminUserAsync(userManager, context);
}

async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
{
    var roles = new[] { "Admin", "Distributor", "Retailer" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

async Task SeedAdminUserAsync(UserManager<WholesaleDistributionAppUser> userManager, WholesaleDistributionAppContext context)
{
    var admins = new List<(string username, string email, string password, string phoneNumber, string address)>
    {
        ("admin1", "admin@example.com", "Admin@123", "1234567890", "123 Admin St, City"),
        ("admin2", "admin2@example.com", "Admin@123", "2345678901", "456 Admin Ave, Town"),
        ("admin3", "admin3@example.com", "Admin@123", "3456789012", "789 Admin Rd, Village"),
        ("admin4", "admin4@example.com", "Admin@123", "4567890123", "987 Admin Blvd, Metro"),
        ("admin5", "admin5@example.com", "Admin@123", "4567890123", "987 Admin JJ, Town"),
    };

    foreach (var (username, email, password, phoneNumber, address) in admins)
    {
        if (await userManager.FindByEmailAsync(email) == null)
        {
            var adminUser = new WholesaleDistributionAppUser
            {
                UserName = username,
                Email = email,
                PhoneNumber = phoneNumber
            };

            var result = await userManager.CreateAsync(adminUser, password);

            if (result.Succeeded)
            {
                var userInfo = new UserInfo
                {
                    UserId = adminUser.Id,
                    UserName = adminUser.UserName,
                    Email = adminUser.Email,
                    PhoneNumber = adminUser.PhoneNumber,
                    Password = adminUser.PasswordHash,
                    UserRole = "Admin",
                    Address = address
                };

                context.UserInfo.Add(userInfo);
                await context.SaveChangesAsync();

                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.Run();
