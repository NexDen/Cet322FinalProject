using Microsoft.AspNetCore.Identity;

namespace Cet322FinalProject.Data.Entities;

public class Admin : IdentityUser
{
    public Company Company { get; set; }
    public string CompanyId { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
}
