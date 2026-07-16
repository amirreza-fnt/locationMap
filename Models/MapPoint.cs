using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LocationMap.API.Models.Enums;

namespace LocationMap.API.Models;

[Table("MapPoints")]
public class MapPoint
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Column(TypeName = "decimal(10,8)")]
    public decimal Latitude { get; set; }

    [Column(TypeName = "decimal(11,8)")]
    public decimal Longitude { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    public Guid CategoryId { get; set; }

    public Guid GuideId { get; set; }

    public PointStatus Status { get; set; } = PointStatus.Pending;

    public Guid SubmittedById { get; set; }

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public Guid? ReviewedById { get; set; }

    public DateTime? ReviewedAt { get; set; }

    [MaxLength(500)]
    public string? ReviewNote { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public long VisitCount { get; set; } = 0;

    [ForeignKey(nameof(CategoryId))]
    public virtual Category Category { get; set; } = null!;

    [ForeignKey(nameof(GuideId))]
    public virtual Guide Guide { get; set; } = null!;

    [ForeignKey(nameof(SubmittedById))]
    public virtual MapUser SubmittedBy { get; set; } = null!;

    [ForeignKey(nameof(ReviewedById))]
    public virtual MapUser? ReviewedBy { get; set; }

    public virtual ICollection<MapPointMedia> Media { get; set; } = new List<MapPointMedia>();
}
