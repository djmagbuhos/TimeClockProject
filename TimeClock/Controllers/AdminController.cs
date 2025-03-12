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
        public async Task<IActionResult> RecordList(string EmpId, string month, string day, string year)
        {
            var timeLogs = await _dataAccessService.GetAllTimeLogsAsync(EmpId, month, day, year);
            ViewData["EmpId"] = EmpId;
            ViewBag.Status = await _dataAccessService.GetAllStatusAsync();
            ViewBag.Employee = await _dataAccessService.GetAllEmployeeAsync();
            return View(timeLogs);
        }

        [HttpPost]
        public async Task<IActionResult> AddTimeLogs([FromBody] VMTimeLogs model)
        {
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
            var roles = await _dataAccessService.GetAllRolesAsync();
            return View(roles);
        }

        [HttpPost]
        public async Task<IActionResult> AddUser([FromBody] VMUsers model)
        {
            if (model == null)
            {
                return BadRequest("Invalid user data");
            }

            //HashPassword Before Saving
            model.PasswordHash = HashPassword(model.PasswordHash);

            var result = await _dataAccessService.AddUsersAsync(model);
            if (result == null)
            {
                return StatusCode(500, "Failed to add user.");
            }

            return Ok(new { message = "User added successfully!", id = result.Id });
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

        //---------------------------------------------------------------------//
        //---------------------------------------------------------------------//


        //---------------------------------------------------------------------//
        //------------------------ EMPLOYEE -----------------------------------//
        public async Task<IActionResult> EmployeeList()
        {
            var model = await _dataAccessService.GetAllEmployeeAsync();
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

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateEmployeeAsync(VMEmployee model, IFormFile ProfilePicture)
        {
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
            var model = await _dataAccessService.GetAllPositionAsync();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddEmployeeAsync(VMEmployee model, IFormFile ProfilePicture)
        {
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

        public async Task<IActionResult> DeleteEmployee(int id)
        {
            bool isDeleted = await _dataAccessService.DeleteEmployeeAsync(id);

            if (isDeleted)
            {
                return RedirectToAction("EmployeeList", "Admin");
            }

            return RedirectToAction("EmployeeList", "Admin");  // will add error msg in the future
        }
        //--------------------------END OF EMPLOYEE-----------------------------//
        //---------------------------------------------------------------------//
    }
}
