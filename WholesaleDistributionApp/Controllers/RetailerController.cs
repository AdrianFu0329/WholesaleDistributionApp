using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WholesaleDistributionApp.Areas.Identity.Data;
using WholesaleDistributionApp.Data;

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
    }
}
