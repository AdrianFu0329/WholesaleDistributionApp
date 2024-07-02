using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using WholesaleDistributionApp.Areas.Identity.Data;
using WholesaleDistributionApp.Models;

namespace WholesaleDistributionApp.Data;

public class WholesaleDistributionAppContext : IdentityDbContext<WholesaleDistributionAppUser>
{
    public WholesaleDistributionAppContext(DbContextOptions<WholesaleDistributionAppContext> options)
        : base(options)
    {
    }

    public DbSet<UserInfo> UserInfo { get; set; }

    public DbSet<WarehouseStock> WarehouseStock { get; set; }
    public DbSet<DistributorStock> DistributorStock { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetails> OrderDetails { get; set; }
    public DbSet<RefundRequest> RefundRequest { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);

        //join relationship between Distributor and UserInfo
        builder.Entity<DistributorStock>()
                    .HasOne(s => s.Distributor)
                    .WithMany()
                    .HasForeignKey(s => s.StockDistributorId)
                    .HasPrincipalKey(d => d.UserId);

        builder.Entity<WarehouseStock>()
                    .HasOne(s => s.Distributor)
                    .WithMany()
                    .HasForeignKey(s => s.StockDistributorId)
                    .HasPrincipalKey(d => d.UserId);
    }
}
