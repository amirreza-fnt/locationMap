using Microsoft.EntityFrameworkCore;
using LocationMap.API.Data;
using LocationMap.API.Models;
using LocationMap.API.Models.Enums;
using LocationMap.API.Repositories.Interfaces;

namespace LocationMap.API.Repositories;

public class MapPointRepository : IMapPointRepository
{
    private readonly AppDbContext _context;

    public MapPointRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<MapPoint?> GetByIdAsync(Guid id)
    {
        return await _context.MapPoints
            .Include(m => m.Category)
            .Include(m => m.SubmittedBy)
            .Include(m => m.ReviewedBy)
            .Include(m => m.Media)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<IEnumerable<MapPoint>> GetApprovedAsync(int page, int pageSize, Guid? categoryId = null)
    {
        var query = _context.MapPoints
            .Include(m => m.Category)
            .Include(m => m.SubmittedBy)
            .Include(m => m.Media.Where(md => md.IsMain))
            .Where(m => m.Status == PointStatus.Approved)
            .OrderByDescending(m => m.SubmittedAt);

        if (categoryId.HasValue)
            query = (IOrderedQueryable<MapPoint>)query.Where(m => m.CategoryId == categoryId.Value);

        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<MapPoint>> GetByStatusAsync(PointStatus status, int page, int pageSize)
    {
        return await _context.MapPoints
            .Include(m => m.Category)
            .Include(m => m.SubmittedBy)
            .Where(m => m.Status == status)
            .OrderByDescending(m => m.SubmittedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<MapPoint>> GetBySubmitterAsync(Guid submitterId)
    {
        return await _context.MapPoints
            .Include(m => m.Category)
            .Where(m => m.SubmittedById == submitterId)
            .OrderByDescending(m => m.SubmittedAt)
            .ToListAsync();
    }

    public async Task<int> GetApprovedCountAsync(Guid? categoryId = null)
    {
        var query = _context.MapPoints.Where(m => m.Status == PointStatus.Approved);
        if (categoryId.HasValue)
            query = query.Where(m => m.CategoryId == categoryId.Value);
        return await query.CountAsync();
    }

    public Task<MapPoint> CreateAsync(MapPoint point)
    {
        point.SubmittedAt = DateTime.UtcNow;
        _context.MapPoints.Add(point);
        return Task.FromResult(point);
    }

    public Task UpdateAsync(MapPoint point)
    {
        point.UpdatedAt = DateTime.UtcNow;
        _context.MapPoints.Update(point);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var point = await _context.MapPoints.FindAsync(id);
        if (point != null)
            _context.MapPoints.Remove(point);
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.MapPoints.AnyAsync(m => m.Id == id);
    }
}
