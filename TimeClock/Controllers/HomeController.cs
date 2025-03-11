using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TimeClock.Models;

namespace TimeClock.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

}
