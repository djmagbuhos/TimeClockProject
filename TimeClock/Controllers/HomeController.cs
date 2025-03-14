using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using TimeClock.Services; // Import your service
using TimeClock.Models;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TimeClock.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDataAccessService _dataAccessService;
        private readonly ApplicationDbContext _context;

        public HomeController(IDataAccessService dataAccessService, ApplicationDbContext context)
        {
            _dataAccessService = dataAccessService;
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            int? roleId = HttpContext.Session.GetInt32("RoleId");

            if (roleId >= 3 || roleId == 1)
            {
                TempData["AccessDenied"] = "Access Denied! This page is for Employees only.";
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetTodaysLogs()
        {
            int? empId = HttpContext.Session.GetInt32("EmpId");

            if (empId == null)
            {
                return Unauthorized(new { message = "User is not logged in." });
            }

            var logs = await _dataAccessService.GetTodaysLogsAsync((int)empId);

            return Ok(logs);
        }


        [HttpPost]
        public IActionResult ClearSession()
        {
            HttpContext.Session.Clear();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Clear session before login
            HttpContext.Session.Clear();

            var user = await _dataAccessService.ValidateUserAsync(username, password);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            // Check if the user has the correct role
            if (user.RoleId != 2) // Assuming RoleId = 2 is for Employees
            {
                return BadRequest(new { message = "Only users with the role of employees are able to Clock-in." });
            }

            // Check if the employee already has an active clock-in today
            bool hasActiveClockIn = await _context.TimeLogs
                .AnyAsync(t => t.EmpId == user.EmpId && t.LogDate == DateTime.Today && t.TimeOUT == null);

            if (hasActiveClockIn)
            {
                return BadRequest(new { message = "You already have an active clock-in today. Please clock out first before clocking in again." });
            }

            // Store EmpId and RoleID in session
            HttpContext.Session.SetInt32("EmpId", (int)user.EmpId);
            HttpContext.Session.SetInt32("RoleID", user.RoleId);
            HttpContext.Session.SetString("PositionName", user.PositionName);
            HttpContext.Session.SetString("EmployeeName", user.EmployeeName);

            // Insert new TimeLog entry
            var timeLog = new TimeLogs
            {
                EmpId = user.EmpId,
                LogDate = DateTime.Today, // Store only date
                TimeIN = DateTime.ParseExact(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "yyyy-MM-dd HH:mm:ss", null), // Removes milliseconds
                StatusID = 4
            };

            await _context.TimeLogs.AddAsync(timeLog);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                empId = user.EmpId,
                roleId = user.RoleId,
                employeeName = user.EmployeeName,
                positionName = user.PositionName
            });
        }


        [HttpPost]
        public async Task<IActionResult> ClockOut(string username, string password)
        {
            var user = await _dataAccessService.ValidateUserAsync(username, password);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            // Check if the user has the correct role
            if (user.RoleId != 2) // Assuming RoleId = 2 is for Employees
            {
                return BadRequest(new { message = "Only employees are allowed to clock out." });
            }

            // Check if the user has an active clock-in today
            var activeLog = await _dataAccessService.GetActiveClockIn(user.EmpId);

            if (activeLog == null)
            {
                return BadRequest(new { message = "No active clock-in found for today. Please clock in first." });
            }

            // Update the TimeOUT field
            activeLog.TimeOUT = DateTime.Now;

            // Save changes
            await _dataAccessService.UpdateClockOut(activeLog);

            // Store the new session data AFTER clock-out
            HttpContext.Session.SetInt32("EmpId", user.EmpId);
            HttpContext.Session.SetInt32("RoleId", user.RoleId);
            HttpContext.Session.SetString("PositionName", user.PositionName);
            HttpContext.Session.SetString("EmployeeName", user.EmployeeName);

            return Ok(new { message = "Clocked out successfully!",
                            empId = user.EmpId,
                            roleId = user.RoleId,
                            employeeName = user.EmployeeName,
                            positionName = user.PositionName
            });
        }


        ///// SITE LOGIN NAV PART
        [HttpPost]
        public async Task<IActionResult> LoginSite(string username, string password)
        {
            var user = await _dataAccessService.ValidateUserAsync(username, password);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            // Store session values
            HttpContext.Session.SetInt32("EmpId", user.EmpId);
            HttpContext.Session.SetInt32("RoleId", user.RoleId);
            HttpContext.Session.SetString("PositionName", user.PositionName);
            HttpContext.Session.SetString("EmployeeName", user.EmployeeName);

            // If RoleId == 1 (Admin), redirect to /Admin/RecordList
            if (user.RoleId == 1)
            {
                return Ok(new
                {
                    message = "Login successful!",
                    empId = user.EmpId,
                    roleId = user.RoleId,
                    employeeName = user.EmployeeName,
                    positionName = user.PositionName,
                    redirectUrl = Url.Action("RecordList", "Admin") // Send the URL in response
                });
            }

            // Otherwise, return normal response
            return Ok(new
            {
                message = "Login successful!",
                empId = user.EmpId,
                roleId = user.RoleId,
                employeeName = user.EmployeeName,
                positionName = user.PositionName
            });
        }


        /// LOGOUT NAV PART
        /// 

        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Clears all session data
            return RedirectToAction("Index"); // Redirects to homepage
        }


    }
}
