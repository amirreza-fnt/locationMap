using Map.Shared.Auth.Authorization;
using Map.Shared.Auth.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LocationMap.API.DTOs.MapPoint;
using LocationMap.API.Services.Interfaces;

namespace LocationMap.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MapPointController : ControllerBase
{
    private readonly IMapPointService _mapPointService;
    private readonly ILogger<MapPointController> _logger;

    public MapPointController(IMapPointService mapPointService, ILogger<MapPointController> logger)
    {
        _mapPointService = mapPointService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetApproved([FromQuery] int page = 1, [FromQuery] int pageSize = 50, [FromQuery] Guid? categoryId = null)
    {
        try
        {
            var (items, totalCount) = await _mapPointService.GetApprovedAsync(page, pageSize, categoryId);
            return Ok(new
            {
                success = true,
                data = items,
                totalCount,
                page,
                pageSize
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting approved points");
            return StatusCode(500, new { success = false, message = "خطا در دریافت نقاط" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var point = await _mapPointService.GetByIdAsync(id);
            if (point == null)
                return NotFound(new { success = false, message = "نقطه یافت نشد" });

            await _mapPointService.IncrementVisitCountAsync(id);

            return Ok(new { success = true, data = point });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting point {Id}", id);
            return StatusCode(500, new { success = false, message = "خطا در دریافت نقطه" });
        }
    }

    [HttpPost]
    [HasPermission(PermissionConstants.PointCreate)]
    public async Task<IActionResult> Create([FromBody] CreateMapPointDto dto)
    {
        try
        {
            var id = await _mapPointService.CreateAsync(dto);
            _logger.LogInformation("MapPoint {Id} created", id);
            return Ok(new { success = true, message = "نقطه با موفقیت ثبت شد. پس از تایید نمایش داده می‌شود.", id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating map point");
            return StatusCode(500, new { success = false, message = "خطا در ثبت نقطه" });
        }
    }

    [HttpPut("{id}")]
    [HasPermission(PermissionConstants.PointUpdate)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMapPointDto dto)
    {
        try
        {
            var result = await _mapPointService.UpdateAsync(id, dto);
            if (!result)
                return NotFound(new { success = false, message = "نقطه یافت نشد" });

            return Ok(new { success = true, message = "نقطه با موفقیت به‌روزرسانی شد" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating point {Id}", id);
            return StatusCode(500, new { success = false, message = "خطا در به‌روزرسانی نقطه" });
        }
    }

    [HttpPut("{id}/review")]
    [HasPermission(PermissionConstants.PointReview)]
    public async Task<IActionResult> Review(Guid id, [FromBody] ReviewMapPointDto dto)
    {
        try
        {
            var reviewerId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var result = await _mapPointService.ReviewAsync(id, dto, reviewerId);
            if (!result)
                return NotFound(new { success = false, message = "نقطه یافت نشد" });

            var statusText = dto.Status == Models.Enums.PointStatus.Approved ? "تایید" : "رد";
            return Ok(new { success = true, message = $"نقطه با موفقیت {statusText} شد" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reviewing point {Id}", id);
            return StatusCode(500, new { success = false, message = "خطا در بررسی نقطه" });
        }
    }

    [HttpDelete("{id}")]
    [HasPermission(PermissionConstants.PointDelete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _mapPointService.DeleteAsync(id);
            if (!result)
                return NotFound(new { success = false, message = "نقطه یافت نشد" });

            return Ok(new { success = true, message = "نقطه با موفقیت حذف شد" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting point {Id}", id);
            return StatusCode(500, new { success = false, message = "خطا در حذف نقطه" });
        }
    }
}
