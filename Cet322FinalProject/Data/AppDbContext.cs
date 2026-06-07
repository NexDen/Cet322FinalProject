using Cet322FinalProject.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : IdentityDbContext<Admin>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Address>           Addresses { get; set; }
    public DbSet<Company>           Companies { get; set; }
    public DbSet<Document>          Documents { get; set; }
    public DbSet<Driver>            Drivers { get; set; }
    public DbSet<TransportationJob> TransportationJobs { get; set; }
    public DbSet<Vehicle>           Vehicles { get; set; }
}
