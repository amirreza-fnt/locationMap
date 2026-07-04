using LocationMap.API.DTOs.Category;

namespace LocationMap.API.Services.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllActiveAsync();
    Task<Guid> CreateAsync(CreateCategoryDto dto);
    Task<bool> UpdateAsync(Guid id, CreateCategoryDto dto);
    Task<bool> DeleteAsync(Guid id);
}
