using LocationMap.API.Models;
using LocationMap.API.Models.Enums;

namespace LocationMap.API.Repositories.Interfaces;

public interface IMapPointRepository
{
    Task<MapPoint?> GetByIdAsync(Guid id);
    Task<IEnumerable<MapPoint>> GetApprovedAsync(int page, int pageSize, Guid? categoryId = null);
    Task<IEnumerable<MapPoint>> GetByStatusAsync(PointStatus status, int page, int pageSize);
    Task<IEnumerable<MapPoint>> GetBySubmitterAsync(Guid submitterId);
    Task<int> GetApprovedCountAsync(Guid? categoryId = null);
    Task<MapPoint> CreateAsync(MapPoint point);
    Task UpdateAsync(MapPoint point);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}
