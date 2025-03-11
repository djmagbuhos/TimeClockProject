using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeClock.Services;
using TimeClock.ViewModel;

namespace TimeClock.Controllers
{
    public class AdminController : Controller
    {
        private readonly ILogger<EmployeeController> _logger;
        private readonly IDataAccessService _dataAccessService;
        private readonly IConfiguration _configuration;

        public AdminController(
            IConfiguration configuration,
            IDataAccessService dataAccessService,
            ILogger<EmployeeController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dataAccessService = dataAccessService ?? throw new ArgumentNullException(nameof(dataAccessService));
            this._configuration = configuration;

        }

        public IActionResult RecordList()
        {
            //var data = _dataAccessService.GetAllUsersAsync();

            return View();
        }

        public IActionResult AddUser()
        {
            return View();
        }

        public IActionResult UserList()
        {
            return View();
        }


        public async Task<IActionResult> EmployeeList()
        {
            var model = await _dataAccessService.GetAllEmployeeAsync();
            var positions = await _dataAccessService.GetAllPositionAsync();

            if (positions == null || !positions.Any())
            {
                Console.WriteLine("⚠️ No positions found!");
                ViewBag.Positions = new List<VMPosition>(); // Avoid null reference
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
                // If no new profile picture is uploaded, retain the current image.
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

        //private async Task<VMEmployee> AddEmployeeAsync(VMEmployee model)
        //{
        //    await _dataAccessService.AddEmployeeAsync(model);
        //    return model;
        //}

        //private async Task<VMEmployee> UpdateEmployeeAsync(VMEmployee model)
        //{
        //    await _dataAccessService.UpdateEmployeeAsync(model);

        //    return model;
        //}

        public async Task<IActionResult> DeleteEmployee(int id)
        {
            bool isDeleted = await _dataAccessService.DeleteEmployeeAsync(id);

            if (isDeleted)
            {
                return RedirectToAction("EmployeeList", "Admin");
            }

            return RedirectToAction("EmployeeList", "Admin");  // Or return a view with error will add error msg in the future
        }


        private async Task DeleteEmployeeAsync(VMEmployee model)
        {
            await _dataAccessService.DeleteEmployeeAsync(model.Id);
        }


        //private VMEmployee AddEmployee(VMEmployee model)
        //{
        //    _dataAccessService.AddEmployeeAsync(model);
        //    return model;
        //}


        private void DeleteEmployee(VMEmployee model)
        {
            _dataAccessService.DeleteEmployeeAsync(model.Id);
        }


        //public async Task<IActionResult> Testinglaang()
        //{
        //    var vmAddEmployee = new VMEmployee()
        //    {
        //        Id = 0,
        //        FirstName = "Juan 1",
        //        LastName = "dela Cruz1",
        //        Gender = "M",
        //        PositionID = 1,
        //        DateOfBirth = new DateTime(2000, 02, 28),
        //        CreatedAt = DateTime.Now
        //    };
        //    vmAddEmployee = AddEmployee(vmAddEmployee);

        //    var vmUpdateEmployee = await _dataAccessService.GetEmployeeByIdAsync(vmAddEmployee.Id);
        //    if (vmUpdateEmployee != null)
        //    {
        //        vmUpdateEmployee.FirstName = "Mark";
        //        vmUpdateEmployee.LastName = "Cortel maraming chicks";
        //        vmUpdateEmployee.DateOfBirth = new DateTime(2000, 02, 28);
        //        vmUpdateEmployee = UpdateEmployee(vmUpdateEmployee);
        //    }


        //    var vmDeleteEmployee = await _dataAccessService.GetEmployeeByIdAsync(vmUpdateEmployee.Id);
        //    if (vmDeleteEmployee != null)
        //    {
        //        DeleteEmployee(vmDeleteEmployee);
        //    }



        //    var allEmployees = await _dataAccessService.GetAllEmployeeAsync();
        //    foreach (var emp in allEmployees)
        //    {
        //        var employee = await _dataAccessService.GetEmployeeByIdAsync(emp.Id);

        //        var x = employee;
        //    }


        //    var allUsers = await _dataAccessService.GetAllUsersAsync();
        //    foreach (var item in allUsers)
        //    {
        //        var user = await _dataAccessService.GetAllUsersByIdAsync(item.Id);

        //        var y = user;
        //    }


        //    return View(allEmployees);
        //}




    }
}
