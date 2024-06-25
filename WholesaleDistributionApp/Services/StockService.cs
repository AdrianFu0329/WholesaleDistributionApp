using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WholesaleDistributionApp.Data;
using WholesaleDistributionApp.Models;

public class StockService
{
    private readonly WholesaleDistributionAppContext _context;

    public StockService(WholesaleDistributionAppContext context)
    {
        _context = context;
    }

    public async Task<List<Stock>> GetAllStockAsync()
    {
        return await _context.Stock.ToListAsync();
    }

    // Add more methods as needed for specific queries or CRUD operations
}
