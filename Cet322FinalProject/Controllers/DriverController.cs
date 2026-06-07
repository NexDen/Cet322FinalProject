using Cet322FinalProject.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cet322FinalProject.Controllers;

[Authorize]
public class DriverController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<Admin> _userManager;
    private readonly IWebHostEnvironment _env;

    public DriverController(AppDbContext context, UserManager<Admin> userManager, IWebHostEnvironment env)
    {
        _context = context;
        _userManager = userManager;
        _env = env;
    }

    public async Task<IActionResult> Index()
    {
        var admin = await _userManager.GetUserAsync(User);
        var drivers = await _context.Drivers
            .Where(d => d.CompanyId == admin!.CompanyId)
            .ToListAsync();
        return View(drivers);
    }

    public IActionResult Create() => View(new Driver());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Surname,Email,Password,LicenseFile")] Driver model)
    {
        var admin = await _userManager.GetUserAsync(User);

        var license = new Document
        {
            Id = Guid.NewGuid().ToString(),
            CreatedDate = DateTime.UtcNow,
            FilePath = await SaveFile(model.LicenseFile),
            DocType = DocumentType.License
        };
        _context.Documents.Add(license);

        model.Id = Guid.NewGuid().ToString();
        model.CompanyId = admin!.CompanyId;
        model.LicenseId = license.Id;

        _context.Drivers.Add(model);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
        
    }

    public async Task<IActionResult> Edit(string id)
    {
        var driver = await _context.Drivers.FindAsync(id);
        if (driver == null) return NotFound();
        return View(driver);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([Bind("Id,Name,Surname,Email,LicenseFile")] Driver model)
    {
        var driver = await _context.Drivers.FindAsync(model.Id);
        if (driver == null) return NotFound();

        if (model.Name != null) driver.Name = model.Name;
        if (model.Surname != null) driver.Surname = model.Surname;

        if (model.LicenseFile != null)
        {
            var license = new Document
            {
                Id = Guid.NewGuid().ToString(),
                CreatedDate = DateTime.UtcNow,
                FilePath = await SaveFile(model.LicenseFile),
                DocType = DocumentType.License
            };
            _context.Documents.Add(license);
            driver.LicenseId = license.Id;
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(string id)
    {
        var driver = await _context.Drivers
            .Include(d => d.License)
            .FirstOrDefaultAsync(d => d.Id == id);
        if (driver == null) return NotFound();
        return View(driver);
    }

    private async Task<string> SaveFile(IFormFile? file)
    {
        if (file == null) return string.Empty;
        var dir = Path.Combine(_env.WebRootPath, "uploads");
        Directory.CreateDirectory(dir);
        var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
        await using var stream = new FileStream(Path.Combine(dir, fileName), FileMode.Create);
        await file.CopyToAsync(stream);
        return "/uploads/" + fileName;
    }
}