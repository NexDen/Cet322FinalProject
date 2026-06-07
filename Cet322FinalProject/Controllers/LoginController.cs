using Cet322FinalProject.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cet322FinalProject.Controllers;

public class LoginController : Controller
{
    private readonly SignInManager<Admin> _signInManager;
    private readonly UserManager<Admin> _userManager;
    private readonly AppDbContext _db;

    public LoginController(SignInManager<Admin> signInManager, UserManager<Admin> userManager, AppDbContext db)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _db = db;
    }

    [HttpGet]
    public IActionResult Index() => View();

    [HttpPost]
    public async Task<IActionResult> Login(string username, string password, bool rememberMe = false)
    {
        var result = await _signInManager.PasswordSignInAsync(username, password, rememberMe, lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            ViewBag.Error = "Invalid username or password.";
            return View("Index");
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public async Task<IActionResult> Register()
    {
        ViewBag.Companies = await _db.Companies.ToListAsync();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(string username, string email, string name, string surname, string companyId, string password, string confirmPassword)
    {
        if (password != confirmPassword)
        {
            ViewBag.Error = "Passwords do not match.";
            ViewBag.Companies = await _db.Companies.ToListAsync();
            return View();
        }

        var admin = new Admin
        {
            UserName = username,
            Email = email,
            Name = name,
            Surname = surname,
            CompanyId = companyId,
        };

        var result = await _userManager.CreateAsync(admin, password);

        if (!result.Succeeded)
        {
            ViewBag.Error = string.Join(" ", result.Errors.Select(e => e.Description));
            ViewBag.Companies = await _db.Companies.ToListAsync();
            return View();
        }

        await _signInManager.SignInAsync(admin, isPersistent: false);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [Route("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index");
    }
}
