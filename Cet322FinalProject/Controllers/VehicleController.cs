using Cet322FinalProject.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cet322FinalProject.Controllers;

[Authorize]
public class VehicleController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<Admin> _userManager;
    private readonly IWebHostEnvironment _env;

    public VehicleController(AppDbContext context, UserManager<Admin> userManager, IWebHostEnvironment env)
    {
        _context = context;
        _userManager = userManager;
        _env = env;
    }

    public async Task<IActionResult> Index()
    {
        var admin = await _userManager.GetUserAsync(User);
        var vehicles = await _context.Vehicles
            .Where(v => v.CompanyId == admin!.CompanyId)
            .ToListAsync();
        return View(vehicles);
    }

    public IActionResult Create() => View(new Vehicle());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("PlateNo,Model,ProductionYear,CertificationFile")] Vehicle vehicle)
    {
        var admin = await _userManager.GetUserAsync(User);

        var cert = new Document
        {
            Id = Guid.NewGuid().ToString(),
            CreatedDate = DateTime.UtcNow,
            FilePath = await SaveFile(vehicle.CertificationFile),
            DocType = DocumentType.Certificate
        };
        _context.Documents.Add(cert);

        vehicle.Id = Guid.NewGuid().ToString();
        vehicle.CompanyId = admin!.CompanyId;
        vehicle.VehicleCertificationId = cert.Id;

        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string id)
    {
        var vehicle = await _context.Vehicles.FindAsync(id);
        if (vehicle == null) return NotFound();
        return View(vehicle);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([Bind("Id,PlateNo,Model,ProductionYear,CertificationFile")] Vehicle vehicle)
    {
        var existing = await _context.Vehicles.FindAsync(vehicle.Id);
        if (existing == null) return NotFound();

        if (vehicle.PlateNo != null) existing.PlateNo = vehicle.PlateNo;
        if (vehicle.Model != null) existing.Model = vehicle.Model;
        if (vehicle.ProductionYear != null) existing.ProductionYear = vehicle.ProductionYear;

        if (vehicle.CertificationFile != null)
        {
            var cert = new Document
            {
                Id = Guid.NewGuid().ToString(),
                CreatedDate = DateTime.UtcNow,
                FilePath = await SaveFile(vehicle.CertificationFile),
                DocType = DocumentType.Certificate
            };
            _context.Documents.Add(cert);
            existing.VehicleCertificationId = cert.Id;
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(string id)
    {
        var vehicle = await _context.Vehicles
            .Include(v => v.VehicleCertification)
            .FirstOrDefaultAsync(v => v.Id == id);
        if (vehicle == null) return NotFound();
        return View(vehicle);
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