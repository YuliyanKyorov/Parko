using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
namespace Parko.Controllers;

public class ErrorController : Controller
{
    // /Error      -> необработена грешка (500)
    // /Error/404  -> страницата не е намерена
    public IActionResult Index(int? id)
    {
        int code = id ?? 500;
        ViewBag.Code = code;
        ViewBag.Title = code switch
        {
            404 => "Page not found",
            403 => "Access denied",
            _ => "An error occurred"
        };
        ViewBag.Message = code switch
        {
            404 => "The address does not exist or has been removed.",
            403 => "You do not have permission to view this page.",
            _ => "Something went wrong in the application. Please try again later."
        };
        ViewBag.RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        Response.StatusCode = code;
        return View();
    }
}