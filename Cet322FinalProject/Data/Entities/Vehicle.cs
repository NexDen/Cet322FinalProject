using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace Cet322FinalProject.Data.Entities;

public class Vehicle
{
    public string Id { get; set; }
    public Company? Company { get; set; }
    public string? CompanyId { get; set; }
    public string? PlateNo { get; set; }
    public string? Model { get; set; }
    public string? ProductionYear { get; set; }
    public Document? VehicleCertification { get; set; }
    public string? VehicleCertificationId { get; set; }

    [NotMapped] public IFormFile? CertificationFile { get; set; }
}