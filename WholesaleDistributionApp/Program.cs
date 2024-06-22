using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WholesaleDistributionApp.Data;
using WholesaleDistributionApp.Areas.Identity.Data;
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("WholesaleDistributionAppContextConnection") ?? throw new InvalidOperationException("Connection string 'WholesaleDistributionAppContextConnection' not found.");

builder.Services.AddDbContext<WholesaleDistributionAppContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<WholesaleDistributionAppUser>(options => options.SignIn.RequireConfirmedAccount = false) // Was true for email confirmation
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<WholesaleDistributionAppContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Seed roles
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<WholesaleDistributionAppUser>>();

    await SeedRolesAsync(roleManager);
    await SeedAdminUserAsync(userManager);
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

async Task SeedAdminUserAsync(UserManager<WholesaleDistributionAppUser> userManager)
{
    var adminEmail = "admin@example.com";
    var adminPassword = "Admin@123";

    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var adminUser = new WholesaleDistributionAppUser { UserName = adminEmail, Email = adminEmail };
        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
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
