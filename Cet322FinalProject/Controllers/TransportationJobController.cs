using Cet322FinalProject.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Cet322FinalProject.Controllers;

[Authorize]
public class TransportationJobController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<Admin> _userManager;
    private readonly IWebHostEnvironment _env;

    public TransportationJobController(AppDbContext context, UserManager<Admin> userManager, IWebHostEnvironment env)
    {
        _context = context;
        _userManager = userManager;
        _env = env;
    }

    public async Task<IActionResult> Index()
    {
        var admin = await _userManager.GetUserAsync(User);
        var jobs = await _context.TransportationJobs
            .Include(j => j.Driver)
            .Include(j => j.Vehicle)
            .Where(j => j.Driver.CompanyId == admin!.CompanyId)
            .ToListAsync();
        return View(jobs);
    }

    public async Task<IActionResult> Create()
    {
        var admin = await _userManager.GetUserAsync(User);
        await PopulateDropdowns(admin!.CompanyId);
        return View(new TransportationJob { OrderDate = DateTime.Today, DeliveryDate = DateTime.Today.AddDays(1) });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("OrderDate,DeliveryDate,DriverId,VehicleId,CommodityType,Tonnage,SaleCost,LoadingAddress,UnloadingAddress")] TransportationJob model,
        IFormFile? deliveryDocumentFile)
    {
        var admin = await _userManager.GetUserAsync(User);

        var doc = new Document
        {
            Id = Guid.NewGuid().ToString(),
            CreatedDate = DateTime.Now.ToUniversalTime(),
            FilePath = await SaveFile(deliveryDocumentFile),
            DocType = DocumentType.DeliveryDocument
        };
        _context.Documents.Add(doc);

        model.Id = Guid.NewGuid().ToString();
        model.CreatedBy = admin!;
        model.CreatedDate = DateTime.Now.ToUniversalTime();
        model.DeliveryDocument = doc;
        model.IsActive = true;

        _context.TransportationJobs.Add(model);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string id)
    {
        var job = await _context.TransportationJobs.FindAsync(id);
        if (job == null) return NotFound();

        var admin = await _userManager.GetUserAsync(User);
        await PopulateDropdowns(admin!.CompanyId);
        return View(job);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        [Bind("Id,OrderDate,DeliveryDate,DriverId,VehicleId,CommodityType,Tonnage,SaleCost,LoadingAddress,UnloadingAddress,IsActive")] TransportationJob model)
    {
        var job = await _context.TransportationJobs.FindAsync(model.Id);
        if (job == null) return NotFound();

        if (model.OrderDate != null) job.OrderDate = model.OrderDate;
        if (model.DeliveryDate != null) job.DeliveryDate = model.DeliveryDate;
        if (model.DriverId != null) job.DriverId = model.DriverId;
        if (model.VehicleId != null) job.VehicleId = model.VehicleId;
        if (model.CommodityType != null) job.CommodityType = model.CommodityType;
        if (model.Tonnage != null) job.Tonnage = model.Tonnage;
        if (model.SaleCost != null) job.SaleCost = model.SaleCost;
        if (model.LoadingAddress != null) job.LoadingAddress = model.LoadingAddress;
        if (model.UnloadingAddress != null) job.UnloadingAddress = model.UnloadingAddress;
        job.IsActive = model.IsActive;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(string id)
    {
        var job = await _context.TransportationJobs
            .Include(j => j.Driver)
            .Include(j => j.Vehicle)
            .Include(j => j.DeliveryDocument)
            .Include(j => j.CreatedBy)
            .FirstOrDefaultAsync(j => j.Id == id);
        if (job == null) return NotFound();
        return View(job);
    }


    private async Task PopulateDropdowns(string companyId)
    {
        ViewBag.Drivers = await _context.Drivers
            .Where(d => d.CompanyId == companyId)
            .Select(d => new SelectListItem(d.Name + " " + d.Surname, d.Id))
            .ToListAsync();

        ViewBag.Vehicles = await _context.Vehicles
            .Where(v => v.CompanyId == companyId)
            .Select(v => new SelectListItem(v.PlateNo + " — " + v.Model, v.Id))
            .ToListAsync();

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
