using LocationMap.API.Models;

namespace LocationMap.API.Repositories.Interfaces;

public interface IMapUserRepository
{
    Task<MapUser?> GetByIdAsync(Guid id);
    Task<MapUser?> GetByMelliCodeAsync(string melliCode);
    Task<MapUser> CreateAsync(MapUser user);
    Task UpdateAsync(MapUser user);
}
