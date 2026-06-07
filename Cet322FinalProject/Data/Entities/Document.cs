using Microsoft.EntityFrameworkCore;

namespace Cet322FinalProject.Data.Entities;

public class Document
{
    public string Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public string FilePath { get; set; }
    public DocumentType DocType { get; set; }
}

public enum DocumentType
{
    License = 0,
    Certificate = 1,
    DeliveryDocument = 2
}