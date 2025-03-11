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


        public IActionResult Index()
        {
            //var model = _dataAccessService.GetAllEmployeeAsync();

            return View();
        }
    }
}
