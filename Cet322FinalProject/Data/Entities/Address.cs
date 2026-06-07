using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Cet322FinalProject.Data.Entities;

public class Address
{
    public string Id { get; set; }
    public string LocationName { get; set; }
    public float Latitude { get; set; }
    public float Longitude { get; set; }
}