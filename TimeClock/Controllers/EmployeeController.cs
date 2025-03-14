using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TimeClock.Services;

namespace TimeClock.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly ILogger<EmployeeController> _logger;
        private readonly IDataAccessService _dataAccessService;
        private readonly IConfiguration _configuration;

        public EmployeeController(
            IConfiguration configuration,
            IDataAccessService dataAccessService,
            ILogger<EmployeeController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dataAccessService = dataAccessService ?? throw new ArgumentNullException(nameof(dataAccessService));
            this._configuration = configuration;

        }


        public async Task<IActionResult> Index(int? month, int? day, int? year, int page = 1, int pageSize = 20)
        {
            //------ACCESS VALIDATION-----//
            int? roleId = HttpContext.Session.GetInt32("RoleId");

            if (roleId == null || roleId != 2)
            {
                TempData["AccessDenied"] = "Access Denied! This page is for Employees only.";
                return RedirectToAction("Index", "Home");
            }
            //-------END VALIDATION-------//

            int empId = Convert.ToInt32(HttpContext.Session.GetInt32("EmpId")); // Get logged-in Employee ID

            var timeLogs = await _dataAccessService.GetFilteredTimeLogsAsync(empId, month, day, year);

            int totalRecords = timeLogs.Count();
            var paginatedLogs = timeLogs.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            return View(paginatedLogs);
        }


    }
}