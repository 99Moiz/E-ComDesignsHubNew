using System.ComponentModel.DataAnnotations;

namespace FursonaHub.API.DTOs;

public class WhatsAppInquiryRequest
{
    [MaxLength(100)]
    public string? ProductSlug { get; set; }
}

public record WhatsAppInquiryResponse(string Link, string Message);
