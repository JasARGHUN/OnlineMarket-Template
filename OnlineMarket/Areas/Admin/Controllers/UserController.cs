using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineMarket.DataAccess.Data;
using OnlineMarket.Models;
using OnlineMarket.Utility;

namespace OnlineMarket.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/User
        public IActionResult Index()
        {
            return View();
        }


        #region API CALLS

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userList = await _context.ApplicationUsers.Include(x => x.Company).ToListAsync(); // Include Company to User
            var userRole = await _context.UserRoles.ToListAsync();
            var roles = await _context.Roles.ToListAsync();

            foreach(var user in userList)
            {
                var roleId = userRole.FirstOrDefault(x => x.UserId == user.Id).RoleId;
                user.Role = roles.FirstOrDefault(x => x.Id == roleId).Name;

                if (user.Company == null)
                {
                    user.Company = new Company()
                    {
                        Name = ""
                    };
                }
            }

            return Json(new { data = userList });
        }

        [HttpPost]
        public async Task<IActionResult> LockUnlock([FromBody] string id)
        {
            var dbItem = _context.ApplicationUsers.FirstOrDefault(x => x.Id == id);

            if(dbItem == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Error while Locking/Unlocking"
                });
            }

            if(dbItem.LockoutEnd != null && dbItem.LockoutEnd > DateTime.Now)
            {
                // User is currently locked, we will unlock them
                dbItem.LockoutEnd = DateTime.Now;
            }
            else
            {
                dbItem.LockoutEnd = DateTime.Now.AddYears(100);
            }

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Successful"
            });
        }

        #endregion
    }
}
