using Microsoft.EntityFrameworkCore;
using LocationMap.API.Data;
using LocationMap.API.DTOs.Category;
using LocationMap.API.Models;
using LocationMap.API.Repositories.Interfaces;
using LocationMap.API.Services.Interfaces;

namespace LocationMap.API.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepo;
    private readonly AppDbContext _context;

    public CategoryService(ICategoryRepository categoryRepo, AppDbContext context)
    {
        _categoryRepo = categoryRepo;
        _context = context;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllActiveAsync()
    {
        var categories = await _categoryRepo.GetAllActiveAsync();
        var pointCounts = await _context.MapPoints
            .Where(m => m.Status == Models.Enums.PointStatus.Approved)
            .GroupBy(m => m.CategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Count);

        return categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Color = c.Color,
            SortOrder = c.SortOrder,
            PointCount = pointCounts.GetValueOrDefault(c.Id, 0)
        });
    }

    public async Task<Guid> CreateAsync(CreateCategoryDto dto)
    {
        var category = new Category
        {
            Name = dto.Name,
            Color = dto.Color,
            SortOrder = dto.SortOrder
        };

        await _categoryRepo.CreateAsync(category);
        return category.Id;
    }

    public async Task<bool> UpdateAsync(Guid id, CreateCategoryDto dto)
    {
        var category = await _categoryRepo.GetByIdAsync(id);
        if (category == null) return false;

        category.Name = dto.Name;
        category.Color = dto.Color;
        category.SortOrder = dto.SortOrder;

        await _categoryRepo.UpdateAsync(category);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var category = await _categoryRepo.GetByIdAsync(id);
        if (category == null) return false;

        await _categoryRepo.DeleteAsync(id);
        return true;
    }
}
