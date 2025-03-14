using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TimeClock.Models;
using TimeClock.Services;
using TimeClock.ViewModel;

namespace TimeClock.Controllers
{
    public class AdminController : Controller
    {
        private readonly ILogger<EmployeeController> _logger;
        private readonly IDataAccessService _dataAccessService;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;




        public AdminController(
            IConfiguration configuration,
            IDataAccessService dataAccessService,
            ILogger<EmployeeController> logger,
            ApplicationDbContext context)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dataAccessService = dataAccessService ?? throw new ArgumentNullException(nameof(dataAccessService));
            this._configuration = configuration;
            _context = context;
        }

        //--------------- RECORD LIST (ADMIN) --------------------------//

        [HttpGet]
        public async Task<IActionResult> RecordList(string EmpId, string month, string day, string year, int page = 1)
        {
            //------ACCESS VALIDATION-----//
            int? roleId = HttpContext.Session.GetInt32("RoleId");

            if (roleId == null || roleId != 1)
            {
                TempData["AccessDenied"] = "Access Denied! You do not have permission to view this page.";
                return RedirectToAction("Index", "Home");
            }
            //-------END VALIDATION-------//

            int pageSize = 10; // Set the number of records per page

            var timeLogs = await _dataAccessService.GetAllTimeLogsAsync(EmpId, month, day, year);

            // Paginate the data
            var paginatedLogs = timeLogs.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewData["EmpId"] = EmpId;
            ViewBag.Status = await _dataAccessService.GetAllStatusAsync();
            ViewBag.Employee = await _dataAccessService.GetAllEmployeeAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)timeLogs.Count() / pageSize);

            return View(paginatedLogs);
        }


        [HttpPost]
        public async Task<IActionResult> AddTimeLogs([FromBody] VMTimeLogs model)
        {
            //------ACCESS VALIDATION-----//
            // Check session for RoleId
            int? roleId = HttpContext.Session.GetInt32("RoleId");

            if (roleId == null || roleId != 1)
            {
                TempData["AccessDenied"] = "Access Denied! You do not have permission to view this page.";
                return RedirectToAction("Index", "Home");
            }
            //-------END VALIDATION-------//

            bool isSuccess = await _dataAccessService.AddTimeLogs(model);

            if (isSuccess)
            {
                return Json(new { redirectUrl = Url.Action("RecordList", "Admin") });
            }
            else
            {
                return BadRequest(new { message = "Time Out must be greater than Time In." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditTimeLogs(EditTimeLogVM model)
        {
            //------ACCESS VALIDATION-----//
            // Check session for RoleId
            int? roleId = HttpContext.Session.GetInt32("RoleId");

            if (roleId == null || roleId != 1)
            {
                TempData["AccessDenied"] = "Access Denied! You do not have permission to view this page.";
                return RedirectToAction("Index", "Home");
            }
            //-------END VALIDATION-------//

            Console.WriteLine($"Received: Id={model.Id}, TimeIN={model.TimeIN}, TimeOUT={model.TimeOUT}, StatusID={model.StatusID}");

            if (ModelState.IsValid)
            {
                bool isSuccess = await _dataAccessService.UpdateTimeLogs(model);
                return isSuccess ? Json(new { success = true }) : BadRequest(new { message = "error on updating data" });
            }
            else
            {
                // Send back detailed validation errors
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(errors);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTimeLog(int id)
        {
            //------ACCESS VALIDATION-----//
            // Check session for RoleId
            int? roleId = HttpContext.Session.GetInt32("RoleId");

            if (roleId == null || roleId != 1)
            {
                TempData["AccessDenied"] = "Access Denied! You do not have permission to view this page.";
                return RedirectToAction("Index", "Home");
            }
            //-------END VALIDATION-------//

            var timeLog = await _context.TimeLogs.FindAsync(id);

            if (timeLog == null)
            {
                return NotFound(new { message = "Time log not found." });
            }

            _context.TimeLogs.Remove(timeLog);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Time log deleted successfully." });
        }

        //---------------------------------------------------------------------//
        //---------------------------------------------------------------------//


        //---------------------------------------------------------------------//
        //------------------------ USER ---------------------------------------//

        [HttpGet]
        public async Task<IActionResult> AddUser()
        {
            //------ACCESS VALIDATION-----//
            // Check session for RoleId
            int? roleId = HttpContext.Session.GetInt32("RoleId");

            if (roleId == null || roleId != 1)
            {
                TempData["AccessDenied"] = "Access Denied! You do not have permission to view this page.";
                return RedirectToAction("Index", "Home");
            }
            //-------END VALIDATION-------//

            var roles = await _dataAccessService.GetAllRolesAsync();
            return View(roles);
        }

        [HttpPost]
        public async Task<IActionResult> AddUser([FromBody] VMUsers model)
        {
            int? roleId = HttpContext.Session.GetInt32("RoleId");
            if (roleId == null || roleId != 1)
            {
                return Unauthorized(new { message = "Access Denied! You do not have permission to view this page." });
            }

            if (model == null)
            {
                return BadRequest(new { message = "Invalid user data." });
            }

            if (string.IsNullOrWhiteSpace(model.PasswordHash))
            {
                return BadRequest(new { message = "Password cannot be empty." });
            }

            if (model.PasswordHash.Length < 6)
            {
                return BadRequest(new { message = "Password must be at least 6 characters long." });
            }

            // Hash the password before saving
            model.PasswordHash = HashPassword(model.PasswordHash);

            try
            {
                var result = await _dataAccessService.AddUsersAsync(model);

                if (result == null)
                {
                    return BadRequest(new { message = "Failed to add user." });
                }

                return Ok(new { message = "User added successfully!", id = result.Id });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message }); // Custom validation error
            }
            catch (DbUpdateException dbEx)
            {
                string errorMessage = "An unexpected error occurred. Please try again.";

                if (dbEx.InnerException?.Message.Contains("UserName") == true)
                {
                    errorMessage = "This username is already taken. Choose another.";
                }
                else if (dbEx.InnerException?.Message.Contains("Email") == true)
                {
                    errorMessage = "This email is already in use. Use a different email.";
                }
                else if (dbEx.InnerException?.Message.Contains("EmpId") == true)
                {
                    errorMessage = "An account already exists for this Employee ID.";
                }

                return BadRequest(new { message = errorMessage });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Unexpected error: {ex.Message}" });
            }
        }




        private static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }



        public async Task<IActionResult> UserList()
        {
            //------ACCESS VALIDATION-----//
            // Check session for RoleId
            int? roleId = HttpContext.Session.GetInt32("RoleId");

            if (roleId == null || roleId != 1)
            {
                TempData["AccessDenied"] = "Access Denied! You do not have permission to view this page.";
                return RedirectToAction("Index", "Home");
            }
            //-------END VALIDATION-------//

            var users = await _dataAccessService.GetAllUsersAsync();
            var roles = await _dataAccessService.GetAllRolesAsync(); // Fetch roles

            if (users == null)
            {
                users = new List<VMUsers>();
            }

            ViewBag.Roles = roles; // Pass roles to the view

            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUser(int userId, string role, string email)
        {
            //------ACCESS VALIDATION-----//
            // Check session for RoleId
            int? roleId = HttpContext.Session.GetInt32("RoleId");

            if (roleId == null || roleId != 1)
            {
                TempData["AccessDenied"] = "Access Denied! You do not have permission to view this page.";
                return RedirectToAction("Index", "Home");
            }
            //-------END VALIDATION-------//

            var user = await _dataAccessService.GetAllUsersByIdAsync(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            //user.RoleName = role;
            user.RoleId = Convert.ToInt32(role);
            user.Email = email;

            var updatedUser = await _dataAccessService.UpdateUsersAsync(user);
            if (updatedUser != null)
            {
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Failed to update user" });

        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int Id)
        {
            bool isDeleted = await _dataAccessService.DeleteUsersAsync(Id); // Gamitin ang service

            if (isDeleted)
            {
                return Json(new { success = true, message = "User deleted successfully!" });
            }
            else
            {
                return Json(new { success = false, message = "User not found or could not be deleted." });
            }
        }
        //---------------------------------------------------------------------//
        //---------------------------------------------------------------------//


        //---------------------------------------------------------------------//
        //------------------------ EMPLOYEE -----------------------------------//
        public async Task<IActionResult> EmployeeList(int page = 1, int pageSize = 8)
        {
            //------ACCESS VALIDATION-----//
            int? roleId = HttpContext.Session.GetInt32("RoleId");
            if (roleId == null || roleId != 1)
            {
                TempData["AccessDenied"] = "Access Denied! You do not have permission to view this page.";
                return RedirectToAction("Index", "Home");
            }
            //-------END VALIDATION-------//

            var allEmployees = await _dataAccessService.GetAllEmployeeAsync();
            var positions = await _dataAccessService.GetAllPositionAsync();

            if (positions == null || !positions.Any())
            {
                Console.WriteLine("⚠️ No positions found!");
                ViewBag.Positions = new List<VMPosition>();
            }
            else
            {
                foreach (var pos in positions)
                {
                    Console.WriteLine($"✅ Position Found: {pos.Id} - {pos.description}");
                }
                ViewBag.Positions = positions;
            }

            // Pagination logic
            var paginatedEmployees = allEmployees
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(allEmployees.Count / (double)pageSize);

            return View(paginatedEmployees);
        }


        [HttpPost]
        public async Task<IActionResult> UpdateEmployeeAsync(VMEmployee model, IFormFile ProfilePicture)
        {
            //------ACCESS VALIDATION-----//
            // Check session for RoleId
            int? roleId = HttpContext.Session.GetInt32("RoleId");

            if (roleId == null || roleId != 1)
            {
                TempData["AccessDenied"] = "Access Denied! You do not have permission to view this page.";
                return RedirectToAction("Index", "Home");
            }
            //-------END VALIDATION-------//

            if (ProfilePicture != null && ProfilePicture.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await ProfilePicture.CopyToAsync(memoryStream);
                    model.ProfilePicture = memoryStream.ToArray(); // Store the new image as byte[]
                }
            }
            else
            {
                // If no new pfp is uploaded, retain the current image.
                var existingEmployee = await _dataAccessService.GetEmployeeByIdAsync(model.Id);
                if (existingEmployee != null)
                {
                    model.ProfilePicture = existingEmployee.ProfilePicture;
                }
            }
       
            await _dataAccessService.UpdateEmployeeAsync(model);

            return RedirectToAction("EmployeeList", "Admin");
        }

        [HttpGet]
        public async Task<IActionResult> AddEmployeeAsync()
        {
            //------ACCESS VALIDATION-----//
            // Check session for RoleId
            int? roleId = HttpContext.Session.GetInt32("RoleId");

            if (roleId == null || roleId != 1)
            {
                TempData["AccessDenied"] = "Access Denied! You do not have permission to view this page.";
                return RedirectToAction("Index", "Home");
            }
            //-------END VALIDATION-------//

            var model = await _dataAccessService.GetAllPositionAsync();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddEmployeeAsync(VMEmployee model, IFormFile ProfilePicture)
        {
            //------ACCESS VALIDATION-----//
            // Check session for RoleId
            int? roleId = HttpContext.Session.GetInt32("RoleId");

            if (roleId == null || roleId != 1)
            {
                TempData["AccessDenied"] = "Access Denied! You do not have permission to view this page.";
                return RedirectToAction("Index", "Home");
            }
            //-------END VALIDATION-------//

            if (ProfilePicture != null && ProfilePicture.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await ProfilePicture.CopyToAsync(memoryStream);
                    model.ProfilePicture = memoryStream.ToArray(); // Store image as byte[]
                }
            }

            // Add Employee to Database
            await _dataAccessService.AddEmployeeAsync(model);

            return RedirectToAction("EmployeeList", "Admin");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteEmployee([FromBody] int id)
        {
            //------ACCESS VALIDATION-----//
            // Check session for RoleId
            int? roleId = HttpContext.Session.GetInt32("RoleId");

            if (roleId == null || roleId != 1)
            {
                TempData["AccessDenied"] = "Access Denied! You do not have permission to view this page.";
                return RedirectToAction("Index", "Home");
            }
            //-------END VALIDATION-------//

            if (id <= 0)
            {
                return Json(new { success = false, message = "Invalid employee ID." });
            }

            try
            {
                bool isDeleted = await _dataAccessService.DeleteEmployeeAsync(id);
                if (isDeleted)
                {
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "Employee not found or cannot be deleted." });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting employee: " + ex.Message);
                return Json(new { success = false, message = "Server error: " + ex.Message });
            }
        }

    }
    //--------------------------END OF EMPLOYEE-----------------------------//
    //---------------------------------------------------------------------//
}

