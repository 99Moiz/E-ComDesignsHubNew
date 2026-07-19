namespace FursonaHub.API.Models;

public class WhatsAppInquiry
{
    public int Id { get; set; }
    public int? ProductId { get; set; }
    public string ProductSlug { get; set; } = string.Empty;
    public string MessageSent { get; set; } = string.Empty;
    public string IPAddress { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Product? Product { get; set; }
}
