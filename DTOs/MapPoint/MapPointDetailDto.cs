using LocationMap.API.Models.Enums;

namespace LocationMap.API.DTOs.MapPoint;

public class MapPointDetailDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string? Address { get; set; }
    public Guid CategoryId { get; set; }
    public Guid GuideId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? CategoryIcon { get; set; }
    public string? GuideIcon { get; set; }
    public string? CategoryColor { get; set; }
    public PointStatus Status { get; set; }
    public string SubmittedByName { get; set; } = string.Empty;
    public string? SubmittedByMelliCode { get; set; }
    public DateTime SubmittedAt { get; set; }
    public string? ReviewedByName { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNote { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long VisitCount { get; set; }
    public List<MediaDto> Media { get; set; } = new();
}

public class MediaDto
{
    public Guid Id { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string? FileType { get; set; }
    public bool IsMain { get; set; }
}
