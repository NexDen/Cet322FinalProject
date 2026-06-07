using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace Cet322FinalProject.Data.Entities;

public class Driver
{
    public string Id { get; set; }
    public Company? Company { get; set; }
    public string? CompanyId { get; set; }
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public Document? License { get; set; }
    public string? LicenseId { get; set; }
    [NotMapped] public IFormFile? LicenseFile { get; set; }
}