using System.ComponentModel.DataAnnotations;
using LocationMap.API.Models.Enums;

namespace LocationMap.API.DTOs.MapPoint;

public class ReviewMapPointDto
{
    [Required]
    public PointStatus Status { get; set; }

    [MaxLength(500)]
    public string? ReviewNote { get; set; }
}
