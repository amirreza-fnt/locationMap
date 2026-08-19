namespace LocationMap.API.Services.Interfaces;

public interface IShortLinkClient
{
    Task<string?> CreateShortUrlAsync(string longUrl, CancellationToken ct = default);
}
