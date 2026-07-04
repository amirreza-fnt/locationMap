using Microsoft.AspNetCore.Mvc;
using LocationMap.API.Models;
using LocationMap.API.Models.Enums;
using LocationMap.API.Repositories.Interfaces;

namespace LocationMap.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MapUserController : ControllerBase
{
    private readonly IMapUserRepository _userRepo;
    private readonly ILogger<MapUserController> _logger;

    public MapUserController(IMapUserRepository userRepo, ILogger<MapUserController> logger)
    {
        _userRepo = userRepo;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
                return BadRequest(new { success = false, message = "نام الزامی است" });

            var existingUser = string.IsNullOrEmpty(request.MelliCode)
                ? null
                : await _userRepo.GetByMelliCodeAsync(request.MelliCode);

            if (existingUser != null)
                return Ok(new { success = true, message = "کاربر قبلاً ثبت شده است", id = existingUser.Id });

            var user = new MapUser
            {
                MelliCode = request.MelliCode,
                FullName = request.FullName,
                Phone = request.Phone,
                AccessLevel = request.AccessLevel
            };

            await _userRepo.CreateAsync(user);

            _logger.LogInformation("MapUser {Id} created with access level {Level}", user.Id, user.AccessLevel);
            return Ok(new { success = true, message = "کاربر با موفقیت ثبت شد", id = user.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, new { success = false, message = "خطا در ثبت کاربر" });
        }
    }

    [HttpGet("by-melli/{melliCode}")]
    public async Task<IActionResult> GetByMelliCode(string melliCode)
    {
        try
        {
            var user = await _userRepo.GetByMelliCodeAsync(melliCode);
            if (user == null)
                return NotFound(new { success = false, message = "کاربر یافت نشد" });

            return Ok(new { success = true, data = new { user.Id, user.FullName, user.MelliCode, user.Phone, AccessLevel = user.AccessLevel.ToString() } });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by melli code");
            return StatusCode(500, new { success = false, message = "خطا" });
        }
    }
}

public class CreateUserRequest
{
    public string? MelliCode { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public AccessLevel AccessLevel { get; set; } = AccessLevel.Citizen;
}
