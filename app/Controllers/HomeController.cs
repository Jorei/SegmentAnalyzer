using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

using aspnetapp.Models;

namespace aspnetapp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index(ActivityViewModel model)
    {
        // Create activities view bag
        List<SelectListItem> activities = new List<SelectListItem>();
        foreach (string file in Directory.GetFiles("gpx").Where(f => f.EndsWith(".gpx") || f.EndsWith(".tcx")).Where(f => !f.Contains("route") && !f.Contains("Route")))
        {
            activities.Add(new SelectListItem { Value = file, Text = file });
        }
        ViewBag.Activities = activities;

        // Create routes view bag
        List<SelectListItem> routes = new List<SelectListItem>();
        foreach (string file in Directory.GetFiles("gpx").Where(f => f.EndsWith(".gpx") || f.EndsWith(".tcx")).Where(f => f.Contains("route") || f.Contains("Route")))
        {
            string text = file.Replace("gpx\\","").Replace("gpx/","").Replace("route_","");
            routes.Add(new SelectListItem { Value = text, Text = text });
        }
        ViewBag.Routes = routes;

        model.Process();
        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpPost]
    public IActionResult Select(ActivityViewModel model)
    {
        model.Process();
        return RedirectToAction("Index", model);
    }

    [HttpPost]
    public async Task<IActionResult> Index(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return Content("Please select a file.");

        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "gpx");

        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        string filePath = Path.Combine(uploadsFolder, file.FileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return Content("File uploaded successfully.");
    }
}
