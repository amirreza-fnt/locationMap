using LocationMap.API.DTOs.MapPoint;

namespace LocationMap.API.Services.Interfaces;

public interface IMapPointService
{
    Task<MapPointDetailDto?> GetByIdAsync(Guid id);
    Task<(IEnumerable<MapPointListDto> Items, int TotalCount)> GetApprovedAsync(int page, int pageSize, Guid? categoryId = null, List<Guid>? visibleCategoryIds = null);
    Task<IEnumerable<MapPointListDto>> GetByStatusAsync(int status, int page, int pageSize);
    Task<Guid> CreateAsync(CreateMapPointDto dto);
    Task<bool> UpdateAsync(Guid id, UpdateMapPointDto dto);
    Task<bool> ReviewAsync(Guid id, ReviewMapPointDto dto, Guid reviewerId);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> IncrementVisitCountAsync(Guid id);
}
