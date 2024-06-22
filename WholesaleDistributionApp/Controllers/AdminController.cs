using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using WholesaleDistributionApp.Data;
using WholesaleDistributionApp.Models;

namespace WholesaleDistributionApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly WholesaleDistributionAppContext _context;

        // Constructor with dependency injection for WholesaleDistributionAppContext
        public AdminController(WholesaleDistributionAppContext context)
        {
            _context = context;
        }

        public IActionResult AdminManagement()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
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
                        u.Address.Contains(searchValue)); // Add filtering for Address
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
                // Log error here if needed
                return BadRequest("Error loading data.");
            }
        }
    }
}
