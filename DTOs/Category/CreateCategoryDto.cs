using System.ComponentModel.DataAnnotations;

namespace LocationMap.API.DTOs.Category;

public class CreateCategoryDto
{
    [Required(ErrorMessage = "نام دسته‌بندی الزامی است")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Color { get; set; }

    public int SortOrder { get; set; } = 0;
}
