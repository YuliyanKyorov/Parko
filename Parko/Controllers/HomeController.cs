using Microsoft.AspNetCore.Mvc;
using Parko.Models;

namespace Parko.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Parking");
        }

        return View(new LoginViewModel());
    }
}
