using Microsoft.EntityFrameworkCore;
using LocationMap.API.Data;
using LocationMap.API.Models;
using LocationMap.API.Repositories.Interfaces;

namespace LocationMap.API.Repositories;

public class MapUserRepository : IMapUserRepository
{
    private readonly AppDbContext _context;

    public MapUserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<MapUser?> GetByIdAsync(Guid id)
    {
        return await _context.MapUsers.FindAsync(id);
    }

    public async Task<MapUser?> GetByMelliCodeAsync(string melliCode)
    {
        return await _context.MapUsers.FirstOrDefaultAsync(u => u.MelliCode == melliCode);
    }

    public Task<MapUser> CreateAsync(MapUser user)
    {
        _context.MapUsers.Add(user);
        return Task.FromResult(user);
    }

    public Task UpdateAsync(MapUser user)
    {
        _context.MapUsers.Update(user);
        return Task.CompletedTask;
    }
}
