using Microsoft.AspNetCore.Mvc;
using Parko.Models;

namespace Parko.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View(new LoginViewModel());
    }
}
