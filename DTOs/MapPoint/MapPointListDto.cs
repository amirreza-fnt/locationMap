using LocationMap.API.Models.Enums;

namespace LocationMap.API.DTOs.MapPoint;

public class MapPointListDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string? Address { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? CategoryIcon { get; set; }
    public string? GuideIcon { get; set; }
    public string? CategoryColor { get; set; }
    public PointStatus Status { get; set; }
    public string SubmittedByName { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public string? MainImageUrl { get; set; }
    public long VisitCount { get; set; }
    public string VisitLink { get; set; } = string.Empty;
    public string? ShortVisitLink { get; set; }
}
