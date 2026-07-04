using Microsoft.Extensions.Caching.Memory;
using LocationMap.API.DTOs.MapPoint;
using LocationMap.API.Models;
using LocationMap.API.Models.Enums;
using LocationMap.API.Repositories.Interfaces;
using LocationMap.API.Services.Interfaces;

namespace LocationMap.API.Services;

public class MapPointService : IMapPointService
{
    private readonly IMapPointRepository _pointRepo;
    private readonly IMapUserRepository _userRepo;
    private readonly IMemoryCache _cache;
    private readonly ILogger<MapPointService> _logger;
    private const string ApprovedCacheKey = "approved_points";
    private const string PointCachePrefix = "point_";

    public MapPointService(
        IMapPointRepository pointRepo,
        IMapUserRepository userRepo,
        IMemoryCache cache,
        ILogger<MapPointService> logger)
    {
        _pointRepo = pointRepo;
        _userRepo = userRepo;
        _cache = cache;
        _logger = logger;
    }

    public async Task<MapPointDetailDto?> GetByIdAsync(Guid id)
    {
        var cacheKey = $"{PointCachePrefix}{id}";
        if (_cache.TryGetValue(cacheKey, out MapPointDetailDto? cached))
            return cached;

        var point = await _pointRepo.GetByIdAsync(id);
        if (point == null) return null;

        var dto = MapToDetailDto(point);

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
            .SetSlidingExpiration(TimeSpan.FromMinutes(2));

        _cache.Set(cacheKey, dto, cacheOptions);
        return dto;
    }

    public async Task<(IEnumerable<MapPointListDto> Items, int TotalCount)> GetApprovedAsync(
        int page, int pageSize, Guid? categoryId = null)
    {
        var cacheKey = $"{ApprovedCacheKey}_{page}_{pageSize}_{categoryId}";
        if (_cache.TryGetValue(cacheKey, out (IEnumerable<MapPointListDto> Items, int TotalCount) cached))
            return cached;

        var points = await _pointRepo.GetApprovedAsync(page, pageSize, categoryId);
        var totalCount = await _pointRepo.GetApprovedCountAsync(categoryId);

        var items = points.Select(MapToListDto).ToList();

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
            .SetSlidingExpiration(TimeSpan.FromMinutes(1));

        _cache.Set(cacheKey, (Items: (IEnumerable<MapPointListDto>)items, TotalCount: totalCount), cacheOptions);
        return (items, totalCount);
    }

    public async Task<IEnumerable<MapPointListDto>> GetByStatusAsync(int status, int page, int pageSize)
    {
        var pointStatus = (PointStatus)status;
        var points = await _pointRepo.GetByStatusAsync(pointStatus, page, pageSize);
        return points.Select(MapToListDto);
    }

    public async Task<Guid> CreateAsync(CreateMapPointDto dto)
    {
        MapUser? submitter = null;

        if (dto.SubmittedById.HasValue)
            submitter = await _userRepo.GetByIdAsync(dto.SubmittedById.Value);
        else if (!string.IsNullOrEmpty(dto.SubmittedByMelliCode))
            submitter = await _userRepo.GetByMelliCodeAsync(dto.SubmittedByMelliCode);

        if (submitter == null)
            throw new InvalidOperationException("کاربر یافت نشد. لطفاً ابتدا کاربر را ثبت کنید.");

        var point = new MapPoint
        {
            Title = dto.Title,
            Description = dto.Description,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            Address = dto.Address,
            CategoryId = dto.CategoryId,
            SubmittedById = submitter.Id,
            Status = PointStatus.Pending
        };

        await _pointRepo.CreateAsync(point);
        return point.Id;
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateMapPointDto dto)
    {
        var point = await _pointRepo.GetByIdAsync(id);
        if (point == null) return false;

        if (dto.Title != null) point.Title = dto.Title;
        if (dto.Description != null) point.Description = dto.Description;
        if (dto.Latitude.HasValue) point.Latitude = dto.Latitude.Value;
        if (dto.Longitude.HasValue) point.Longitude = dto.Longitude.Value;
        if (dto.Address != null) point.Address = dto.Address;
        if (dto.CategoryId.HasValue) point.CategoryId = dto.CategoryId.Value;

        await _pointRepo.UpdateAsync(point);
        InvalidatePointCache(id);
        return true;
    }

    public async Task<bool> ReviewAsync(Guid id, ReviewMapPointDto dto, Guid reviewerId)
    {
        var point = await _pointRepo.GetByIdAsync(id);
        if (point == null) return false;

        point.Status = dto.Status;
        point.ReviewedById = reviewerId;
        point.ReviewedAt = DateTime.UtcNow;
        point.ReviewNote = dto.ReviewNote;

        await _pointRepo.UpdateAsync(point);
        InvalidatePointCache(id);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var exists = await _pointRepo.ExistsAsync(id);
        if (!exists) return false;

        await _pointRepo.DeleteAsync(id);
        InvalidatePointCache(id);
        return true;
    }

    public async Task<bool> IncrementVisitCountAsync(Guid id)
    {
        var point = await _pointRepo.GetByIdAsync(id);
        if (point == null) return false;

        point.VisitCount++;
        await _pointRepo.UpdateAsync(point);
        InvalidatePointCache(id);
        return true;
    }

    private void InvalidatePointCache(Guid id)
    {
        _cache.Remove($"{PointCachePrefix}{id}");
        _cache.Remove(ApprovedCacheKey);
    }

    private static MapPointListDto MapToListDto(MapPoint point)
    {
        return new MapPointListDto
        {
            Id = point.Id,
            Title = point.Title,
            Description = point.Description,
            Latitude = point.Latitude,
            Longitude = point.Longitude,
            Address = point.Address,
            CategoryName = point.Category?.Name ?? "",
            CategoryIcon = point.Category?.Icon,
            CategoryColor = point.Category?.Color,
            Status = point.Status,
            SubmittedByName = point.SubmittedBy?.FullName ?? "",
            SubmittedAt = point.SubmittedAt,
            MainImageUrl = point.Media?.FirstOrDefault(m => m.IsMain)?.FileUrl,
            VisitCount = point.VisitCount
        };
    }

    private static MapPointDetailDto MapToDetailDto(MapPoint point)
    {
        return new MapPointDetailDto
        {
            Id = point.Id,
            Title = point.Title,
            Description = point.Description,
            Latitude = point.Latitude,
            Longitude = point.Longitude,
            Address = point.Address,
            CategoryId = point.CategoryId,
            CategoryName = point.Category?.Name ?? "",
            CategoryIcon = point.Category?.Icon,
            CategoryColor = point.Category?.Color,
            Status = point.Status,
            SubmittedByName = point.SubmittedBy?.FullName ?? "",
            SubmittedByMelliCode = point.SubmittedBy?.MelliCode,
            SubmittedAt = point.SubmittedAt,
            ReviewedByName = point.ReviewedBy?.FullName,
            ReviewedAt = point.ReviewedAt,
            ReviewNote = point.ReviewNote,
            UpdatedAt = point.UpdatedAt,
            VisitCount = point.VisitCount,
            Media = point.Media?.Select(m => new MediaDto
            {
                Id = m.Id,
                FileUrl = m.FileUrl,
                FileType = m.FileType,
                IsMain = m.IsMain
            }).ToList() ?? new()
        };
    }
}
