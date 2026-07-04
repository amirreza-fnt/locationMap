using Microsoft.AspNetCore.Mvc;
using LocationMap.API.DTOs.Category;
using LocationMap.API.Services.Interfaces;

namespace LocationMap.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoryController> _logger;

    public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var categories = await _categoryService.GetAllActiveAsync();
            return Ok(new { success = true, data = categories });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories");
            return StatusCode(500, new { success = false, message = "خطا در دریافت دسته‌بندی‌ها" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
    {
        try
        {
            var id = await _categoryService.CreateAsync(dto);
            return Ok(new { success = true, message = "دسته‌بندی با موفقیت ایجاد شد", id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return StatusCode(500, new { success = false, message = "خطا در ایجاد دسته‌بندی" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateCategoryDto dto)
    {
        try
        {
            var result = await _categoryService.UpdateAsync(id, dto);
            if (!result)
                return NotFound(new { success = false, message = "دسته‌بندی یافت نشد" });

            return Ok(new { success = true, message = "دسته‌بندی با موفقیت به‌روزرسانی شد" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category {Id}", id);
            return StatusCode(500, new { success = false, message = "خطا در به‌روزرسانی دسته‌بندی" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _categoryService.DeleteAsync(id);
            if (!result)
                return NotFound(new { success = false, message = "دسته‌بندی یافت نشد" });

            return Ok(new { success = true, message = "دسته‌بندی با موفقیت حذف شد" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category {Id}", id);
            return StatusCode(500, new { success = false, message = "خطا در حذف دسته‌بندی" });
        }
    }
}
