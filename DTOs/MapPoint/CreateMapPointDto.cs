using System.ComponentModel.DataAnnotations;

namespace LocationMap.API.DTOs.MapPoint;

public class CreateMapPointDto
{
    [Required(ErrorMessage = "عنوان الزامی است")]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    public Guid GuideId { get; set; }

    public Guid? SubmittedById { get; set; }

    public string? SubmittedByMelliCode { get; set; }
}
