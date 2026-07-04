using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LocationMap.API.Models.Enums;

namespace LocationMap.API.Models;

[Table("MapUsers")]
public class MapUser
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(20)]
    public string? MelliCode { get; set; }

    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(15)]
    public string? Phone { get; set; }

    public AccessLevel AccessLevel { get; set; } = AccessLevel.Citizen;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<MapPoint> SubmittedPoints { get; set; } = new List<MapPoint>();
    public virtual ICollection<MapPoint> ReviewedPoints { get; set; } = new List<MapPoint>();
}
